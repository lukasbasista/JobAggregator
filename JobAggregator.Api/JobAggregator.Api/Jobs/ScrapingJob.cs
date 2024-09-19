using JobAggregator.Api.Services.Implementations;
using Quartz;

namespace JobAggregator.Api.Jobs
{
    public class ScrapingJob : IJob
    {
        private readonly ScraperManager _scraperManager;

        public ScrapingJob(ScraperManager scraperManager)
        {
            _scraperManager = scraperManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _scraperManager.ScrapeAllAsync();
        }
    }
}
