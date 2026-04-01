using DreamozTech.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace DreamozTech.Service
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

        // New: Send an email with a single PDF invoice attachment
        public async Task<bool> SendInvoiceEmailAsync(string toEmail, string subject, string message, byte[] invoicePdfBytes, string invoiceFileName)
        {
            if (invoicePdfBytes == null || invoicePdfBytes.Length == 0)
            {
                _logger.LogWarning("SendInvoiceEmailAsync called with empty invoice bytes for {ToEmail}", toEmail);
                return false;
            }

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

            try
            {
                // Create attachment from byte[] and add to message
                using var ms = new MemoryStream(invoicePdfBytes);
                // Fallback to provided filename or default
                var fileName = string.IsNullOrWhiteSpace(invoiceFileName) ? "invoice.pdf" : invoiceFileName;
                var attachment = new Attachment(ms, fileName, MediaTypeNames.Application.Pdf);
                // Disable stream disposal on attachment disposal so MemoryStream is closed only once by using block
                attachment.ContentDisposition.Inline = false;
                mailMessage.Attachments.Add(attachment);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Invoice email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP Error: Failed to send invoice email to {ToEmail}. Details: {Message}", toEmail, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Error: Failed to send invoice email to {ToEmail}. Details: {Message}", toEmail, ex.Message);
                return false;
            }
        }
    }
}
