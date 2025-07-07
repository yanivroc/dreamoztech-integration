using System.ComponentModel.DataAnnotations.Schema;

namespace OvenBites.Models
{
    public class InvoiceDto
    {
        public string InvoiceTitle { get; set; } = default!;
        public string InvoicePath { get; set; } = default!;
        public string? InvoiceUrl { get; set; } = default!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceAmount { get; set; }
        public bool IsInvoicePaid { get; set; }
        public DateTime? PaidDateTime { get; set; }
        public int? WebCount { get; set; }
        public int? PostCount { get; set; }
        public int? WebPageCount { get; set; }
        public int? PaymentMode { get; set; }
        public List<OrderDto> Orders { get; set; } = default!;
    }
}
