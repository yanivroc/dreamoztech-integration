using System.Text.Json.Serialization;

namespace OvenBites.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int BizId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal Total { get; set; }
        public DateTime CreateDateTime { get; set; }
        public bool IsShipped { get; set; }
        public DateTime? ShippedDateTime { get; set; }
        public string? Notes { get; set; } = default!;
        public int InvoiceId { get; set; }
        public bool ShipToStore { get; set; }
        public string? SubItemsNotes { get; set; } = default!;
        public bool IsRefunded { get; set; }
        public DateTime? RefundDateTime { get; set; }
        public string? RefundNotes { get; set; } = default!;
    }
}
