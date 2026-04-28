namespace JobPortal.NotificationService.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Abhi ke liye sirf log kar rahe hain. 
            // Baad mein SendGrid, MailKit, ya AWS SES ka code yahan aayega.
            _logger.LogInformation($"[SENDING EMAIL] To: {to}, Subject: {subject}");
            _logger.LogInformation($"[BODY]: {body}");

            await Task.Delay(100); // Mock delay
        }
    }
}
