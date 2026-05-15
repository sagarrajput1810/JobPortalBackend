using JobPortal.ApplicationService.Data;
using JobPortal.ApplicationService.DTOs;
using JobPortal.ApplicationService.Models;
using JobPortal.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.ApplicationService.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<ApplicationService> _logger;
        private static readonly TimeSpan EventSendTimeout = TimeSpan.FromSeconds(30);

        public ApplicationService(
            ApplicationDbContext context,
            ISendEndpointProvider sendEndpointProvider,
            ILogger<ApplicationService> logger)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }

        public async Task<JobApplicationResponseDto> ApplyForJobAsync(JobApplicationCreateDto applicationDto, string candidateId, string candidateName, string candidateEmail)
        {
            // Check if already applied
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.JobId == applicationDto.JobId && a.CandidateId == candidateId);
            
            if (existingApplication != null)
            {
                throw new InvalidOperationException("You have already applied for this job.");
            }

            var application = new JobApplication
            {
                JobId = applicationDto.JobId,
                JobTitle = applicationDto.JobTitle,
                CompanyName = applicationDto.CompanyName,
                CandidateId = candidateId,
                CandidateName = candidateName,
                CandidateEmail = candidateEmail,
                ResumeUrl = applicationDto.ResumeUrl,
                CoverLetter = applicationDto.CoverLetter,
                AppliedDate = DateTime.UtcNow,
                Status = "Applied"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            var appEvent = new JobAppliedEvent(
                application.Id, 
                application.JobId, 
                application.CandidateEmail, 
                application.CandidateName, 
                application.ResumeUrl,
                application.JobTitle,
                application.CompanyName
            );

            // Swap order: Try AI first, then Notification
            await TrySendEventAsync(new Uri("queue:job-applied-event-ai"), appEvent);
            await TrySendEventAsync(new Uri("queue:job-applied-event-notification"), appEvent);

            return MapToDto(application);
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByJobIdAsync(int jobId)
        {
            // Sorting by AtsScore Descending so high score comes first
            var apps = await _context.JobApplications
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AtsScore) 
                .ToListAsync();
            return apps.Select(MapToDto);
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCandidateIdAsync(string candidateId)
        {
            var apps = await _context.JobApplications.Where(a => a.CandidateId == candidateId).ToListAsync();
            return apps.Select(MapToDto);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int id, string status)
        {
            var application = await _context.JobApplications.FindAsync(id);
            if (application == null) return false;

            application.Status = status;
            await _context.SaveChangesAsync();

            await TrySendEventAsync(new Uri("queue:application-status-updated-event-notification"), new ApplicationStatusUpdatedEvent(
                application.Id,
                application.CandidateEmail,
                "Your Application", 
                status
            ));

            return true;
        }

        public async Task<JobApplicationResponseDto?> GetApplicationByIdAsync(int id)
        {
            var application = await _context.JobApplications.FindAsync(id);
            return application != null ? MapToDto(application) : null;
        }

        private JobApplicationResponseDto MapToDto(JobApplication application)
        {
            return new JobApplicationResponseDto
            {
                Id = application.Id,
                JobId = application.JobId,
                JobTitle = application.JobTitle,
                CompanyName = application.CompanyName,
                CandidateId = application.CandidateId,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                ResumeUrl = application.ResumeUrl,
                Status = application.Status,
                AppliedDate = application.AppliedDate,
                CoverLetter = application.CoverLetter,
                AtsScore = application.AtsScore,
                AiSummary = application.AiSummary
            };
        }

        private async Task TrySendEventAsync<T>(Uri endpointUri, T message) where T : class
        {
            try
            {
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(endpointUri);
                await endpoint.Send(message);
                _logger.LogInformation("Successfully sent event of type {MessageType} to {Endpoint}", typeof(T).Name, endpointUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application saved, but failed to send event of type {MessageType} to {Endpoint}", typeof(T).Name, endpointUri);
            }
        }
    }
}
