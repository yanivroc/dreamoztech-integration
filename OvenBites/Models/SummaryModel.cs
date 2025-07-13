using System.Text.Json.Serialization;

namespace OvenBites.Models
{
    public class SummaryModel
    {
        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("deliveryCost")]
        public decimal DeliveryCost { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }
}
