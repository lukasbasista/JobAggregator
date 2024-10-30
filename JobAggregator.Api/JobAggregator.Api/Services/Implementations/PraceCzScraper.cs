using HtmlAgilityPack;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using Serilog.Extensions.Logging;
using Serilog;

namespace JobAggregator.Api.Services.Implementations
{
    public class PraceCzScraper : IJobScraper
    {
        public Portal PortalInfo { get; private set; }

        private readonly HttpClient _httpClient;
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly ILogger<PraceCzScraper> _logger;

        public PraceCzScraper(IJobPostingRepository jobPostingRepository)
        {
            PortalInfo = new Portal
            {
                PortalName = "Prace.cz",
                BaseUrl = "https://www.prace.cz",
                PortalLogoUrl = "https://pracecdn.cz/images/logo-beata.svg",
            };

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(PortalInfo.BaseUrl);

            _jobPostingRepository = jobPostingRepository;

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"Logs/{PortalInfo.PortalName}/{PortalInfo.PortalName}_Log.txt", rollingInterval: RollingInterval.Day);

            var serilogLogger = loggerConfiguration.CreateLogger();
            var loggerFactory = new SerilogLoggerFactory(serilogLogger);
            _logger = loggerFactory.CreateLogger<PraceCzScraper>();
        }

        public async Task<IEnumerable<JobPosting>> ScrapeAsync()
        {
            _logger.LogInformation("Starting scrape for {PortalName}", PortalInfo.PortalName);
            var jobPostings = new List<JobPosting>();
            bool foundExistingJob = false;
            int pageNumber = 1;

            var portal = await _jobPostingRepository.GetPortalByNameAsync(PortalInfo.PortalName);
            if (portal == null)
            {
                _logger.LogInformation("Portal not found in database. Creating a new entry for {PortalName}", PortalInfo.PortalName);
                portal = new Portal
                {
                    PortalName = PortalInfo.PortalName,
                    BaseUrl = PortalInfo.BaseUrl,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow
                };
                await _jobPostingRepository.AddPortalAsync(portal);
                _logger.LogInformation("New portal entry created with ID: {PortalID}", portal.PortalID);
            }

            PortalInfo.PortalID = portal.PortalID;

            while (!foundExistingJob && pageNumber <= 5)
            {
                var url = $"/nabidky/?page={pageNumber}";
                _logger.LogDebug("Scraping page {PageNumber} for {PortalName} from URL: {Url}", pageNumber, PortalInfo.PortalName, url);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve page {PageNumber}. StatusCode: {StatusCode}", pageNumber, response.StatusCode);
                    return jobPostings;
                }

                var pageContents = await response.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(pageContents);

                var jobNodes = htmlDoc.DocumentNode.SelectNodes("//li[contains(@class, 'search-result__advert')]");
                _logger.LogDebug("Found {JobCount} job nodes on page {PageNumber}", jobNodes?.Count ?? 0, pageNumber);

                if (jobNodes == null || !jobNodes.Any()) break;

                foreach (var jobNode in jobNodes)
                {
                    var jobPosting = await ParseJobNodeAsync(jobNode);
                    if (jobPosting != null)
                    {
                        jobPosting.PortalID = PortalInfo.PortalID;
                        bool exists = await _jobPostingRepository.IDExistsAsync(jobPosting.ExternalID);
                        if (exists)
                        {
                            _logger.LogInformation("Job with ExternalID: {ExternalID} already exists in the database.", jobPosting.ExternalID);
                            foundExistingJob = true;
                            break;
                        }
                        _logger.LogInformation("Adding new job posting with title: {Title}", jobPosting.Title);
                        jobPostings.Add(jobPosting);
                    }
                }

                if (!foundExistingJob) pageNumber++;
            }

