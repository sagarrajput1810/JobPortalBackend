using JobPortal.Shared.Events;
using JobPortal.NotificationService.Services;
using MassTransit;

namespace JobPortal.NotificationService.Consumers
{
    public class ApplicationStatusUpdatedConsumer : IConsumer<ApplicationStatusUpdatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ApplicationStatusUpdatedConsumer> _logger;

        public ApplicationStatusUpdatedConsumer(IEmailService emailService, ILogger<ApplicationStatusUpdatedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ApplicationStatusUpdatedEvent> context)
        {
            var msg = context.Message;
            string subject = $"Status Updated: {msg.JobTitle}";
            string body = $"Hi, your application for '{msg.JobTitle}' has been updated to: {msg.NewStatus}.";

            _logger.LogInformation($"Processing ApplicationStatusUpdatedEvent for {msg.CandidateEmail}");
            await _emailService.SendEmailAsync(msg.CandidateEmail, subject, body);
        }
    }
}
