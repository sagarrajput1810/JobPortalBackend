using JobPortal.JobService.DTOs;
using JobPortal.JobService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobPortal.JobService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobResponseDto>> GetJobById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        public async Task<ActionResult<JobResponseDto>> CreateJob(JobCreateDto jobCreateDto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var createdJob = await _jobService.CreateJobAsync(jobCreateDto, recruiterId);
            return CreatedAtAction(nameof(GetJobById), new { id = createdJob.Id }, createdJob);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, JobUpdateDto jobUpdateDto)
        {
            // Optional: Check if the recruiter owns the job
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();
            
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (job.RecruiterId != recruiterId) return Forbid();

            var result = await _jobService.UpdateJobAsync(id, jobUpdateDto);
            if (!result) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Recruiter")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();

            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (job.RecruiterId != recruiterId) return Forbid();

            var result = await _jobService.DeleteJobAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("recruiter/{recruiterId}")]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetJobsByRecruiter(string recruiterId)
        {
            var jobs = await _jobService.GetJobsByRecruiterAsync(recruiterId);
            return Ok(jobs);
        }
    }
}