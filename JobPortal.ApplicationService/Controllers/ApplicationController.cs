using JobPortal.ApplicationService.DTOs;
using JobPortal.ApplicationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace JobPortal.ApplicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IFileService _fileService;
        private readonly ILogger<ApplicationController> _logger;
        private const int MaxResumeUploadSizeBytes = 25 * 1024 * 1024;

        public ApplicationController(
            IApplicationService applicationService,
            IFileService fileService,
            ILogger<ApplicationController> logger)
        {
            _applicationService = applicationService;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("apply")]
        [Authorize]
        [Consumes("multipart/form-data")] // Specify multipart form data
        [RequestSizeLimit(MaxResumeUploadSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxResumeUploadSizeBytes)]
        [SuppressMessage("Security", "S5693:Make sure the content length limit is safe here", Justification = "Resume uploads are limited to 25 MB to allow PDF/DOC resumes while preventing oversized multipart requests.")]
        public async Task<IActionResult> Apply(
            [FromForm] int jobId,
            [FromForm] string? jobTitle,
            [FromForm] string? companyName,
            [FromForm] string? coverLetter,
            [FromForm] IFormFile resume)
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
                    JobTitle = jobTitle ?? string.Empty,
                    CompanyName = companyName ?? string.Empty,
                    CoverLetter = coverLetter,
                    ResumeUrl = resumeUrl // Relative URL saved in DB
                };

                var result = await _applicationService.ApplyForJobAsync(applicationDto, userId, userName ?? "Unknown", userEmail ?? "Unknown");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply for job {JobId} as user {UserId}", jobId, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            try
            {
                var results = await _applicationService.GetApplicationsByJobIdAsync(jobId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load applications for job {JobId}", jobId);
                return Ok(Array.Empty<JobApplicationResponseDto>());
            }
        }

        [HttpGet("my-applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var results = await _applicationService.GetApplicationsByCandidateIdAsync(userId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load applications for candidate {CandidateId}", userId);
                return Ok(Array.Empty<JobApplicationResponseDto>());
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] JobApplicationStatusUpdateDto statusDto)
        {
            var success = await _applicationService.UpdateApplicationStatusAsync(id, statusDto.Status);
            return success ? NoContent() : NotFound();
        }

        // Admin-only: Get ALL applications across all jobs
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var all = await _applicationService.GetAllApplicationsAsync();
                return Ok(all);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin failed to load all applications");
                return Ok(Array.Empty<JobApplicationResponseDto>());
            }
        }

        // Admin-only: Delete any application
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var success = await _applicationService.DeleteApplicationAsync(id);
            return success ? Ok("Application deleted.") : NotFound();
        }
    }
}
