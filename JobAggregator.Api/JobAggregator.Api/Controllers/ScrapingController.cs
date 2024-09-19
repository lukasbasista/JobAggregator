using JobAggregator.Api.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace JobAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapingController : ControllerBase
    {
        private readonly ScraperManager _scraperManager;

        public ScrapingController(ScraperManager scraperManager)
        {
            _scraperManager = scraperManager;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartScraping()
        {
            await _scraperManager.ScrapeAllAsync();
            return Ok(new { message = "Scraping started successfully." });
        }
    }
}
