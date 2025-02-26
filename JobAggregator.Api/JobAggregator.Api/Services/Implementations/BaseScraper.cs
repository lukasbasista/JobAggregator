using HtmlAgilityPack;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace JobAggregator.Api.Services.Implementations
{
    public abstract class BaseScraper : IJobScraper
    {
        public abstract Portal PortalInfo { get; }

        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly IGptJobParser _gptJobParser;
        protected readonly IServiceProvider _serviceProvider;

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _jobPostingLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        protected BaseScraper(IServiceProvider serviceProvider, ILogger logger, IGptJobParser gptJobParser)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };
            _httpClient = new HttpClient(handler);
            _serviceProvider = serviceProvider;
            _logger = logger;
            _gptJobParser = gptJobParser;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36");
        }

        /// <summary>
        /// Scrapes job postings concurrently across pages.
        /// </summary>
        public virtual async Task<IEnumerable<JobPosting>> ScrapeAsync()
        {
            _logger.LogInformation("Starting scrape for {PortalName}", PortalInfo.PortalName);
            await EnsurePortalExistsAsync();

            var jobPostings = new ConcurrentBag<JobPosting>();
            var processedCompanies = new ConcurrentDictionary<string, Lazy<Task<Company>>>();
            int pageNumber = 1;

            while (pageNumber <= MaxPages)
            {
                var pageUrl = GetPageUrl(pageNumber);
                var listings = await GetJobListingsAsync(pageUrl);

                if (listings == null || !listings.Any())
                {
                    _logger.LogInformation("No further job postings found on page {PageNumber}", pageNumber);
                    break;
                }

                using var throttler = new SemaphoreSlim(MaxDegreeOfParallelism);
                var tasks = listings.Select(async listing =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        await ProcessJobListingAsync(listing, jobPostings, processedCompanies);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                });

                await Task.WhenAll(tasks);
                pageNumber++;
            }

            _logger.LogInformation("Scraping for {PortalName} completed. Total job postings found: {Count}\"", PortalInfo.PortalName, jobPostings.Count);
            return jobPostings.ToList();
        }

        /// <summary>
        /// Maximum number of pages to process – can be overridden in derived classes.
        /// </summary>
        protected virtual int MaxPages => 5;

        /// <summary>
        /// Maximum number of parallel threads for processing job postings.
        /// </summary>
        protected virtual int MaxDegreeOfParallelism => 10;

        /// <summary>
        /// Returns URL of the page for specified page number.
        /// </summary>
        /// <param name="pageNumber">page number.</param>
        /// <returns>String with page URL.</returns>
        protected abstract string GetPageUrl(int pageNumber);

        /// <summary>
        /// Downloads HTML from the specified URL and returns HTML nodes representing job postings.
        /// </summary>
        /// <param name="url">URL to download from.</param>
        protected abstract Task<IEnumerable<HtmlNode>> GetJobListingsAsync(string url);

        /// <summary>
        /// Extracts job posting detail URL from given HTML node.
        /// </summary>
        /// <param name="jobListing">HTML node representing job listing.</param>
        /// <returns>String containing job URL, or null if not found.</returns>
        protected abstract string? ExtractJobUrl(HtmlNode jobListing);

        /// <summary>
        /// Retrieves job posting detail content as text from the specified URL.
        /// </summary>
        /// <param name="jobUrl">URL of the job posting detail.</param>
        /// <returns>Task that returns job posting content.</returns>
        protected abstract Task<string> GetJobContentAsync(string jobUrl);

        /// <summary>
        /// Processes a single job listing node by extracting, parsing, and mapping it to JobPosting object.
        /// </summary>
        /// <param name="listing">HTML node of the job listing.</param>
        /// <param name="jobPostings">concurrent collection to add the JobPosting to.</param>
        /// <param name="processedCompanies">dictionary to track processed companies.</param>
        protected async Task ProcessJobListingAsync(HtmlNode listing, ConcurrentBag<JobPosting> jobPostings, ConcurrentDictionary<string, Lazy<Task<Company>>> processedCompanies)
        {
            var jobUrl = ExtractJobUrl(listing);
            if (string.IsNullOrEmpty(jobUrl))
            {
                _logger.LogWarning("Could not extract URL from job listing.");
                return;
            }

            var externalId = GenerateExternalId(jobUrl);
            var jobLock = _jobPostingLocks.GetOrAdd(externalId, _ => new SemaphoreSlim(1, 1));
            await jobLock.WaitAsync();
            try
            {
                if (await JobExistsByExternalIdAsync(externalId))
                {
                    _logger.LogInformation("Job posting with ExternalID {ExternalID} already exists.", externalId);
                    return;
                }

                var jobContent = await GetJobContentAsync(jobUrl);
                if (string.IsNullOrEmpty(jobContent))
                {
                    _logger.LogWarning("Job content from URL {JobUrl} is empty.", jobUrl);
                    return;
                }

                var jobData = await _gptJobParser.ParseJobAsync(jobContent, jobUrl, PortalInfo.PortalName, PortalInfo.BaseUrl);
                if (jobData == null)
                {
                    _logger.LogWarning("Failed to parse job posting from URL {JobUrl}.", jobUrl);
                    return;
                }

                var company = await processedCompanies.GetOrAdd(
                    jobData.CompanyName,
                    key => new Lazy<Task<Company>>(() => GetOrCreateCompanyAsync(key))
                ).Value;

                var jobPosting = await MapJobDataToJobPostingAsync(jobData, company);
                if (await AddJobPostingIfNotExistsAsync(jobPosting))
                {
                    jobPostings.Add(jobPosting);
                }
            }
            finally
            {
                jobLock.Release();
            }
        }

        #region Common methods

        /// <summary>
        /// Retrieves an existing Company by name or creates one if doesn't exist.
        /// </summary>
        /// <param name="companyName">name of the company.</param>
        protected async Task<Company> GetOrCreateCompanyAsync(string companyName)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            var company = await repository.GetCompanyByNameAsync(companyName);
            if (company == null)
            {
                _logger.LogInformation("Creating new company: {CompanyName}", companyName);
                var companyData = await _gptJobParser.ParseCompanyAsync(companyName);
                company = companyData == null
                    ? new Company
                    {
                        CompanyName = companyName,
                        Description = "No description available.",
                        WebsiteUrl = null,
                        FoundedYear = null,
                        NumberOfEmployees = null,
                        Headquarters = null,
                        Industry = null,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    }
                    : new Company
                    {
                        CompanyName = companyData.CompanyName,
                        Description = companyData.Description,
                        WebsiteUrl = companyData.WebsiteUrl,
                        LogoUrl = companyData.LogoUrl,
                        FoundedYear = companyData.FoundedYear,
                        NumberOfEmployees = companyData.NumberOfEmployees,
                        Headquarters = companyData.Headquarters,
                        Industry = companyData.Industry,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };

                await repository.AddCompanyAsync(company);
            }
            return company;
        }

        protected string GenerateExternalId(string url) => HashHelper.ComputeSha256Hash(url);

        /// <summary>
        /// Maps JobPostingData and Company to a new JobPosting and computes its hash.
        /// </summary>
        /// <param name="jobData">parsed job posting data.</param>
        /// <param name="company">associated Company entity.</param>
        protected Task<JobPosting> MapJobDataToJobPostingAsync(JobPostingData jobData, Company company)
        {
            var jobPosting = new JobPosting
            {
                Title = jobData.Title ?? "none",
                Location = jobData.Location,
                Description = jobData.Description,
                SalaryFrom = jobData.SalaryFrom,
                SalaryTo = jobData.SalaryTo,
                Currency = "CZK",
                JobType = jobData.JobType ?? "none",
                ApplyUrl = jobData.ApplyUrl,
                ExternalID = GenerateExternalId(jobData.ApplyUrl),
                DateScraped = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CompanyID = company.CompanyID,
                PortalID = PortalInfo.PortalID
            };

            jobPosting.HashCode = HashHelper.ComputeSha256Hash(jobPosting.Title + jobPosting.ApplyUrl);
            return Task.FromResult(jobPosting);
        }

        /// <summary>
        /// Ensures that portal exists in the database; creates new record if missing.
        /// </summary>
        protected async Task EnsurePortalExistsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            var portal = await repository.GetPortalByNameAsync(PortalInfo.PortalName);
            if (portal == null)
            {
                _logger.LogInformation("Portal {PortalName} does not exist in the database, creating a new record.", PortalInfo.PortalName);
                portal = new Portal
                {
                    PortalName = PortalInfo.PortalName,
                    BaseUrl = PortalInfo.BaseUrl,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    PortalLogoUrl = PortalInfo.PortalLogoUrl
                };
                await repository.AddPortalAsync(portal);
                _logger.LogInformation("New portal created with ID: {PortalID}", portal.PortalID);
            }

            PortalInfo.PortalID = portal.PortalID;
        }

        /// <summary>
        /// Checks if given job posting (by hash code) does not already exist, returns true if new.
        /// </summary>
        /// <param name="jobPosting">JobPosting object to check.</param>
        protected async Task<bool> AddJobPostingIfNotExistsAsync(JobPosting jobPosting)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            bool exists = await repository.ExistsAsync(jobPosting.HashCode!);
            if (exists)
            {
                _logger.LogInformation("IJob posting with HashCode {HashCode} already exists.", jobPosting.HashCode);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if job posting with the specified external ID exists.
        /// </summary>
        /// <param name="externalId">external ID to check.</param>
        protected async Task<bool> JobExistsByExternalIdAsync(string externalId)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();
            return await repository.ExistsByExternalIdAsync(externalId);
        }

        #endregion
    }
}