            _logger.LogInformation("Scraping completed. Total jobs scraped: {JobCount}", jobPostings.Count);
            return jobPostings;
        }

        private async Task<JobPosting> ParseJobNodeAsync(HtmlNode jobNode)
        {
            _logger.LogDebug("Parsing job node...");

            if (jobNode.GetAttributeValue("id", "") == "signUpWrapper")
            {
                _logger.LogDebug("Skipped signup/advertisement node.");
                return null;
            }

            var jobPosting = new JobPosting();

            var titleNode = jobNode.SelectSingleNode(".//h3[@class='half-standalone']/a");
            if (titleNode == null)
            {
                _logger.LogWarning("Failed to parse title node.");
                return null;
            }

            jobPosting.Title = CleanText(titleNode.InnerText);
            _logger.LogDebug("Parsed title: {Title}", jobPosting.Title);

            var relativeUrl = titleNode.GetAttributeValue("href", null);
            if (relativeUrl == null)
            {
                _logger.LogWarning("No URL found for job title: {Title}", jobPosting.Title);
                return null;
            }

            var jobUrl = $"{PortalInfo.BaseUrl.TrimEnd('/')}{relativeUrl}";
            jobPosting.ApplyUrl = jobUrl;
            _logger.LogDebug("Parsed job URL: {JobUrl}", jobUrl);

            jobPosting.ExternalID = titleNode.GetAttributeValue("data-jd", null);
            if (string.IsNullOrEmpty(jobPosting.ExternalID))
            {
                _logger.LogWarning("Failed to parse ExternalID for job: {Title}", jobPosting.Title);
                return null;
            }

            var imageNode = jobNode.SelectSingleNode(".//img");
            if (imageNode != null)
            {
                var imageUrl = imageNode.GetAttributeValue("src", null);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (!imageUrl.StartsWith("http"))
                    {
                        imageUrl = new Uri(new Uri(PortalInfo.BaseUrl), imageUrl).ToString();
                    }

                    jobPosting.CompanyLogoUrl = imageUrl;
                    _logger.LogDebug("Parsed company logo URL: {CompanyLogoUrl}", jobPosting.CompanyLogoUrl);
                }
            }
            else
            {
                _logger.LogDebug("No company logo found for job: {Title}", jobPosting.Title);
            }

            var companyNode = jobNode.SelectSingleNode(".//div[contains(@class, 'search-result__advert__box__item--company')]");
            jobPosting.CompanyName = CleanText(companyNode?.InnerText.Trim() ?? PortalInfo.PortalName);
            _logger.LogDebug("Parsed company name: {CompanyName}", jobPosting.CompanyName);

            var locationNode = jobNode.SelectSingleNode(".//div[contains(@class, 'search-result__advert__box__item--location')]/strong");
            jobPosting.Location = CleanText(locationNode?.InnerText.Trim() ?? "");
            _logger.LogDebug("Parsed location: {Location}", jobPosting.Location);

            var salaryNode = jobNode.SelectSingleNode(".//span[contains(@class, 'search-result__advert__box__item--salary')]");
            jobPosting.Salary = CleanText(salaryNode?.InnerText.Trim() ?? "").Replace("Kč", "Kč").Replace("/měsíc", " per month");
            _logger.LogDebug("Parsed salary: {Salary}", jobPosting.Salary);

            var employmentTypeNode = jobNode.SelectSingleNode(".//div[contains(@class, 'search-result__advert__box__item--employment-type')]");
            jobPosting.JobType = CleanText(employmentTypeNode?.InnerText.Trim() ?? "");
            _logger.LogDebug("Parsed job type: {JobType}", jobPosting.JobType);

            var detailResponse = await _httpClient.GetAsync(relativeUrl);
            jobPosting.Description = "";
            if (detailResponse.IsSuccessStatusCode)
            {
                var detailContent = await detailResponse.Content.ReadAsStringAsync();
                var detailDoc = new HtmlDocument();
                detailDoc.LoadHtml(detailContent);

                var descriptionNode = detailDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'advert__richtext')]");
                jobPosting.Description = descriptionNode != null
                    ? CleanText(HtmlEntity.DeEntitize(descriptionNode.InnerText.Trim()))
                    : "";
                _logger.LogDebug("Parsed job description for {Title}", jobPosting.Title);
            }
            else
            {
                _logger.LogWarning("Failed to fetch job detail page for {Title}", jobPosting.Title);
            }

            jobPosting.DateScraped = DateTime.UtcNow;
            jobPosting.IsActive = true;
            jobPosting.CreatedDate = DateTime.UtcNow;
            jobPosting.LastUpdatedDate = DateTime.UtcNow;

            var hashInput = $"{jobPosting.Title}{jobPosting.ApplyUrl}";
            jobPosting.HashCode = HashHelper.ComputeSha256Hash(hashInput);
            _logger.LogDebug("Generated hash code for job: {HashCode}", jobPosting.HashCode);

            return jobPosting;
        }

        private string CleanText(string text)
        {
            return HtmlEntity.DeEntitize(text)
                .Replace("&nbsp;", " ")
                .Replace("&ndash;", "-")
                .Replace("–", "-")
                .Replace("&#160;", " ")
                .Replace("&#8211;", "-")
                .Replace("&amp;", "&")
                .Trim();
        }
    }
}
