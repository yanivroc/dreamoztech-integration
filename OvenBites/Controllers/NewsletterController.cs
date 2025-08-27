using Microsoft.AspNetCore.Mvc;
using OvenBites.Models;
using OvenBites.Service;
using Square;
using Square.Exceptions;
using Square.Models;
using System.Globalization;
using System.Text.Json;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OvenBites.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsletterController : ControllerBase
    {
        private readonly string _recaptchaSecretKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IDataService _dataService;
        private readonly SquareClient _squareClient;
        private readonly string _locationId;
        private readonly string _captchaSiteVerifyUrl;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IEmailService emailService, IDataService dataService, ILogger<NewsletterController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _recaptchaSecretKey = _configuration["GoogleReCaptcha:SecretKey"];
            _captchaSiteVerifyUrl = _configuration["GoogleReCaptcha:CaptchaVerifyUrl"];
            _emailService = emailService;
            _dataService = dataService;

            var accessToken = _configuration["Square:AccessToken"];
            var environmentString = _configuration["Square:Environment"];

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException("Square:AccessToken is not configured in appsettings.json");
            }

            // Determine Square Environment based on string from config
            Square.Environment squareEnvironment;
            if (!Enum.TryParse<Square.Environment>(environmentString, true, out squareEnvironment))
            {
                // Default to Sandbox if parsing fails or config is missing/invalid
                squareEnvironment = Square.Environment.Sandbox;
                _logger.LogWarning($"Warning: Square:Environment config value '{environmentString}' is invalid. Defaulting to Sandbox.");
            }

            // SquareClient instantiation using Builder pattern for v22.0.0
            _squareClient = new SquareClient.Builder()
                .Environment(squareEnvironment) // Square.Environment enum should be recognized now
                .AccessToken(accessToken)
                .UserAgentDetail("OvenBites_App_Csharp_Payment") // Custom user agent for your app
                .Build();

            _locationId = _configuration["Square:LocationId"];
            if (string.IsNullOrEmpty(_locationId))
            {
                throw new ArgumentNullException("Square:LocationId is not configured in appsettings.json");
            }

            _dataService = dataService;
            _logger = logger;
        }

        // ACTION FOR CONTACT FORM
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

                var recaptchaResponse = await httpClient.PostAsync(_captchaSiteVerifyUrl, content);
                recaptchaResponse.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var recaptchaResponseBody = await recaptchaResponse.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResponseBody);

                if (verificationResult == null || !verificationResult.Success)
                {
                    _logger.LogError($"reCAPTCHA verification failed for contact form. Error codes: {string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>())}");
                    return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error verifying reCAPTCHA for contact form: {ex.Message}");
                return StatusCode(500, new { message = "Error verifying reCAPTCHA. Please try again later." });
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error parsing reCAPTCHA response for contact form: {ex.Message}");
                return StatusCode(500, new { message = "Error processing reCAPTCHA response." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred during reCAPTCHA verification for contact form: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during reCAPTCHA verification." });
            }

            // 3. Send email
            var memberDetailObject = await _dataService.GetMemberDetailsAsync();
            var receivingEmail = memberDetailObject.MemberEmail;
            var emailBody = $"<h3>New contact from {MakeNameCapitals(model.Name)}</h3>" +
                            $"<p><strong>Email:</strong> {model.Email}</p>" +
                            $"<p><strong>Message:</strong> {model.Message}</p>";
            var emailSent = await _emailService.SendEmailAsync(
                                receivingEmail,
                                $"New contact from {MakeNameCapitals(model.Name)}",
                                emailBody
                            );
            if (emailSent)
            {
                return Ok(new { message = "Your message has been sent successfully!" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to send your message. Please try again." });
            }
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

                var recaptchaResponse = await httpClient.PostAsync(_captchaSiteVerifyUrl, content);
                recaptchaResponse.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var recaptchaResponseBody = await recaptchaResponse.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResponseBody);

                if (verificationResult == null || !verificationResult.Success)
                {
                    _logger.LogError($"reCAPTCHA verification failed for subscribe. Error codes: {string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>())}");
                    return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error verifying reCAPTCHA for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "Error verifying reCAPTCHA. Please try again later." });
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error parsing reCAPTCHA response for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "Error processing reCAPTCHA response." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred during reCAPTCHA verification for subscribe: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during reCAPTCHA verification." });
            }

            // 3. Send email
            var memberDetailObject = await _dataService.GetMemberDetailsAsync();
            var receivingEmail = memberDetailObject.MemberEmail;
            var emailBody = $"<h3>New subscription from {MakeNameCapitals(model.Name)}</h3>" +
                            $"<p><strong>Email:</strong> {model.Email}</p>";
            var emailSent = await _emailService.SendEmailAsync(
                                receivingEmail,
                                $"New subscription from {MakeNameCapitals(model.Name)}",
                                emailBody
                            );
            if (emailSent)
            {
                return Ok(new { message = "Thank you for subscribing!" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to subscribe. Please try again." });
            }
        }

        /// <summary>
        /// Processes a payment request using Square Payments API.
        /// Receives a payment token from the client-side.
        /// </summary>
        /// <param name="model">The payment request model containing order and customer details, and the Square payment token.</param>
        /// <returns>IActionResult indicating success or failure.</returns>
        [HttpPost("pay")] // Full route: /Newsletter/pay
        public async Task<IActionResult> Pay([FromBody] PaymentRequestModel model)
        {
            // --- 1. Server-Side Validation of Input Data ---
            if (model == null)
            {
                return BadRequest(new { message = "Invalid request payload." });
            }

            // Validate Customer Details
            if (string.IsNullOrWhiteSpace(model.CustomerDetails?.Name)) return BadRequest(new { message = "Customer name is required." });
            if (string.IsNullOrWhiteSpace(model.CustomerDetails?.Email)) return BadRequest(new { message = "Customer email is required." });
            if (!IsValidEmail(model.CustomerDetails.Email)) return BadRequest(new { message = "Invalid customer email format." });
            if (string.IsNullOrWhiteSpace(model.CustomerDetails?.Phone)) return BadRequest(new { message = "Customer phone is required." });
            if (string.IsNullOrWhiteSpace(model.CustomerDetails?.Address)) return BadRequest(new { message = "Customer address is required." });

            // Validate Cart Items (optional, but good practice)
            if (model.CartItems == null || !model.CartItems.Any()) return BadRequest(new { message = "Cart is empty." });
            foreach (var item in model.CartItems)
            {
                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name) || item.Price <= 0 || item.Quantity <= 0)
                {
                    return BadRequest(new { message = "Invalid cart item details." });
                }
            }

            // Validate Summary amounts (optional, but good for consistency)
            if (model.Summary == null || model.Summary.Subtotal < 0 || model.Summary.Total < 0)
            {
                return BadRequest(new { message = "Invalid summary amounts." });
            }
            // IMPORTANT: Recalculate total on server to prevent client-side tampering
            decimal calculatedSubtotal = model.CartItems.Sum(item => item.Price * item.Quantity);
            decimal calculatedDeliveryCost = 0;
            if (model.Summary.DeliveryCost > 0)
            {
                int totalQuantity = model.CartItems.Sum(item => item.Quantity);
                if (totalQuantity <= 6) calculatedDeliveryCost = 9.99m;
                else if (totalQuantity > 6 && totalQuantity <= 12) calculatedDeliveryCost = 12.99m;
                else calculatedDeliveryCost = 15.00m;
            }
            decimal calculatedTotal = calculatedSubtotal + calculatedDeliveryCost;

            if (calculatedTotal != model.Summary.Total || calculatedSubtotal != model.Summary.Subtotal || calculatedDeliveryCost != model.Summary.DeliveryCost)
            {
                // This indicates potential tampering or a client-side calculation error.
                // You might want to log this for investigation.
                return BadRequest(new { message = "Price mismatch. Please refresh and try again." });
            }

            // --- 2. reCAPTCHA Server-Side Verification ---
            if (string.IsNullOrWhiteSpace(model.RecaptchaResponse))
            {
                return BadRequest(new { message = "reCAPTCHA response is missing." });
            }

            var recaptchaSecretKey = _configuration["GoogleReCaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(recaptchaSecretKey))
            {
                return StatusCode(500, new { message = "reCAPTCHA secret key not configured on server." });
            }

            var httpClient = _httpClientFactory.CreateClient(); // Use a local variable to avoid confusion with _squareClient
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", recaptchaSecretKey),
                new KeyValuePair<string, string>("response", model.RecaptchaResponse)
            });

            var recaptchaResponse = await httpClient.PostAsync(_captchaSiteVerifyUrl, content);
            recaptchaResponse.EnsureSuccessStatusCode();

            var recaptchaResultString = await recaptchaResponse.Content.ReadAsStringAsync();
            var recaptchaVerification = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(recaptchaResultString);

            if (recaptchaVerification == null || !recaptchaVerification.Success)
            {
                Console.WriteLine($"reCAPTCHA verification failed. Errors: {string.Join(", ", recaptchaVerification?.ErrorCodes ?? new List<string>())}");
                return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
            }

            // --- 3. Square Payment Processing ---
            // Capitalize the first letter of each word
            var capitalizedCustomerName = MakeNameCapitals(model.CustomerDetails.Name); 

            // The nonce (paymentToken) is generated by Square's Web Payments SDK on the client-side.
            if (string.IsNullOrWhiteSpace(model.PaymentToken))
            {
                return BadRequest(new { message = "Payment token is missing." });
            }

            // Convert total amount to cents (or smallest currency unit) as required by Square
            long amountInCents = (long)(model.Summary.Total * 100);

            // Money instantiation using Builder pattern (v22.0.0 syntax)
            var amountMoney = new Money.Builder()
                .Amount(amountInCents)
                .Currency("AUD") // Specify Australian Dollars
                .Build();

            // Generate a unique idempotency key to prevent duplicate charges
            string idempotencyKey = Guid.NewGuid().ToString();

            // CreatePaymentRequest instantiation using Builder pattern (v22.0.0 syntax)
            var createPaymentRequest = new CreatePaymentRequest.Builder(
                sourceId: model.PaymentToken,
                idempotencyKey: idempotencyKey,
                amountMoney: amountMoney)
                .LocationId(_locationId) // Use the injected locationId
                .BuyerEmailAddress(model.CustomerDetails.Email)
                .Note($"Order for {capitalizedCustomerName}")
                .Build();

            try
            {
                // PaymentsApi is correctly accessed via _squareClient.
                // This property is available on the SquareClient instance.
                var createPaymentResponse = await _squareClient.PaymentsApi.CreatePaymentAsync(createPaymentRequest);

                if (createPaymentResponse.Errors != null && createPaymentResponse.Errors.Any())
                {
                    // Log Square API errors
                    foreach (var error in createPaymentResponse.Errors)
                    {
                        _logger.LogError($"Square API Error: Category={error.Category}, Code={error.Code}, Detail={error.Detail}, Field={error.Field}");
                    }
                    // Return a generic error to the client for security
                    return StatusCode(500, new { message = "Payment failed. Please try again or contact support. (Square API Error)" });
                }

                // Payment successful
                var payment = createPaymentResponse.Payment;
                Console.WriteLine($"Square Payment Successful. Payment ID: {payment.Id}, Status: {payment.Status}");

                var memberDetailObject = await _dataService.GetMemberDetailsAsync();
                var companyName = memberDetailObject.MemberFullName;
                var memberEmail = memberDetailObject.MemberEmail;

                // Define a variable for the fulfillment message
                string fulfillmentMessage;

                if (calculatedDeliveryCost > 0)
                {
                    // If there is a delivery cost, it means the customer chose delivery.
                    fulfillmentMessage = $"We'll notify you once your order has been shipped. If you have any questions, please reply to this email. {memberEmail}";
                }
                else
                {
                    // If the delivery cost is zero, it means the customer chose pickup.
                    fulfillmentMessage = $"Your order is ready for pickup. We will send a separate email with pickup instructions. If you have any questions, please reply to this email. {memberEmail}";
                }

                // A. Email to the Customer
                var customerEmailBody = $"<h3>Hello {capitalizedCustomerName},</h3>" +
                                        "<p>Thank you for your order! Your payment has been processed successfully.</p>" +
                                        "<h4>Order Summary:</h4>" +
                                        $"<p><strong>Subtotal:</strong> ${model.Summary.Subtotal:F2}</p>" +
                                        $"<p><strong>Delivery Cost:</strong> ${model.Summary.DeliveryCost:F2}</p>" +
                                        $"<p><strong>Total:</strong> ${model.Summary.Total:F2}</p>" +
                                        "<h4>Items:</h4>" +
                                        "<ul>" +
                                        string.Join("", model.CartItems.Select(item => $"<li>{item.Name} (x{item.Quantity}) - ${item.Price * item.Quantity:F2}</li>")) +
                                        "</ul>" +
                                        $"<p><strong>Notes:</strong> {model.CustomerDetails.Notes}</p>" +
                                        $"<p>{fulfillmentMessage}</p>" +
                                        $"<p>Thank you,<br/>{companyName}</p>";

                var customerEmailSent = await _emailService.SendEmailAsync(
                    model.CustomerDetails.Email,
                    "Your Order Confirmation",
                    customerEmailBody
                );

                // Define a variable for the fulfillment message for the owner
                string fulfillmentTypeMessage;

                if (model.Summary.DeliveryCost > 0)
                {
                    // If there is a delivery cost, it means delivery is required.
                    fulfillmentTypeMessage = "<p><strong>Fulfillment Type:</strong> Delivery</p>";
                }
                else
                {
                    // If the delivery cost is zero, it means the customer will pick up the order.
                    fulfillmentTypeMessage = "<p><strong>Fulfillment Type:</strong> Pickup</p>";
                }

                // B. Email to the Owner
                var ownerEmailBody = $"<h3>New Order Received!</h3>" +
                                     $"<p>A new order has been placed on your website.</p>" +
                                     "<h4>Customer Details:</h4>" +
                                     $"<p><strong>Name:</strong> {capitalizedCustomerName}</p>" +
                                     $"<p><strong>Email:</strong> {model.CustomerDetails.Email}</p>" +
                                     $"<p><strong>Phone:</strong> {model.CustomerDetails.Phone}</p>" +
                                     $"<p><strong>Address:</strong> {model.CustomerDetails.Address}</p>" +
                                     fulfillmentTypeMessage + 
                                     "<h4>Order Details:</h4>" +
                                     $"<p><strong>Subtotal:</strong> ${model.Summary.Subtotal:F2}</p>" +
                                     $"<p><strong>Delivery Cost:</strong> ${model.Summary.DeliveryCost:F2}</p>" +
                                     $"<p><strong>Total:</strong> ${model.Summary.Total:F2}</p>" +
                                     $"<p><strong>Notes:</strong> {model.CustomerDetails.Notes}</p>" +
                                     "<h4>Items:</h4>" +
                                     "<ul>" +
                                     string.Join("", model.CartItems.Select(item => $"<li>{item.Name} (x{item.Quantity}) - ${item.Price * item.Quantity:F2}</li>")) +
                                     "</ul>";

                var ownerEmailSent = await _emailService.SendEmailAsync(
                    memberEmail,
                    $"New Order from {capitalizedCustomerName}",
                    ownerEmailBody
                );

                // It's generally better to proceed with the success response even if the email fails,
                // as the payment has already been processed and the customer shouldn't be penalized.
                // You should log the email failure for investigation.
                if (!customerEmailSent || !ownerEmailSent)
                {
                    _logger.LogWarning("Warning: One or more emails failed to send after a successful payment.");
                }

                return Ok(new { message = "Payment and order processed successfully!", orderId = Guid.NewGuid().ToString(), squarePaymentId = payment.Id });
            }
            catch (ApiException e) // ApiException is correctly referenced from Square.Exceptions
            {
                // Handle Square SDK specific exceptions (e.g., network issues, invalid credentials)
                _logger.LogError($"Square API Exception: {e.Message}");
                foreach (var error in e.Errors)
                {
                    _logger.LogError($"Square Exception Error: Category={error.Category}, Code={error.Code}, Detail={error.Detail}, Field={error.Field}");
                }
                return StatusCode(500, new { message = "Payment processing failed due to an API error. Please try again. (Square SDK Exception)" });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                _logger.LogError($"Unexpected error during payment processing: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during payment processing. Please try again." });
            }
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

        // Helper method for name capitals
        private string MakeNameCapitals(string name)
        {
            var customerName = name;

            // Make sure the culture is appropriate, for example, for English
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            // Capitalize the first letter of each word
            var capitalizedCustomerName = textInfo.ToTitleCase(customerName.ToLower());

            return capitalizedCustomerName;
        }
    }
}