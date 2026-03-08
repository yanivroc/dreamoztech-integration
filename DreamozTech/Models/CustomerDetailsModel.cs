using System.Text.Json.Serialization;

namespace DreamozTech.Models
{
    public class CustomerDetailsModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }
}
