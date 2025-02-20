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

        /// <summary>
        /// Searches job postings based on query parameters.
        /// </summary>
        /// <param name="criteria">search criteria.</param>
        /// <param name="pageNumber">page number (default 1).</param>
        /// <param name="pageSize">number of postings per page (default 9).</param>
        /// <returns>IActionResult containing the search results.</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchCriteria criteria, int pageNumber = 1, int pageSize = 9)
        {
            var postings = await _service.SearchAsync(criteria, pageNumber, pageSize);
            return Ok(postings);
        }

        /// <summary>
        /// Retrieves the latest job postings with pagination.
        /// </summary>
        /// <param name="pageNumber">page number (default 1).</param>
        /// <param name="pageSize">number of postings per page (default 10).</param>
        /// <returns>IActionResult containing latest job postings.</returns>
        [HttpGet("Latest")]
        public async Task<ActionResult<IEnumerable<JobPosting>>> GetLatestJobPostings(
           int pageNumber = 1, int pageSize = 10)
        {
            var jobPostings = await _service.GetLatestJobPostingsAsync(pageNumber, pageSize);
            return Ok(jobPostings);
        }

        /// <summary>
        /// Retrieves a job posting by its ID.
        /// </summary>
        /// <param name="id">job posting ID.</param>
        /// <returns>IActionResult with job posting or NotFound otherwise.</returns>
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

        /// <summary>
        /// Retrieves keyword suggestions for autocompletion based on a term.
        /// </summary>
        /// <param name="term">search term.</param>
        /// <returns>IActionResult containing keyword suggestions.</returns>
        [HttpGet("Autocomplete/Keywords")]
        public async Task<ActionResult<IEnumerable<string>>> GetKeywords(string term)
        {
            var keywords = await _service.GetKeywordSuggestionsAsync(term);
            return Ok(keywords);
        }

        /// <summary>
        /// Retrieves location suggestions for autocompletion based on a term.
        /// </summary>
        /// <param name="term">search term.</param>
        /// <returns>IActionResult containing location suggestions.</returns>
        [HttpGet("Autocomplete/Locations")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocations(string term)
        {
            var locations = await _service.GetLocationSuggestionsAsync(term);
            return Ok(locations);
        }

        /// <summary>
        /// Retrieves company name suggestions for autocompletion based on a term.
        /// </summary>
        /// <param name="term">search term.</param>
        /// <returns>IActionResult containing company name suggestions.</returns>
        [HttpGet("Autocomplete/CompanyNames")]
        public async Task<ActionResult<IEnumerable<string>>> GetCompanyNames(string term)
        {
            var companyNames = await _service.GetCompanyNamesSuggestionsAsync(term);
            return Ok(companyNames);
        }

        /// <summary>
        /// Retrieves job type suggestions for autocompletion based on a term.
        /// </summary>
        /// <param name="term">search term.</param>
        /// <returns>IActionResult containing job type suggestions.</returns>
        [HttpGet("Autocomplete/JobTypes")]
        public async Task<ActionResult<IEnumerable<string>>> GetJobTypes(string term)
        {
            var jobTypes = await _service.GetJobTypesSuggestionsAsync(term);
            return Ok(jobTypes);
        }
    }
}
