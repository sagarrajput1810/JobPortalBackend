using JobPortal.Shared.Events;
using MassTransit;
using JobPortal.NotificationService.Services;
using JobPortal.NotificationService.Data;
using JobPortal.NotificationService.Models;

namespace JobPortal.NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredConsumer> _logger;
        private readonly IEmailService _emailService;
        private readonly NotificationDbContext _dbContext;

        public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IEmailService emailService, NotificationDbContext dbContext)
        {
            _logger = logger;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var userEvent = context.Message;
            Console.WriteLine($"[NotificationService] Consumer received event for: {userEvent.Email}");
            _logger.LogInformation($"Processing UserRegisteredEvent for: {userEvent.Email}");

            // 1. Send Email
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

            // 2. Save In-App Notification
            var notification = new UserNotification
            {
                UserEmail = userEvent.Email,
                Title = "Welcome to JobPortal!",
                Message = $"Hi! Your registration as a {userEvent.Role} was successful. Please verify your email using OTP: {userEvent.Otp}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.UserNotifications.Add(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}