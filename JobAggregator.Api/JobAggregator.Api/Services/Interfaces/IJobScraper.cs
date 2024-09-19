using JobAggregator.Api.Models;

namespace JobAggregator.Api.Services.Interfaces
{
    public interface IJobScraper
    {
        Task<IEnumerable<JobPosting>> ScrapeAsync();
        Portal PortalInfo { get; }
    }
}
