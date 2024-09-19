using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAggregator.Api.Data.Repositories.Implementations
{
    public class JobPostingRepository : IJobPostingRepository
    {
        private readonly ApplicationDbContext _context;

        public JobPostingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string hashCode)
        {
            return await _context.JobPostings.AnyAsync(j => j.HashCode == hashCode);
        }

        public async Task<bool> IDExistsAsync(string externalId)
        {
            return await _context.JobPostings.AnyAsync(jp => jp.ExternalID == externalId);
        }

        public async Task AddAsync(JobPosting posting)
        {
            await _context.JobPostings.AddAsync(posting);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPosting>> SearchAsync(SearchCriteria criteria, int pageNumber, int pageSize)
        {
            var query = _context.JobPostings.AsQueryable();

            if (!string.IsNullOrEmpty(criteria.Keywords))
            {
                query = query.Where(j => j.Title.Contains(criteria.Keywords) || j.Description.Contains(criteria.Keywords));
            }

            if (!string.IsNullOrEmpty(criteria.Location))
            {
                var location = criteria.Location.Trim().ToLower();
                query = query.Where(j => j.Location.ToLower().Contains(location));
            }

            if (!string.IsNullOrEmpty(criteria.JobType))
            {
                query = query.Where(j => j.JobType.Contains(criteria.JobType));
            }

            if (!string.IsNullOrEmpty(criteria.CompanyName))
            {
                query = query.Where(j => j.CompanyName.Contains(criteria.CompanyName));
            }

            query = query.OrderByDescending(jp => jp.CreatedDate)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);


            return await query.ToListAsync();
        }

        public async Task<JobPosting> GetByIdAsync(int id)
        {
            return await _context.JobPostings.FindAsync(id);
        }

        public async Task<IEnumerable<JobPosting>> GetLatestJobPostingsAsync(int pageNumber, int pageSize)
        {
            return await _context.JobPostings
                .OrderByDescending(jp => jp.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetKeywordSuggestionsAsync(string term)
        {
            return await _context.JobPostings
                .Where(jp => jp.Title.Contains(term))
                .Select(jp => jp.Title)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetJobTypesSuggestionsAsync(string term)
        {
            return await _context.JobPostings
                .Where(jp => jp.JobType.Contains(term))
                .Select(jp => jp.JobType)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCompanyNamesSuggestionsAsync(string term)
        {
            return await _context.JobPostings
                .Where(jp => jp.CompanyName.Contains(term))
                .Select(jp => jp.CompanyName)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string term)
        {
            return await _context.JobPostings
                .Where(jp => jp.Location.Contains(term))
                .Select(jp => jp.Location)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }

    }
}
