using JobAggregator.Api.Services.Interfaces;

namespace JobAggregator.Api.Services.Implementations
{
    public class ScraperManager
    {
        private readonly IEnumerable<IJobScraper> _scrapers;
        private readonly IJobPostingService _jobPostingService;

        public ScraperManager(IEnumerable<IJobScraper> scrapers, IJobPostingService jobPostingService)
        {
            _scrapers = scrapers;
            _jobPostingService = jobPostingService;
        }

        public async Task ScrapeAllAsync()
        {
            foreach (var scraper in _scrapers)
            {
                var postings = await scraper.ScrapeAsync();
                await _jobPostingService.AddJobPostingsAsync(postings);
            }
        }
    }
}
