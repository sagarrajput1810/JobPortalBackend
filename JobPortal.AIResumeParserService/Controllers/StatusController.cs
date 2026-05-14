using Microsoft.AspNetCore.Mvc;

namespace JobPortal.AIResumeParserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Service = "AI Resume Parser Service",
                Status = "Running",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
