using JobPortal.ApplicationService.DTOs;
using JobPortal.ApplicationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobPortal.ApplicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IFileService _fileService;

        public ApplicationController(IApplicationService applicationService, IFileService fileService)
        {
            _applicationService = applicationService;
            _fileService = fileService;
        }

        [HttpPost("apply")]
        [Authorize]
        [Consumes("multipart/form-data")] // Specify multipart form data
        public async Task<IActionResult> Apply([FromForm] int jobId, [FromForm] string? coverLetter, [FromForm] IFormFile resume)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            try
            {
                if (resume == null) return BadRequest("Resume file is required");

                // 1. Save File to Local Storage
                string resumeUrl = await _fileService.SaveFileAsync(resume, "resumes");

                // 2. Map to DTO for Service
                var applicationDto = new JobApplicationCreateDto
                {
                    JobId = jobId,
                    CoverLetter = coverLetter,
                    ResumeUrl = resumeUrl // Relative URL saved in DB
                };

                var result = await _applicationService.ApplyForJobAsync(applicationDto, userId, userName ?? "Unknown", userEmail ?? "Unknown");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            var results = await _applicationService.GetApplicationsByJobIdAsync(jobId);
            return Ok(results);
        }

        [HttpGet("my-applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var results = await _applicationService.GetApplicationsByCandidateIdAsync(userId);
            return Ok(results);
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] JobApplicationStatusUpdateDto statusDto)
        {
            var success = await _applicationService.UpdateApplicationStatusAsync(id, statusDto.Status);
            return success ? Ok("Status updated") : NotFound();
        }
    }
}
