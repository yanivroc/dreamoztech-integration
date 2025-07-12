namespace OvenBites.Models
{
    public class RecaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}
