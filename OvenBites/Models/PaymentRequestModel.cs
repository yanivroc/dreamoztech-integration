using System.Text.Json.Serialization;

namespace OvenBites.Models
{
    public class PaymentRequestModel
    {
        [JsonPropertyName("cartItems")]
        public List<CartItemModel> CartItems { get; set; }

        [JsonPropertyName("summary")]
        public SummaryModel Summary { get; set; }

        [JsonPropertyName("customerDetails")]
        public CustomerDetailsModel CustomerDetails { get; set; }

        [JsonPropertyName("paymentToken")] 
        public string PaymentToken { get; set; }

        [JsonPropertyName("recaptchaResponse")]
        public string RecaptchaResponse { get; set; }
    }
}
