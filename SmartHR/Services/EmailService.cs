using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmartHR.Services
{
    /// <summary>
    /// SMTP email service. Configure via appsettings.json under "EmailSettings".
    /// Falls back to logging only when Host is not configured (development mode).
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host     = _config["EmailSettings:Host"];
            var portStr  = _config["EmailSettings:Port"];
            var from     = _config["EmailSettings:FromEmail"] ?? "noreply@smarthr.com";
            var password = _config["EmailSettings:Password"];
            var fromName = _config["EmailSettings:FromName"] ?? "SmartHR System";

            // If SMTP is not configured (development / demo environment), log only.
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning(
                    "[EmailService] SMTP host not configured. " +
                    "Would have sent to={To} Subject={Subject}", toEmail, subject);
                return;
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            using var message = new MailMessage
            {
                From       = new MailAddress(from, fromName),
                Subject    = subject,
                Body       = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(from, password)
            };

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("[EmailService] Email sent to {To}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] Failed to send email to {To}", toEmail);
                throw;
            }
        }
    }
}
