using Microsoft.AspNetCore.Mvc;
using OvenBites.Models;
using System.Text.Json;

namespace OvenBites.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsletterController : ControllerBase
    {
        // IMPORTANT: Replace with your actual reCAPTCHA secret key
        // You should store this securely, e.g., in appsettings.json or environment variables
        private readonly string _recaptchaSecretKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Constructor for dependency injection of HttpClientFactory
        public NewsletterController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _recaptchaSecretKey = _configuration["Recaptcha:SecretKey"];
        }

        // This action will handle the POST request from your JavaScript form
        [HttpPost("subscribe")] // This defines the specific route for this action, e.g., /Newsletter/subscribe
        public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscriptionModel model)
        {
            // 1. Basic Server-Side Validation (beyond client-side)
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Name and Email are required." });
            }

            // You might add more robust email validation here
            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new { message = "Invalid email format." });
            }

            // 2. reCAPTCHA Server-Side Verification
            if (string.IsNullOrWhiteSpace(model.RecaptchaToken))
            {
                return BadRequest(new { message = "reCAPTCHA token is missing." });
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _recaptchaSecretKey),
                    new KeyValuePair<string, string>("response", model.RecaptchaToken)
                });

                // Send a POST request to Google's reCAPTCHA verification API
                var recaptchaResponse = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                recaptchaResponse.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var recaptchaResponseBody = await recaptchaResponse.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResponseBody);

                if (verificationResult == null || !verificationResult.Success)
                {
                    // Log the error codes for debugging
                    Console.WriteLine($"reCAPTCHA verification failed. Error codes: {string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>())}");
                    return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors during reCAPTCHA verification
                Console.Error.WriteLine($"Error verifying reCAPTCHA: {ex.Message}");
                return StatusCode(500, new { message = "Error verifying reCAPTCHA. Please try again later." });
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors for reCAPTCHA response
                Console.Error.WriteLine($"Error parsing reCAPTCHA response: {ex.Message}");
                return StatusCode(500, new { message = "Error processing reCAPTCHA response." });
            }

            // 3. Process the Subscription (e.g., save to database, send email)
            // In a real application, you would save 'model.Name' and 'model.Email' to a database
            // or integrate with an email marketing service.
            Console.WriteLine($"Newsletter Subscription: Name={model.Name}, Email={model.Email}");

            // Return a success response that the client-side JavaScript expects
            return Ok(new { message = "Thank you for subscribing!" });
        }

        // Simple email validation helper (you might use a more robust library)
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
