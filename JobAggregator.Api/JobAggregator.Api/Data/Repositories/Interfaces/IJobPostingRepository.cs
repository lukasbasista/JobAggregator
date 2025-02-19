using JobAggregator.Api.Models;

namespace JobAggregator.Api.Data.Repositories.Interfaces
{
    public interface IJobPostingRepository
    {
        Task<bool> ExistsAsync(string hashCode);
        Task AddAsync(JobPosting posting);
        Task<IEnumerable<JobPosting>> SearchAsync(SearchCriteria criteria, int pageNumber, int pageSize);
        Task<JobPosting?> GetByIdAsync(int id);
        Task<IEnumerable<JobPosting>> GetLatestJobPostingsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<string>> GetKeywordSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetLocationSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetCompanyNamesSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetJobTypesSuggestionsAsync(string term);
        Task<bool> IDExistsAsync(string externalId);
        Task<Portal?> GetPortalByNameAsync(string portalName);
        Task AddPortalAsync(Portal portal);
        Task<Company?> GetCompanyByNameAsync(string companyName);
        Task AddCompanyAsync(Company company);
        Task<bool> ExistsByExternalIdAsync(string externalId);
    }
}
