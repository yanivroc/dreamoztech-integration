using System.Threading.Tasks;

namespace DreamozTech.Service
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);

        Task<bool> SendInvoiceEmailAsync(string toEmail, string subject, string message, byte[] invoicePdfBytes, string invoiceFileName);
    }
}
