#nullable enable
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using System.Text.Json;
using System.Text;
using Ganss.Xss;
using System.Text.RegularExpressions;

namespace JobAggregator.Api.Services.Implementations
{
    public class GptJobParser : IGptJobParser
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GptJobParser> _logger;

        public GptJobParser(IConfiguration configuration, ILogger<GptJobParser> logger)
        {
            _httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) };
            _apiKey = configuration["GptSettings:ApiKey"] ?? throw new ArgumentNullException("GptSettings:ApiKey not found");
            _logger = logger;
        }

        /// <summary>
        /// Parses job posting data from raw content by generating a prompt, calling the GPT API and deserializing response into a JobPostingData.
        /// </summary>
        /// <param name="content">raw job posting content.</param>
        /// <param name="jobUrl">URL of the job posting.</param>
        /// <param name="portalName">name of the portal.</param>
        /// <param name="portalUrl">URL of the portal.</param>
        /// <returns>JobPostingData object if successful; otherwise, null.</returns>
        public async Task<JobPostingData?> ParseJobAsync(string content, string jobUrl, string portalName, string portalUrl)
        {
            var prompt = GenerateGeneralPrompt(content, portalName, portalUrl);
            var gptResponse = await CallGptApiAsync(prompt);
            if (string.IsNullOrEmpty(gptResponse))
            {
                _logger.LogWarning("GPT failed to parse job at URL: {JobUrl}", jobUrl);
                return null;
            }
            var jobData = DeserializeGptResponse(gptResponse);
            if (jobData == null)
            {
                _logger.LogWarning("Failed to deserialize GPT response for job at URL: {JobUrl}", jobUrl);
                return null;
            }
            jobData.CompanyName = CleanValue(jobData.CompanyName) ?? portalName;
            jobData.Location = CleanValue(jobData.Location);
            jobData.Description = CleanValue(jobData.Description);
            jobData.Description = SanitizeDescription(jobData.Description);
            jobData.Location = MapLocation(jobData.Location);
            jobData.ApplyUrl = jobUrl;
            return jobData;
        }

        private static string GenerateGeneralPrompt(string content, string portalName, string portalUrl)
        {
            return $@"
            EXTREMELY STRICT INSTRUCTIONS:
            You are a JSON data generator. Analyze the job posting and extract ONLY EXPLICITLY STATED INFORMATION.
            YOUR OUTPUT MUST BE VALID JSON WITHOUT ANY DECORATIONS OR MARKDOWN.

            Critical requirements:
            1. STRICT JSON SYNTAX:
               - No Markdown (```json or ```)
               - No unescaped quotes in content
               - No trailing commas
               - Always quote field names and string values
               - Use \"" for quotes inside strings
               - Ensure HTML is properly escaped
            2. STRUCTURAL PRIORITY:
               - Complete JSON structure takes priority over content length
               - If approaching token limit, truncate Description content FIRST
               - Never truncate closing brackets/braces
            3. ERROR PREVENTION:
               - Validate JSON with JSONLint before finalizing
               - Test for parseability programmatically
               - If unsure about any field, use null

            Output template:
            {{
                ""Title"": ""Original Title"",
                ""CompanyName"": ""Exact Name or '{portalName}'"",
                ""Location"": ""City, Kraj"",
                ""Description"": ""HTML content with escaped quotes"",
                ""SalaryFrom"": number|null,
                ""SalaryTo"": number|null,
                ""JobType"": ""Full-time/Part-time/etc""
            }}

            Processing rules:
            1. Content preservation:
               - Maintain original HTML tags in Description - remove buttons, links, etc.
               - Make it cleaner - don't change content just html formating.
               - Convert <br> to <br/>
               - Escape double quotes with \u0022
               - Preserve Czech diacritics
            2. Salary handling:
               - 35-50k → 35000,50000
               - od 45k → 45000,null
               - 500 Kč/hod → null,null
            3. Validation checklist before output:
                - No Markdown formatting
                - All strings properly quoted
                - No special characters in keys
                - HTML properly escaped
                - Final closing }} present

            FAILURE EXAMPLES TO AVOID:
             ```json{{...}}``` 
             Unclosed HTML tags
             Unescaped quotes in Description
             Missing final brace
             Don't use code block or other formathing just plain text in json format

            CONTENT TO PROCESS:
            {content}
            ";
        }

        /// <summary>
        /// Calls GPT API with specified prompt and returns trimmed response content.
        /// </summary>
        /// <param name="prompt">prompt to send to the GPT API.</param>
        /// <returns>trimmed GPT API response content or null.</returns>
        private async Task<string?> CallGptApiAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a data extraction assistant." },
                    new { role = "user", content = prompt },
                },
                max_tokens = 3000,
                temperature = 0.2,
                n = 1,
            };
            var requestBodyJson = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("Request body JSON: {RequestBodyJson}", requestBodyJson);
            var contentData = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", contentData);
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("GPT API response: {ResponseString}", responseString);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GPT API call failed with status code {StatusCode}. Response: {ResponseString}", response.StatusCode, responseString);
                    return null;
                }
                var gptApiResponse = JsonSerializer.Deserialize<GptApiResponse>(responseString);
                if (gptApiResponse?.Choices?[0]?.FinishReason == "length")
                {
                    _logger.LogWarning("GPT API response finish_reason indicates truncation ('length').");
                }
                return gptApiResponse?.Choices?[0]?.Message?.Content?.Trim();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("GPT API request timed out after 3 minutes: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calling GPT API: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Cleans and deserialize GPT API response into a JobPostingData object.
        /// </summary>
        /// <param name="gptResponse">raw GPT API response</param>
        /// <returns>A JobPostingData object or null.</returns>
        private JobPostingData? DeserializeGptResponse(string gptResponse)
        {
            try
            {
                string cleanedResponse = CleanGptResponse(gptResponse);
                if (!IsJsonBalanced(cleanedResponse))
                {
                    _logger.LogWarning("Detected unbalanced JSON response. Attempting to fix by appending missing braces.");
                    cleanedResponse = AppendMissingBraces(cleanedResponse);
                }
                var jobData = JsonSerializer.Deserialize<JobPostingData>(cleanedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return jobData;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to deserialize GPT response: {Error}", ex.Message);
                return null;
            }
        }

        private static bool IsJsonBalanced(string json)
        {
            int openBraces = json.Count(c => c == '{');
            int closeBraces = json.Count(c => c == '}');
            return openBraces == closeBraces;
        }

        private static string AppendMissingBraces(string json)
        {
            int openBraces = json.Count(c => c == '{');
            int closeBraces = json.Count(c => c == '}');
            int missing = openBraces - closeBraces;
            if (missing > 0)
            {
                json = json + new string('}', missing);
            }
            return json;
        }

        private static string SanitizeDescription(string? description)
        {
            if (string.IsNullOrEmpty(description)) return "none";
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("br");
            var sanitizedDescription = sanitizer.Sanitize(description);
            return sanitizedDescription;
        }

        private static string MapLocation(string? location)
        {
            var cleaned = CleanValue(location);
            return string.IsNullOrEmpty(cleaned) ? "none" : cleaned;
        }

        /// <summary>
        /// Parses company data from a GPT response by generating a company-specific prompt and deserializing the response into a CompanyData object.
        /// </summary>
        /// <param name="companyName">name of the company.</param>
        /// <returns>A CompanyData object if successful; otherwise null.</returns>
        public async Task<CompanyData?> ParseCompanyAsync(string companyName)
        {
            var prompt = GenerateCompanyPrompt(companyName);
            var gptResponse = await CallGptApiAsync(prompt);
            if (string.IsNullOrEmpty(gptResponse))
            {
                _logger.LogWarning("GPT failed to retrieve information for company: {CompanyName}", companyName);
                return null;
            }
            var companyData = DeserializeCompanyResponse(gptResponse);
            if (companyData == null)
            {
                _logger.LogWarning("Failed to deserialize GPT response for company: {CompanyName}", companyName);
                return null;
            }
            companyData.LogoUrl = GenerateLogoUrl(companyData.WebsiteUrl);
            companyData.Description = SanitizeDescription(companyData.Description);
            return companyData;
        }

        /// <summary>
        /// Generates a company specific prompt for GPT API based on the provided company name.
        /// </summary>
        /// <param name="companyName">name of the company.</param>
        /// <returns>String representing generated prompt.</returns>
        private static string GenerateCompanyPrompt(string companyName)
        {
            return $@"
        COMPANY ANALYSIS RULES:
        1. Use ONLY KNOWN PUBLIC FACTS about {companyName}
        2. DO NOT CREATE any information
        3. If data unavailable, use NULL
        4. Website MUST be official domain
        5. Headquarters format: 'Město, Kraj, Czech Republic' - use 'NULL' only if there is no data for headquarters, not for partial data
        6. Use czech language for description and industry. English otherwise

        Critical requirements:
            1. STRICT JSON SYNTAX:
               - No Markdown (```json or ```)
               - No unescaped quotes in content
               - No trailing commas
               - Always quote field names and string values
               - Use \"" for quotes inside strings
            2. STRUCTURAL PRIORITY:
               - Complete JSON structure takes priority over content length
               - If approaching token limit, truncate Description content FIRST
               - Never truncate closing brackets/braces
            3. ERROR PREVENTION:
               - Validate JSON with JSONLint before finalizing
               - Test for parseability programmatically
               - If unsure about any field, use null

        FAILURE EXAMPLES TO AVOID:
             ```json{{...}}``` 
             Missing final brace
             Don't use code block or other formathing just plain text in json format

        REQUIRED OUTPUT:
        {{
            ""CompanyName"": ""{companyName}"",
            ""Description"": ""3-5 sentences. KNOWN specialization only"",
            ""WebsiteUrl"": ""Full URL or null"",
            ""FoundedYear"": ""YYYY or null"",
            ""Headquarters"": ""Official HQ location or null"",
            ""Industry"": ""PRIMARY industry only"",
            ""NumberOfEmployees"": ""Exact number or range (e.g. 50-100)""
        }}
    ";
        }

        /// <summary>
        /// Deserializes GPT API response into a CompanyData object.
        /// </summary>
        /// <param name="gptResponse">raw GPT API response.</param>
        /// <returns>CompanyData object if deserialization is successful; otherwise null.</returns>
        private CompanyData? DeserializeCompanyResponse(string gptResponse)
        {
            try
            {
                var companyData = JsonSerializer.Deserialize<CompanyData>(gptResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (companyData != null)
                {
                    companyData.CompanyName = CleanValue(companyData.CompanyName) ?? "";
                    companyData.Description = CleanValue(companyData.Description);
                    companyData.WebsiteUrl = CleanValue(companyData.WebsiteUrl);
                    companyData.Headquarters = CleanValue(companyData.Headquarters);
                    companyData.Industry = CleanValue(companyData.Industry);
                }
                return companyData;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to deserialize GPT company response: {Error}", ex.Message);
                return null;
            }
        }

        private static string CleanGptResponse(string gptResponse)
        {
            if (string.IsNullOrEmpty(gptResponse)) return gptResponse;
            var regex = new Regex(@"^```(?:json)?\s*(.*?)\s*```$", RegexOptions.Singleline);
            var match = regex.Match(gptResponse);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return gptResponse.Trim();
        }

        private static string? GenerateLogoUrl(string? websiteUrl)
        {
            if (string.IsNullOrEmpty(websiteUrl)) return null;
            try
            {
                var uri = new Uri(websiteUrl);
                var domain = uri.Host;
                return $"https://logo.clearbit.com/{domain}?size=500";
            }
            catch
            {
                return null;
            }
        }

        private static string? CleanValue(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            string cleaned = Regex.Replace(input, @"\b(null|none)\b", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @",\s*,", ",");
            cleaned = cleaned.Trim(new char[] { ',', ' ' });
            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }
    }
}