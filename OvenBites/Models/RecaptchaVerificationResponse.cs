using System.Text.Json.Serialization;

namespace OvenBites.Models
{
    public class RecaptchaVerificationResponse
    {
        [JsonPropertyName("success")] // Maps to "success" in the JSON response from Google
        public bool Success { get; set; }

        [JsonPropertyName("challenge_ts")] // Timestamp of the challenge load (ISO format yyyy-MM-ddTHH:mm:ssZ)
        public string ChallengeTs { get; set; }

        [JsonPropertyName("hostname")] // Hostname of the site where the reCAPTCHA was solved
        public string Hostname { get; set; }

        [JsonPropertyName("error-codes")] // Optional: array of error codes
        public List<string> ErrorCodes { get; set; }
    }
}
