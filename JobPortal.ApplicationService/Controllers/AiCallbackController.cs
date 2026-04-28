using JobPortal.ApplicationService.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.ApplicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiCallbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AiCallbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("update-score")]
        public async Task<IActionResult> UpdateScore([FromBody] AiScoreUpdateRequest request)
        {
            var application = await _context.JobApplications.FindAsync(request.ApplicationId);
            if (application == null) return NotFound();

            application.AtsScore = request.AtsScore;
            application.AiSummary = request.AiSummary;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class AiScoreUpdateRequest
    {
        public int ApplicationId { get; set; }
        public int AtsScore { get; set; }
        public string AiSummary { get; set; } = string.Empty;
    }
}
