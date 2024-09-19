using JobAggregator.Api.Models;

namespace JobAggregator.Api.Services.Interfaces
{
    public interface IJobPostingService
    {
        Task AddJobPostingsAsync(IEnumerable<JobPosting> postings);
        Task<IEnumerable<JobPosting>> SearchAsync(SearchCriteria criteria, int pageNumber, int pageSize);
        Task<JobPosting> GetByIdAsync(int id);
        Task<IEnumerable<JobPosting>> GetLatestJobPostingsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<string>> GetKeywordSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetJobTypesSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetCompanyNamesSuggestionsAsync(string term);
        Task<IEnumerable<string>> GetLocationSuggestionsAsync(string term);
    }
}
