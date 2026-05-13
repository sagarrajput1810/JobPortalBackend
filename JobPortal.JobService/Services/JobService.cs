using JobPortal.JobService.Data;
using JobPortal.JobService.DTOs;
using JobPortal.JobService.Models;
using JobPortal.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.JobService.Services
{
    public class JobService : IJobService
    {
        private readonly JobDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<JobService> _logger;
        private static readonly TimeSpan EventSendTimeout = TimeSpan.FromSeconds(5);

        public JobService(JobDbContext context, ISendEndpointProvider sendEndpointProvider, ILogger<JobService> logger)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<JobResponseDto>> GetAllJobsAsync()
        {
            var jobs = await _context.JobPostings.Where(j => j.IsActive).ToListAsync();
            return jobs.Select(MapToResponseDto);
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int id)
        {
            var job = await _context.JobPostings.FindAsync(id);
            return job != null ? MapToResponseDto(job) : null;
        }

        public async Task<JobResponseDto> CreateJobAsync(JobCreateDto jobCreateDto, string recruiterId)
        {
            var job = new JobPosting
            {
                Title = jobCreateDto.Title,
                Description = jobCreateDto.Description,
                CompanyName = jobCreateDto.CompanyName,
                Location = jobCreateDto.Location,
                Salary = jobCreateDto.Salary,
                RecruiterId = recruiterId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.JobPostings.Add(job);
            await _context.SaveChangesAsync();

            await TrySendSearchEventAsync(new Uri("queue:job-created-event-search"), new JobCreatedEvent(
                job.Id,
                job.Title,
                job.Description,
                job.CompanyName,
                job.Location,
                job.Salary,
                job.RecruiterId
            ));

            return MapToResponseDto(job);
        }

        public async Task<bool> UpdateJobAsync(int id, JobUpdateDto jobUpdateDto)
        {
            var job = await _context.JobPostings.FindAsync(id);
            if (job == null) return false;

            job.Title = jobUpdateDto.Title;
            job.Description = jobUpdateDto.Description;
            job.CompanyName = jobUpdateDto.CompanyName;
            job.Location = jobUpdateDto.Location;
            job.Salary = jobUpdateDto.Salary;
            job.IsActive = jobUpdateDto.IsActive;

            await _context.SaveChangesAsync();

            await TrySendSearchEventAsync(new Uri("queue:job-updated-event-search"), new JobUpdatedEvent(
                job.Id,
                job.Title,
                job.Description,
                job.CompanyName,
                job.Location,
                job.Salary,
                job.IsActive
            ));

            return true;
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _context.JobPostings.FindAsync(id);
            if (job == null) return false;

            // Soft delete
            job.IsActive = false;
            await _context.SaveChangesAsync();

            await TrySendSearchEventAsync(new Uri("queue:job-deleted-event-search"), new JobDeletedEvent(job.Id));

            return true;
        }

        public async Task<IEnumerable<JobResponseDto>> GetJobsByRecruiterAsync(string recruiterId)
        {
            var jobs = await _context.JobPostings
                .Where(j => j.RecruiterId == recruiterId)
                .ToListAsync();
            return jobs.Select(MapToResponseDto);
        }

        private JobResponseDto MapToResponseDto(JobPosting job)
        {
            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                CompanyName = job.CompanyName,
                Location = job.Location,
                Salary = job.Salary,
                RecruiterId = job.RecruiterId,
                CreatedAt = job.CreatedAt,
                IsActive = job.IsActive
            };
        }

        private async Task TrySendSearchEventAsync<T>(Uri endpointUri, T message) where T : class
        {
            try
            {
                var searchEndpoint = await _sendEndpointProvider.GetSendEndpoint(endpointUri).WaitAsync(EventSendTimeout);
                await searchEndpoint.Send(message).WaitAsync(EventSendTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job saved, but failed to send search event to {Endpoint}", endpointUri);
            }
        }
    }
}
