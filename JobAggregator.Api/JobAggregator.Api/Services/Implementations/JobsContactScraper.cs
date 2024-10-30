using HtmlAgilityPack;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using Serilog.Extensions.Logging;
using Serilog;

namespace JobAggregator.Api.Services.Implementations
{
    public class JobsContactScraper : IJobScraper
    {
        public Portal PortalInfo { get; private set; }

        private readonly HttpClient _httpClient;
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly ILogger<JobsContactScraper> _logger;

        public JobsContactScraper(IJobPostingRepository jobPostingRepository)
        {
            PortalInfo = new Portal
            {
                PortalName = "JobsContact.cz",
                BaseUrl = "https://www.jobscontact.cz",
                PortalLogoUrl = "https://media.zivefirmy.cz/logo/621b548ce462ba3994ad66a6b8bf66.jpg",
            };

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(PortalInfo.BaseUrl);
            _jobPostingRepository = jobPostingRepository;

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"Logs/{PortalInfo.PortalName}/{PortalInfo.PortalName}_Log.txt", rollingInterval: RollingInterval.Day);

            var serilogLogger = loggerConfiguration.CreateLogger();
            var loggerFactory = new SerilogLoggerFactory(serilogLogger);
            _logger = loggerFactory.CreateLogger<JobsContactScraper>();
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
                _logger.LogInformation("Portal not found in database. Creating new entry for {PortalName}", PortalInfo.PortalName);
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

            while (!foundExistingJob && pageNumber < 206)
            {
                var url = $"https://www.jobscontact.cz/prace?page={pageNumber}";
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

                var jobBoxes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'job-box')]");
                _logger.LogDebug("Found {JobCount} job boxes on page {PageNumber}", jobBoxes?.Count ?? 0, pageNumber);

                if (jobBoxes == null) break;

                var semaphore = new SemaphoreSlim(5);
                var tasks = new List<Task<JobPosting>>();

                foreach (var jobBox in jobBoxes)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var job = await ParseJobBoxAsync(jobBox);
                            return job;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);

                foreach (var jobPosting in results)
                {
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

        private async Task<JobPosting> ParseJobBoxAsync(HtmlNode jobBox)
        {
            _logger.LogDebug("Parsing job box...");

            var jobPosting = new JobPosting();
            var linkNode = jobBox.SelectSingleNode(".//a[@href]");
            var relativeUrl = linkNode?.GetAttributeValue("href", null);
            if (relativeUrl == null)
            {
                _logger.LogWarning("Failed to retrieve job URL from job box.");
                return null;
            }

            var jobUrl = $"{PortalInfo.BaseUrl}{relativeUrl}";
            jobPosting.ApplyUrl = jobUrl;
            _logger.LogDebug("Parsed job URL: {JobUrl}", jobUrl);

            var titleNode = jobBox.SelectSingleNode(".//h2");
            jobPosting.Title = titleNode?.InnerText.Trim() ?? "";
            _logger.LogDebug("Parsed job title: {Title}", jobPosting.Title);

            var salaryNode = jobBox.SelectSingleNode(".//p[contains(@class, 'pay')]");
            jobPosting.Salary = salaryNode?.InnerText.Trim() ?? "";
            _logger.LogDebug("Parsed salary: {Salary}", jobPosting.Salary);

            var locationNode = jobBox.SelectSingleNode(".//p[contains(@class, 'place')]//span");
            jobPosting.Location = locationNode?.InnerText.Trim() ?? "";
            _logger.LogDebug("Parsed location: {Location}", jobPosting.Location);

            var descriptionNode = linkNode.SelectSingleNode(".//p[not(@class)]");
            jobPosting.Description = HtmlEntity.DeEntitize(descriptionNode?.InnerText.Trim() ?? "No description available");
            _logger.LogDebug("Parsed description for job: {Title}", jobPosting.Title);

            var jobTypeNode = jobBox.SelectSingleNode(".//div[contains(@class, 'best-ico')]");
            if (jobTypeNode != null)
            {
                var labels = jobTypeNode.SelectNodes(".//div");
                if (labels != null)
                {
                    var jobTypes = new List<string>();
                    foreach (var label in labels)
                    {
                        var labelText = label.InnerText.Trim();
                        if (!string.IsNullOrEmpty(labelText))
                        {
                            jobTypes.Add(labelText);
                        }
                    }
                    jobPosting.JobType = string.Join(", ", jobTypes);
                }
            }

            if (string.IsNullOrEmpty(jobPosting.JobType))
            {
                jobPosting.JobType = "Unknown";
            }
            _logger.LogDebug("Parsed job type: {JobType}", jobPosting.JobType);

            var detailResponse = await _httpClient.GetAsync(relativeUrl);
            if (detailResponse.IsSuccessStatusCode)
            {
                var detailContent = await detailResponse.Content.ReadAsStringAsync();
                var detailDoc = new HtmlDocument();
                detailDoc.LoadHtml(detailContent);

                var fullDescriptionNode = detailDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'job-content')]//div[contains(@class, 'content')]");
                if (fullDescriptionNode != null)
                {
                    var description = HtmlEntity.DeEntitize(fullDescriptionNode.InnerText.Trim()) ?? string.Empty;
                    jobPosting.Description = CleanDescription(description);
                    _logger.LogDebug("Detailed description parsed for job: {Title}", jobPosting.Title);
                }
            }
            else
            {
                _logger.LogWarning("Failed to fetch job detail page for job URL: {JobUrl}", jobUrl);
            }

            jobPosting.CompanyName = PortalInfo.PortalName;
            jobPosting.DateScraped = DateTime.UtcNow;
            jobPosting.IsActive = true;
            jobPosting.CreatedDate = DateTime.UtcNow;
            jobPosting.LastUpdatedDate = DateTime.UtcNow;

            var hashInput = $"{jobPosting.Title}{jobPosting.ApplyUrl}";
            jobPosting.HashCode = HashHelper.ComputeSha256Hash(hashInput);
            jobPosting.ExternalID = ExtractJobIdFromUrl(relativeUrl);

            _logger.LogDebug("Generated hash code for job: {HashCode} and ExternalID: {ExternalID}", jobPosting.HashCode, jobPosting.ExternalID);
            return jobPosting;
        }

        private string ExtractJobIdFromUrl(string url)
        {
            var idIndex = url.LastIndexOf("_id");
            if (idIndex >= 0)
            {
                var externalId = url.Substring(idIndex + 3);
                _logger.LogDebug("Extracted ExternalID from URL: {ExternalID}", externalId);
                return externalId;
            }
            _logger.LogWarning("Failed to extract ExternalID from URL: {Url}", url);
            return null;
        }

        private string CleanDescription(string description)
        {
            var index = description.IndexOf("Doplňující informace");
            if (index >= 0)
            {
                description = description.Substring(0, index);
            }
            return description.Trim();
        }
    }
}