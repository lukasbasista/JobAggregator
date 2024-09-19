using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;

namespace JobAggregator.Api.Services.Implementations
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IJobPostingRepository _repository;

        public JobPostingService(IJobPostingRepository repository)
        {
            _repository = repository;
        }

        public async Task AddJobPostingsAsync(IEnumerable<JobPosting> postings)
        {
            foreach (var posting in postings)
            {
                if (!await _repository.ExistsAsync(posting.HashCode))
                {
                    await _repository.AddAsync(posting);
                }
            }
        }

        public async Task<IEnumerable<JobPosting>> SearchAsync(SearchCriteria criteria, int pageNumber, int pageSize)
        {
            return await _repository.SearchAsync(criteria, pageNumber, pageSize);
        }

        public async Task<JobPosting> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<JobPosting>> GetLatestJobPostingsAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetLatestJobPostingsAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<string>> GetKeywordSuggestionsAsync(string term)
        {
            return await _repository.GetKeywordSuggestionsAsync(term);
        }

        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string term)
        {
            return await _repository.GetLocationSuggestionsAsync(term);
        }

        public async Task<IEnumerable<string>> GetCompanyNamesSuggestionsAsync(string term)
        {
            return await _repository.GetCompanyNamesSuggestionsAsync(term);
        }

        public async Task<IEnumerable<string>> GetJobTypesSuggestionsAsync(string term)
        {
            return await _repository.GetJobTypesSuggestionsAsync(term);
        }

    }
}
