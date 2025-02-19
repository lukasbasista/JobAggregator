using HtmlAgilityPack;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace JobAggregator.Api.Services.Implementations
{
    public abstract class BaseScraper : IJobScraper
    {
        public abstract Portal PortalInfo { get; }

        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly IGptJobParser _gptJobParser;
        protected readonly IServiceProvider _serviceProvider;

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

        public virtual async Task<IEnumerable<JobPosting>> ScrapeAsync()
        {
            _logger.LogInformation("Starting scrape for {PortalName}", PortalInfo.PortalName);
            await EnsurePortalExistsAsync();

            var jobPostings = new ConcurrentBag<JobPosting>();
            var processedCompanies = new ConcurrentDictionary<string, Lazy<Task<Company>>>();
            int pageNumber = 1;

            // Možnosť definovať maximálny počet strán – ak je potrebné, možno sprístupniť cez virtuálnu vlastnosť.
            while (pageNumber <= MaxPages)
            {
                var pageUrl = GetPageUrl(pageNumber);
                var listings = await GetJobListingsAsync(pageUrl);

                if (listings == null || !listings.Any())
                {
                    _logger.LogInformation("Žiadne ďalšie inzeráty nájdené na stránke {PageNumber}", pageNumber);
                    break;
                }

                // Použitie throttleru na paralelné spracovanie (nastaviteľná hodnota)
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

            _logger.LogInformation("Scraping pre {PortalName} skončený. Celkom nájdených inzerátov: {Count}", PortalInfo.PortalName, jobPostings.Count);
            return jobPostings.ToList();
        }

        /// <summary>
        /// Maximálny počet strán, ktoré sa majú prejsť – možno predefinovať v odvodených triedach.
        /// </summary>
        protected virtual int MaxPages => 2;

        /// <summary>
        /// Maximálny počet paralelných vlákien na spracovanie inzerátov.
        /// </summary>
        protected virtual int MaxDegreeOfParallelism => 10;

        /// <summary>
        /// Vráti URL stránky pre dané číslo stránky.
        /// </summary>
        protected abstract string GetPageUrl(int pageNumber);

        /// <summary>
        /// Zo zadaného URL načíta HTML a vráti kolekciu HTML uzlov predstavujúcich jednotlivé pracovné inzeráty.
        /// </summary>
        protected abstract Task<IEnumerable<HtmlNode>> GetJobListingsAsync(string url);

        /// <summary>
        /// Zo zadaného uzla extrahuje URL detailu pracovného inzerátu.
        /// </summary>
        protected abstract string ExtractJobUrl(HtmlNode jobListing);

        /// <summary>
        /// Pre dané URL vráti obsah detailu inzerátu ako text (prípadne so špecifickým spracovaním).
        /// </summary>
        protected abstract Task<string> GetJobContentAsync(string jobUrl);

        /// <summary>
        /// Spoločná logika pre spracovanie jedného pracovného inzerátu.
        /// </summary>
        protected async Task ProcessJobListingAsync(HtmlNode listing, ConcurrentBag<JobPosting> jobPostings, ConcurrentDictionary<string, Lazy<Task<Company>>> processedCompanies)
        {
            var jobUrl = ExtractJobUrl(listing);
            if (string.IsNullOrEmpty(jobUrl))
            {
                _logger.LogWarning("Nebolo možné získať URL z inzerátu.");
                return;
            }

            var externalId = GenerateExternalId(jobUrl);
            if (await JobExistsByExternalIdAsync(externalId))
            {
                _logger.LogInformation("Inzerát s ExternalID {ExternalID} už existuje.", externalId);
                return;
            }

            var jobContent = await GetJobContentAsync(jobUrl);
            if (string.IsNullOrEmpty(jobContent))
            {
                _logger.LogWarning("Obsah inzerátu z URL {JobUrl} je prázdny.", jobUrl);
                return;
            }

            var jobData = await _gptJobParser.ParseJobAsync(jobContent, jobUrl, PortalInfo.PortalName, PortalInfo.BaseUrl);
            if (jobData == null)
            {
                _logger.LogWarning("Neúspešné parsovanie inzerátu z URL {JobUrl}.", jobUrl);
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

        #region Spoločné metódy (nemeniť)

        protected async Task<Company> GetOrCreateCompanyAsync(string companyName)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            var company = await repository.GetCompanyByNameAsync(companyName);
            if (company == null)
            {
                _logger.LogInformation("Vytváram novú spoločnosť: {CompanyName}", companyName);
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

        protected async Task<JobPosting> MapJobDataToJobPostingAsync(JobPostingData jobData, Company company)
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
            return jobPosting;
        }

        protected async Task EnsurePortalExistsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            var portal = await repository.GetPortalByNameAsync(PortalInfo.PortalName);
            if (portal == null)
            {
                _logger.LogInformation("Portal {PortalName} neexistuje v databáze, vytváram nový záznam.", PortalInfo.PortalName);
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
                _logger.LogInformation("Nový portal vytvorený s ID: {PortalID}", portal.PortalID);
            }

            PortalInfo.PortalID = portal.PortalID;
        }

        protected async Task<bool> AddJobPostingIfNotExistsAsync(JobPosting jobPosting)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();

            bool exists = await repository.ExistsAsync(jobPosting.HashCode);
            if (exists)
            {
                _logger.LogInformation("Inzerát s HashCode {HashCode} už existuje.", jobPosting.HashCode);
                return false;
            }
            return true;
        }

        protected async Task<bool> JobExistsByExternalIdAsync(string externalId)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobPostingRepository>();
            return await repository.ExistsByExternalIdAsync(externalId);
        }

        #endregion
    }
}