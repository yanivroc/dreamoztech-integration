using System.Text.Json.Serialization;

namespace OvenBites.Models
{
    public class ContactFormModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("recaptchaToken")]
        public string RecaptchaToken { get; set; }
        public int? MemberId { get; set; }
    }
}
