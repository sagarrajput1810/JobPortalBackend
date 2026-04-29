using JobPortal.Shared.Events;
using MassTransit;
using JobPortal.NotificationService.Services;

namespace JobPortal.NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredConsumer> _logger;
        private readonly IEmailService _emailService;

        public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var userEvent = context.Message;
            _logger.LogInformation($"Processing UserRegisteredEvent for: {userEvent.Email}");

            string subject = "Welcome to JobPortal - Verify Your Email";
            string body = $@"
                <h1>Welcome to JobPortal!</h1>
                <p>Thank you for registering as a <strong>{userEvent.Role}</strong>.</p>
                <p>Your OTP for email verification is:</p>
                <h2 style='color: #007bff;'>{userEvent.Otp}</h2>
                <p>This OTP is valid for 15 minutes.</p>
                <br/>
                <p>Best Regards,<br/>JobPortal Team</p>";

            await _emailService.SendEmailAsync(userEvent.Email, subject, body);
        }
    }
}