using JobPortal.Shared.Events;
using JobPortal.NotificationService.Services;
using MassTransit;

namespace JobPortal.NotificationService.Consumers
{
    public class JobAppliedConsumer : IConsumer<JobAppliedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<JobAppliedConsumer> _logger;

        public JobAppliedConsumer(IEmailService emailService, ILogger<JobAppliedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<JobAppliedEvent> context)
        {
            var msg = context.Message;
            string subject = $"Application Received: {msg.JobTitle}";
            string body = $"Hi {msg.CandidateName}, You have successfully applied for the job '{msg.JobTitle}'.";

            _logger.LogInformation($"Processing JobAppliedEvent for {msg.CandidateEmail}");
            await _emailService.SendEmailAsync(msg.CandidateEmail, subject, body);
        }
    }
}
