using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : ControllerBase
    {
        private readonly IJobPostingService _service;

        public JobPostingsController(IJobPostingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchCriteria criteria, int pageNumber = 1, int pageSize = 9)
        {
            var postings = await _service.SearchAsync(criteria, pageNumber, pageSize);
            return Ok(postings);
        }

        [HttpGet("Latest")]
        public async Task<ActionResult<IEnumerable<JobPosting>>> GetLatestJobPostings(
           int pageNumber = 1, int pageSize = 10)
        {
            var jobPostings = await _service.GetLatestJobPostingsAsync(pageNumber, pageSize);
            return Ok(jobPostings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var posting = await _service.GetByIdAsync(id);
            if (posting == null)
            {
                return NotFound();
            }
            return Ok(posting);
        }

        [HttpGet("Autocomplete/Keywords")]
        public async Task<ActionResult<IEnumerable<string>>> GetKeywords(string term)
        {
            var keywords = await _service.GetKeywordSuggestionsAsync(term);
            return Ok(keywords);
        }

        [HttpGet("Autocomplete/Locations")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocations(string term)
        {
            var locations = await _service.GetLocationSuggestionsAsync(term);
            return Ok(locations);
        }

        [HttpGet("Autocomplete/CompanyNames")]
        public async Task<ActionResult<IEnumerable<string>>> GetCompanyNames(string term)
        {
            var companyNames = await _service.GetCompanyNamesSuggestionsAsync(term);
            return Ok(companyNames);
        }

        [HttpGet("Autocomplete/JobTypes")]
        public async Task<ActionResult<IEnumerable<string>>> GetJobTypes(string term)
        {
            var jobTypes = await _service.GetJobTypesSuggestionsAsync(term);
            return Ok(jobTypes);
        }
    }
}
