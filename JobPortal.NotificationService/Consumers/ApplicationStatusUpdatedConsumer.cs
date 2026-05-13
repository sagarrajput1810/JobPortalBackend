using JobPortal.Shared.Events;
using JobPortal.NotificationService.Services;
using JobPortal.NotificationService.Data;
using JobPortal.NotificationService.Models;
using MassTransit;

namespace JobPortal.NotificationService.Consumers
{
    public class ApplicationStatusUpdatedConsumer : IConsumer<ApplicationStatusUpdatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ApplicationStatusUpdatedConsumer> _logger;
        private readonly NotificationDbContext _dbContext;

        public ApplicationStatusUpdatedConsumer(IEmailService emailService, ILogger<ApplicationStatusUpdatedConsumer> logger, NotificationDbContext dbContext)
        {
            _emailService = emailService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<ApplicationStatusUpdatedEvent> context)
        {
            var msg = context.Message;
            string subject = $"Status Updated: {msg.JobTitle}";
            string body = $"Hi, your application for '{msg.JobTitle}' has been updated to: {msg.NewStatus}.";

            _logger.LogInformation($"Processing ApplicationStatusUpdatedEvent for {msg.CandidateEmail}");
            
            // 1. Send Email
            await _emailService.SendEmailAsync(msg.CandidateEmail, subject, body);

            // 2. Save In-App Notification
            var notification = new UserNotification
            {
                UserEmail = msg.CandidateEmail,
                Title = "Application Status Updated",
                Message = $"Your application for '{msg.JobTitle}' has been marked as: {msg.NewStatus}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RelatedUrl = "/candidate/applications"
            };

            try
            {
                _dbContext.UserNotifications.Add(notification);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sent, but failed to save status notification for {Email}", msg.CandidateEmail);
            }
        }
    }
}
