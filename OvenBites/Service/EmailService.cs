using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using OvenBites.Models;

namespace OvenBites.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailConfig> emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtpClient = new SmtpClient(_emailConfig.SmtpServer)
            {
                Port = _emailConfig.Port,
                Credentials = new NetworkCredential(_emailConfig.Login, _emailConfig.Password),
                EnableSsl = true, 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.EmailFrom), 
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);
            mailMessage.CC.Add("dreamoz.com.au@gmail.com");

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                // Log a more detailed error message
                _logger.LogError(ex, "SMTP Error: Failed to send email to {ToEmail}. Details: {Message}", toEmail, ex.Message);
                return false; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Error: Failed to send email to {ToEmail}. Details: {Message}", toEmail, ex.Message);
                return false;
            }
        }
    }
}
