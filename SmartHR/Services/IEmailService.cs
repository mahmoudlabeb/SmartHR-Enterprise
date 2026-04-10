namespace SmartHR.Services
{
    /// <summary>
    /// Contract for sending transactional emails from SmartHR.
    /// Register a concrete implementation in Program.cs via DI.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>Send a plain-text or HTML email.</summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="body">HTML body content.</param>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
