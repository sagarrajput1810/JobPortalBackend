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

        public ApplicationService(ApplicationDbContext context, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<JobApplicationResponseDto> ApplyForJobAsync(JobApplicationCreateDto applicationDto, string candidateId, string candidateName, string candidateEmail)
        {
            // Check if already applied
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.JobId == applicationDto.JobId && a.CandidateId == candidateId);
            
            if (existingApplication != null)
            {
                throw new Exception("You have already applied for this job.");
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

            // Send to Notification Queue
            var notificationEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:job-applied-event-notification"));
            await notificationEndpoint.Send(appEvent);

            // Send to AI Parser Queue
            var aiEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:job-applied-event-ai"));
            await aiEndpoint.Send(appEvent);

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

            // Send to Notification Queue
            var notificationEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:application-status-updated-event-notification"));
            await notificationEndpoint.Send(new ApplicationStatusUpdatedEvent(
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
    }
}
