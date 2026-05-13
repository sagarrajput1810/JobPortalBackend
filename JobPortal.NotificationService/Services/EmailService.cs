using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobPortal.NotificationService.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            var fromEmail = _config["EmailSettings:FromEmail"] ?? "no-reply@jobportal.com";
            email.From.Add(MailboxAddress.Parse(fromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            try
            {
                var host = _config["EmailSettings:SmtpServer"] ?? "localhost";
                var port = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var user = _config["EmailSettings:Username"] ?? "";
                var pass = _config["EmailSettings:Password"] ?? "";

                _logger.LogInformation(
                    "Connecting to SMTP server {Host}:{Port}. Username configured: {HasUsername}. Password configured: {HasPassword}",
                    host,
                    port,
                    !string.IsNullOrWhiteSpace(user),
                    !string.IsNullOrWhiteSpace(pass));
                
                // Bypass certificate validation if necessary (common in some environments)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(user))
                {
                    await smtp.AuthenticateAsync(user, pass);
                }
                await smtp.SendAsync(email);
                
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw; // Re-throw to allow MassTransit to retry
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}
