using JobAggregator.Api.Helpers;
using JobAggregator.Api.Services.Implementations;
using Quartz;

namespace JobAggregator.Api.Jobs
{
    [DisallowConcurrentExecution]
    public class ScrapingJob : IJob
    {
        private readonly ScraperManager _scraperManager;
        private readonly ILogger<ScrapingJob> _logger;

        public ScrapingJob(ScraperManager scraperManager, ILogger<ScrapingJob> logger)
        {
            _scraperManager = scraperManager;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Scraping job started at {Time}", DateTimeOffset.Now);

            await _scraperManager.ScrapeAllAsync();

            _logger.LogInformation("Scraping job completed at {Time}", DateTimeOffset.Now);

            var nextFireTime = SchedulerHelper.GetNextRandomTime();
            _logger.LogInformation("Next scraping job scheduled at {Time}", nextFireTime);

            var trigger = TriggerBuilder.Create()
                .ForJob(context.JobDetail)
                .WithIdentity("ScrapingJob-trigger")
                .StartAt(nextFireTime)
                .Build();

            await context.Scheduler.RescheduleJob(new TriggerKey("ScrapingJob-trigger"), trigger);
        }
    }
}
