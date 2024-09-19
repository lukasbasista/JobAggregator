using HtmlAgilityPack;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;

namespace JobAggregator.Api.Services.Implementations
{
    public class JobsContactScraper : IJobScraper
    {
        public Portal PortalInfo { get; private set; }

        private readonly HttpClient _httpClient;
        private readonly IJobPostingRepository _jobPostingRepository;

        public JobsContactScraper(IJobPostingRepository jobPostingRepository)
        {
            PortalInfo = new Portal
            {
                PortalName = "JobsContact.cz",
                BaseUrl = "https://www.jobscontact.cz",
                PortalID = 3
            };

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(PortalInfo.BaseUrl);

            _jobPostingRepository = jobPostingRepository;
        }

        public async Task<IEnumerable<JobPosting>> ScrapeAsync()
        {
            var jobPostings = new List<JobPosting>();
            bool foundExistingJob = false;
            int pageNumber = 1;

            while (!foundExistingJob && pageNumber < 206)
            {
                var url = $"https://www.jobscontact.cz/prace?page={pageNumber}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return jobPostings;
                }

                var pageContents = await response.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(pageContents);

                var jobBoxes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'job-box')]");

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
                            return await ParseJobBoxAsync(jobBox);
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
                            foundExistingJob = true;
                            break;
                        }
                        jobPostings.Add(jobPosting);
                    }
                }
                
                if (!foundExistingJob) pageNumber++;
            }

            return jobPostings;
        }

        private async Task<JobPosting> ParseJobBoxAsync(HtmlNode jobBox)
        {
            var jobPosting = new JobPosting();

            var linkNode = jobBox.SelectSingleNode(".//a[@href]");
            var relativeUrl = linkNode?.GetAttributeValue("href", null);
            if (relativeUrl == null)
            {
                return null;
            }

            var jobUrl = $"{PortalInfo.BaseUrl}{relativeUrl}";
            jobPosting.ApplyUrl = jobUrl;

            var titleNode = jobBox.SelectSingleNode(".//h2");
            jobPosting.Title = titleNode?.InnerText.Trim();

            var salaryNode = jobBox.SelectSingleNode(".//p[contains(@class, 'pay')]");
            jobPosting.Salary = salaryNode?.InnerText.Trim();

            var locationNode = jobBox.SelectSingleNode(".//p[contains(@class, 'place')]//span");
            jobPosting.Location = locationNode?.InnerText.Trim();

            var descriptionNode = linkNode.SelectSingleNode(".//p[not(@class)]");
            jobPosting.Description = HtmlEntity.DeEntitize(descriptionNode.InnerText.Trim());

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
                }
            }

            jobPosting.CompanyName = PortalInfo.PortalName;

            jobPosting.DateScraped = DateTime.UtcNow;
            jobPosting.IsActive = true;
            jobPosting.CreatedDate = DateTime.UtcNow;
            jobPosting.LastUpdatedDate = DateTime.UtcNow;

            var hashInput = $"{jobPosting.Title}{jobPosting.ApplyUrl}";
            jobPosting.HashCode = HashHelper.ComputeSha256Hash(hashInput);

            jobPosting.ExternalID = ExtractJobIdFromUrl(relativeUrl);

            return jobPosting;
        }

        private string ExtractJobIdFromUrl(string url)
        {
            var idIndex = url.LastIndexOf("_id");
            if (idIndex >= 0)
            {
                return url.Substring(idIndex + 3);
            }
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
