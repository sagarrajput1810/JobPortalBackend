using JobPortal.Shared.Events;
using JobPortal.NotificationService.Services;
using JobPortal.NotificationService.Data;
using JobPortal.NotificationService.Models;
using MassTransit;

namespace JobPortal.NotificationService.Consumers
{
    public class JobAppliedConsumer : IConsumer<JobAppliedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<JobAppliedConsumer> _logger;
        private readonly NotificationDbContext _dbContext;

        public JobAppliedConsumer(IEmailService emailService, ILogger<JobAppliedConsumer> logger, NotificationDbContext dbContext)
        {
            _emailService = emailService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<JobAppliedEvent> context)
        {
            var msg = context.Message;
            string subject = $"Application Received: {msg.JobTitle}";
            string body = $"Hi {msg.CandidateName}, You have successfully applied for the job '{msg.JobTitle}'.";

            _logger.LogInformation($"Processing JobAppliedEvent for {msg.CandidateEmail}");
            
            // 1. Send Email
            await _emailService.SendEmailAsync(msg.CandidateEmail, subject, body);

            // 2. Save In-App Notification
            var notification = new UserNotification
            {
                UserEmail = msg.CandidateEmail,
                Title = "Job Applied Successfully",
                Message = $"You have successfully applied for the position: {msg.JobTitle} at {msg.CompanyName}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RelatedUrl = $"/candidate/applications"
            };

            try
            {
                _dbContext.UserNotifications.Add(notification);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sent, but failed to save job-applied notification for {Email}", msg.CandidateEmail);
            }
        }
    }
}
