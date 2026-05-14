using JobPortal.ApplicationService.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.ApplicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiCallbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AiCallbackController> _logger;

        public AiCallbackController(ApplicationDbContext context, ILogger<AiCallbackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("update-score")]
        public async Task<IActionResult> UpdateScore([FromBody] AiScoreUpdateRequest request)
        {
            try 
            {
                _logger.LogInformation("AiCallback: Received update request for ApplicationId: {ApplicationId}, Score: {Score}, Summary: {Summary}", 
                    request.ApplicationId, request.AtsScore, request.AiSummary);
                
                var application = await _context.JobApplications.FindAsync(request.ApplicationId);
                if (application == null)
                {
                    _logger.LogWarning("AiCallback: Application with Id {ApplicationId} not found.", request.ApplicationId);
                    return NotFound(new { Message = $"Application {request.ApplicationId} not found" });
                }

                application.AtsScore = request.AtsScore;
                application.AiSummary = request.AiSummary;

                await _context.SaveChangesAsync();
                _logger.LogInformation("AiCallback: Successfully updated score for ApplicationId: {ApplicationId}", request.ApplicationId);
                return Ok(new { Message = "Score updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiCallback: Error updating score for ApplicationId: {ApplicationId}", request.ApplicationId);
                return StatusCode(500, new { Message = "Internal server error during update", Detail = ex.Message });
            }
        }
    }

    public class AiScoreUpdateRequest
    {
        public required int ApplicationId { get; set; }
        public required int AtsScore { get; set; }
        public required string AiSummary { get; set; }
    }
}
