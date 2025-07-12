using Microsoft.AspNetCore.Mvc;
using OvenBites.Models;
using System.Text.Json;

namespace OvenBites.Controllers
{
    [ApiController]
    [Route("[controller]")] // This means the base route for this controller is /Newsletter
    public class NewsletterController : ControllerBase
    {
        private readonly string _recaptchaSecretKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public NewsletterController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _recaptchaSecretKey = _configuration["Recaptcha:SecretKey"]; // Get secret key from configuration
        }

        // Action for Newsletter Subscription
        [HttpPost("subscribe")] // Full route: /Newsletter/subscribe
        public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscriptionModel model)
        {
            // 1. Basic Server-Side Validation
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Name and Email are required." });
            }

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

                var recaptchaResponse = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                recaptchaResponse.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var recaptchaResponseBody = await recaptchaResponse.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResponseBody);

                if (verificationResult == null || !verificationResult.Success)
                {
                    Console.WriteLine($"reCAPTCHA verification failed for subscribe. Error codes: {string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>())}");
                    return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Error verifying reCAPTCHA for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "Error verifying reCAPTCHA. Please try again later." });
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error parsing reCAPTCHA response for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "Error processing reCAPTCHA response." });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during reCAPTCHA verification for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during reCAPTCHA verification." });
            }

            // 3. Process the Subscription
            // In a real application, you would save 'model.Name' and 'model.Email' to a database
            // or integrate with an email marketing service.
            Console.WriteLine($"Newsletter Subscription: Name={model.Name}, Email={model.Email}");

            return Ok(new { message = "Thank you for subscribing!" });
        }


        // NEW ACTION FOR CONTACT FORM
        [HttpPost("contact")] // Full route: /Newsletter/contact
        public async Task<IActionResult> Contact([FromBody] ContactFormModel model)
        {
            // 1. Basic Server-Side Validation
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest(new { message = "Name, Email, and Message are required." });
            }

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

                var recaptchaResponse = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                recaptchaResponse.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var recaptchaResponseBody = await recaptchaResponse.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResponseBody);

                if (verificationResult == null || !verificationResult.Success)
                {
                    Console.WriteLine($"reCAPTCHA verification failed for contact form. Error codes: {string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>())}");
                    return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Error verifying reCAPTCHA for contact form: {ex.Message}");
                return StatusCode(500, new { message = "Error verifying reCAPTCHA. Please try again later." });
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error parsing reCAPTCHA response for contact form: {ex.Message}");
                return StatusCode(500, new { message = "Error processing reCAPTCHA response." });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during reCAPTCHA verification for contact form: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during reCAPTCHA verification." });
            }

            // 3. Process the Contact Form (e.g., send an email, save to database)
            // In a real application, you would implement your email sending logic here.
            // You might inject an IEmailSender service.
            Console.WriteLine($"Contact Form Submission: Name={model.Name}, Email={model.Email}, Message={model.Message}");

            // Example:
            // var emailSent = await _emailSender.SendEmailAsync(
            //     "your_receiving_email@example.com", // Your email address
            //     $"New Contact from {model.Name} ({model.Email})",
            //     model.Message
            // );

            // if (emailSent)
            // {
            return Ok(new { message = "Your message has been sent successfully!" });
            // }
            // else
            // {
            //    return StatusCode(500, new { message = "Failed to send your message. Please try again." });
            // }
        }

        // Helper method for email validation
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