using Microsoft.AspNetCore.Mvc;
using DreamozTech.Models;
using DreamozTech.Service;
using Square;
using Square.Exceptions;
using Square.Models;
using System.Globalization;
using System.Text.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace DreamozTech.Controllers
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
                _logger?.LogWarning($"Warning: Square:Environment config value '{environmentString}' is invalid. Defaulting to Sandbox.");
            }

            // SquareClient instantiation using Builder pattern for v22.0.0
            _squareClient = new SquareClient.Builder()
                .Environment(squareEnvironment) // Square.Environment enum should be recognized now
                .AccessToken(accessToken)
                .UserAgentDetail("DreamozTech_App_Csharp_Payment") // Custom user agent for your app
                .Build();

            _locationId = _configuration["Square:LocationId"];
            if (string.IsNullOrEmpty(_locationId))
            {
                throw new ArgumentNullException("Square:LocationId is not configured in appsettings.json");
            }

            _emailService = emailService;
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

            // Match client-side logic in site.js:
            // - If client indicated delivery (model.Summary.DeliveryCost > 0), delivery cost is:
            //     subtotal < 75 => 10.00
            //     subtotal >= 75 => 0.00 (free)
            // - Otherwise delivery cost is 0
            decimal calculatedDeliveryCost = 0m;
            if (model.Summary.DeliveryCost > 0)
            {
                calculatedDeliveryCost = calculatedSubtotal < 75m ? 10.00m : 0.00m;
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
                // PaymentsApi is correctly accessed via _square_client.
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
                _logger.LogInformation("Square Payment Successful. Payment ID: {PaymentId}, Status: {Status}", payment?.Id, payment?.Status);

                var memberDetailObject = await _dataService.GetMemberDetailsAsync();
                var companyName = memberDetailObject.MemberFullName;
                var memberEmail = memberDetailObject.MemberEmail;
                // var memberABN = memberDetailObject.MemberABN;

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

                // generate invoice and add as attachment to email
                var orderId = Guid.NewGuid().ToString();
                byte[] invoiceBytes;
                try
                {
                    invoiceBytes = GenerateInvoicePdf(model, orderId, companyName);
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Failed to generate invoice PDF for order {OrderId}", orderId);
                    invoiceBytes = Array.Empty<byte>();
                }

                // A. Email to the Customer (attach invoice if generated)
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

                bool customerEmailSent;
                if (invoiceBytes != null && invoiceBytes.Length > 0)
                {
                    customerEmailSent = await _emailService.SendInvoiceEmailAsync(
                        model.CustomerDetails.Email,
                        "Your Order Confirmation and Invoice",
                        customerEmailBody,
                        invoiceBytes,
                        $"invoice-{orderId}.pdf"
                    );
                }
                else
                {
                    customerEmailSent = await _emailService.SendEmailAsync(
                        model.CustomerDetails.Email,
                        "Your Order Confirmation",
                        customerEmailBody
                    );
                }

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

                // B. Email to the Owner (attach same invoice)
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

                bool ownerEmailSent;
                if (invoiceBytes != null && invoiceBytes.Length > 0)
                {
                    ownerEmailSent = await _emailService.SendInvoiceEmailAsync(
                        memberEmail,
                        $"New Order from {capitalizedCustomerName}",
                        ownerEmailBody,
                        invoiceBytes,
                        $"invoice-{orderId}.pdf"
                    );
                }
                else
                {
                    ownerEmailSent = await _emailService.SendEmailAsync(
                        memberEmail,
                        $"New Order from {capitalizedCustomerName}",
                        ownerEmailBody
                    );
                }

                if (!customerEmailSent || !ownerEmailSent)
                {
                    _logger.LogWarning("Warning: One or more emails (with invoice) failed to send after a successful payment for OrderId {OrderId}.", orderId);
                }

                return Ok(new { message = "Payment and order processed successfully!", orderId = orderId, squarePaymentId = payment.Id });
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

        // Generate a simple PDF invoice using PdfSharpCore
        private byte[] GenerateInvoicePdf(PaymentRequestModel model, string orderId, string companyName)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontHeader = new XFont("Arial", 11, XFontStyle.Bold);
            var font = new XFont("Arial", 10, XFontStyle.Regular);

            double marginLeft = 40;
            double marginRight = 40;
            double usableWidth = page.Width.Point - marginLeft - marginRight;
            double y = 40;

            // Title / header
            gfx.DrawString(companyName, fontTitle, XBrushes.Black, new XRect(marginLeft, y, usableWidth, 30), XStringFormats.TopLeft);
            y += 30;
            gfx.DrawString($"Invoice: {orderId}", font, XBrushes.Black, new XRect(marginLeft, y, usableWidth, 20), XStringFormats.TopLeft);
            y += 18;
            gfx.DrawString($"Date: {DateTime.UtcNow:yyyy-MM-dd}", font, XBrushes.Black, new XRect(marginLeft, y, usableWidth, 20), XStringFormats.TopLeft);
            y += 18;
            gfx.DrawString($"Customer: {MakeNameCapitals(model.CustomerDetails.Name)}", font, XBrushes.Black, new XRect(marginLeft, y, usableWidth, 20), XStringFormats.TopLeft);
            y += 16;
            gfx.DrawString($"Email: {model.CustomerDetails.Email}", font, XBrushes.Black, new XRect(marginLeft, y, usableWidth, 20), XStringFormats.TopLeft);
            y += 24;

            // Table layout: 4 columns (Name, Qty, Price, Total)
            double colNameWidth = usableWidth * 0.55;   // name gets most space
            double colQtyWidth = usableWidth * 0.10;
            double colPriceWidth = usableWidth * 0.175;
            double colTotalWidth = usableWidth * 0.175;

            // Spacing settings: ensure at least 3 lines per row and some gap between rows
            double lineHeight = font.Size + 6;             // line height for wrapped lines
            double minRowHeight = lineHeight * 3;          // at least 3 lines tall
            double rowGap = lineHeight;                    // blank space between rows

            double rowHeaderHeight = lineHeight;
            // Draw header
            gfx.DrawRectangle(XPens.Black, marginLeft, y, usableWidth, rowHeaderHeight);
            gfx.DrawString("Name", fontHeader, XBrushes.Black, new XRect(marginLeft + 4, y + 3, colNameWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString("Qty", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + 4, y + 3, colQtyWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString("Price", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + 4, y + 3, colPriceWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString("Total", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + colPriceWidth + 4, y + 3, colTotalWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
            y += rowHeaderHeight + rowGap;

            // Helper to wrap text into lines fitting a width
            List<string> WrapText(string text, XFont f, double maxWidth)
            {
                var words = text.Split(' ');
                var lines = new List<string>();
                var current = "";
                foreach (var w in words)
                {
                    var test = string.IsNullOrEmpty(current) ? w : current + " " + w;
                    var size = gfx.MeasureString(test, f);
                    if (size.Width <= maxWidth)
                    {
                        current = test;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            lines.Add(current);
                        }
                        // If single word too long, break by characters
                        var singleSize = gfx.MeasureString(w, f);
                        if (singleSize.Width > maxWidth)
                        {
                            var chunk = "";
                            foreach (var ch in w)
                            {
                                var t = chunk + ch;
                                if (gfx.MeasureString(t, f).Width <= maxWidth)
                                {
                                    chunk = t;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(chunk)) lines.Add(chunk);
                                    chunk = ch.ToString();
                                }
                            }
                            if (!string.IsNullOrEmpty(chunk)) current = chunk;
                            else current = "";
                        }
                        else
                        {
                            current = w;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(current)) lines.Add(current);
                if (lines.Count == 0) lines.Add("");
                return lines;
            }

            // Print each item as a row; wrap name into multiple lines
            foreach (var item in model.CartItems)
            {
                var itemTotal = item.Price * item.Quantity;
                var nameLines = WrapText(item.Name, font, colNameWidth - 8);

                // Calculate required height for this row based on wrapped lines and minimum
                double neededHeight = Math.Max(minRowHeight, nameLines.Count * lineHeight);

                // Add page if overflow
                if (y + neededHeight + 140 > page.Height) // leave space for totals
                {
                    var newPage = doc.AddPage();
                    page = newPage;
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;

                    // redraw header on new page
                    gfx.DrawRectangle(XPens.Black, marginLeft, y, usableWidth, rowHeaderHeight);
                    gfx.DrawString("Name", fontHeader, XBrushes.Black, new XRect(marginLeft + 4, y + 3, colNameWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
                    gfx.DrawString("Qty", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + 4, y + 3, colQtyWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
                    gfx.DrawString("Price", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + 4, y + 3, colPriceWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
                    gfx.DrawString("Total", fontHeader, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + colPriceWidth + 4, y + 3, colTotalWidth - 8, rowHeaderHeight), XStringFormats.TopLeft);
                    y += rowHeaderHeight + rowGap;
                }

                // Draw row background/border
                gfx.DrawRectangle(XPens.Black, marginLeft, y, usableWidth, neededHeight);

                // Draw name lines with increased spacing
                double textY = y + 6;
                foreach (var line in nameLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(marginLeft + 6, textY, colNameWidth - 12, lineHeight), XStringFormats.TopLeft);
                    textY += lineHeight;
                }

                // Qty, Price, Total columns
                gfx.DrawString(item.Quantity.ToString(), font, XBrushes.Black, new XRect(marginLeft + colNameWidth + 6, y + 6, colQtyWidth - 8, neededHeight), XStringFormats.TopLeft);
                gfx.DrawString($"${item.Price:F2}", font, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + 6, y + 6, colPriceWidth - 8, neededHeight), XStringFormats.TopLeft);
                gfx.DrawString($"${itemTotal:F2}", font, XBrushes.Black, new XRect(marginLeft + colNameWidth + colQtyWidth + colPriceWidth + 6, y + 6, colTotalWidth - 8, neededHeight), XStringFormats.TopLeft);

                // Advance Y including an extra gap between rows
                y += neededHeight + rowGap;
            }

            // Bottom totals area
            y += 12;
            double totalsX = marginLeft + colNameWidth + colQtyWidth; // align totals to price/total columns

            // Subtotal row
            gfx.DrawString($"Subtotal:", fontHeader, XBrushes.Black, new XRect(totalsX, y, colPriceWidth, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString($"${model.Summary.Subtotal:F2}", fontHeader, XBrushes.Black, new XRect(totalsX + colPriceWidth, y, colTotalWidth, rowHeaderHeight), XStringFormats.TopLeft);
            y += rowHeaderHeight + 4;

            // Delivery row
            gfx.DrawString($"Delivery:", fontHeader, XBrushes.Black, new XRect(totalsX, y, colPriceWidth, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString($"${model.Summary.DeliveryCost:F2}", fontHeader, XBrushes.Black, new XRect(totalsX + colPriceWidth, y, colTotalWidth, rowHeaderHeight), XStringFormats.TopLeft);
            y += rowHeaderHeight + 4;

            // Total row (bold)
            var fontTotal = new XFont("Arial", 12, XFontStyle.Bold);
            gfx.DrawString($"Total:", fontTotal, XBrushes.Black, new XRect(totalsX, y, colPriceWidth, rowHeaderHeight), XStringFormats.TopLeft);
            gfx.DrawString($"${model.Summary.Total:F2}", fontTotal, XBrushes.Black, new XRect(totalsX + colPriceWidth, y, colTotalWidth, rowHeaderHeight), XStringFormats.TopLeft);

            using var ms = new MemoryStream();
            doc.Save(ms);
            gfx.Dispose();
            return ms.ToArray();
        }
    }
}