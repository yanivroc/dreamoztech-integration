using Newtonsoft.Json;
using OvenBites.Api;
using OvenBites.Models;

namespace OvenBites.Service
{
    public class PostService : IPostService
    {
        private readonly ApiService _apiService;
        private readonly ILogger<PostService> _logger;
        private readonly IConfiguration _configuration;
        public PostService(ApiService apiService, ILogger<PostService> logger, IConfiguration configuration)
        {
            _apiService = apiService;
            _logger = logger;
            _configuration = configuration;
        }
        /// <summary>
        /// Registers a new member contact by sending a POST request to the Ops API.
        /// </summary>
        /// <param name="contactRequest">The payload containing member contact details.</param>
        /// <returns>True if the contact was registered successfully, false otherwise.</returns>
        public async Task<bool> RegisterMemberMessageAsync(ContactFormModel contactRequest)
        {
            try
            {
                var token = await _apiService.GetTokenAsync(); // Get the authentication token

                // Define the API endpoint for registering member contacts
                var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
                var endpoint = apiEndpoint + "/Ops/RegisterMemberMessage";

                // Send the POST request using the IApiService
                var response = await _apiService.PostAsync(endpoint, contactRequest, token);

                // Check if the request was successful (status code 2xx)
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Member contact registered successfully for {Email}", contactRequest.Email);
                    return true;
                }
                else
                {
                    // Log detailed error information from the API response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to register member contact for {Email}. Status: {StatusCode}. Response: {ErrorContent}",
                                     contactRequest.Email, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors or issues connecting to the API
                _logger.LogError(ex, "Network error while registering member contact for {Email}: {Message}",
                                 contactRequest.Email, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                _logger.LogError(ex, "An unexpected error occurred while registering member contact for {Email}: {Message}",
                                 contactRequest.Email, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a member's ID by their email address via a POST request to the API.
        /// The API is expected to return a single integer value.
        /// </summary>
        /// <param name="email">The email address of the member.</param>
        /// <returns>The MemberId if found, otherwise null.</returns>
        public async Task<int?> GetMemberIdByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Attempted to get member ID with an empty or null email.");
                return null;
            }

            try
            {
                var token = await _apiService.GetTokenAsync();
                var response = await _apiService.GetMemberIdAsync(token, email);
                if (!string.IsNullOrEmpty(response))
                {
                    var memberId = Convert.ToInt32(response);
                    return memberId;
                }
                else
                {
                    _logger.LogError("Failed to get MemberId for email {Email}. Status: {StatusCode}. Response: {ErrorContent}",
                                     email, response, string.Empty);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while getting member ID for email {Email}: {Message}",
                                 email, ex.Message);
                return null;
            }
            // Catching a JsonSerializationException for robust error handling
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing member ID response for email {Email}. Expected single integer, but received: {Response}",
                                 email, "N/A - See previous log entry for response.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting member ID for email {Email}: {Message}",
                                 email, ex.Message);
                return null;
            }
        }
    }
}

