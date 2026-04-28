using JobPortal.SearchService.Models;
using JobPortal.SearchService.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var results = await _searchService.SearchJobsAsync(query);
            return Ok(results);
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexJob([FromBody] JobDocument job)
        {
            var success = await _searchService.IndexJobAsync(job);
            return success ? Ok("Job indexed successfully") : BadRequest("Failed to index job");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var success = await _searchService.DeleteJobAsync(id);
            return success ? Ok("Job deleted from index") : NotFound("Job not found in index");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateJob([FromBody] JobDocument job)
        {
            var success = await _searchService.UpdateJobAsync(job);
            return success ? Ok("Job index updated") : NotFound("Job not found in index");
        }
    }
}
