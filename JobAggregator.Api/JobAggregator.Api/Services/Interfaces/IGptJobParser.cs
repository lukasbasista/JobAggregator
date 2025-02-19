using JobAggregator.Api.Models;

namespace JobAggregator.Api.Services.Interfaces
{
    public interface IGptJobParser
    {
        Task<JobPostingData?> ParseJobAsync(string content, string jobUrl, string portalName, string portalUrl);
        Task<CompanyData?> ParseCompanyAsync(string companyName);
    }
}
