using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace DreamozTech.Api
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync(string apiKey = null, string apiSecret = null)
        {
            var client = _httpClientFactory.CreateClient();
            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];

            apiKey ??= _configuration["ApiSettings:ApiKey"];
            apiSecret ??= _configuration["ApiSettings:ApiSecret"];

            var requestBody = new
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret
            };

            var response = await client.PostAsJsonAsync(apiEndpoint + "/Client/Token", requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            JsonNode jsonNode = JsonNode.Parse(responseContent);
            string token = jsonNode["token"]?.ToString();

            Console.WriteLine($"Token: {token}");
            return token;
        }

        public async Task<string> GetProductsAsync(string token, int item = 200)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
            var getLink = apiEndpoint + "/Member/Products?item=" + item;
            var response = await client.GetAsync(getLink);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetMemberDetailsAsync(string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
            var getLink = apiEndpoint + "/Member/Get";
            var response = await client.GetAsync(getLink);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPostDetailsAsync(string token, int item = 50)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
            var getLink = apiEndpoint + "/Member/Posts?item=" + item;
            var response = await client.GetAsync(getLink);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetWebDetailsAsync(string token, int item = 50)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
            var getLink = apiEndpoint + "/Member/Webs?item=" + item;
            var response = await client.GetAsync(getLink);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetMemberIdAsync(string token, string email)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiEndpoint = _configuration["ApiSettings:ApiEndpoint"];
            var postLink = apiEndpoint + "/Member/Id";

            // The API expects the raw email string in the request body.
            var httpContent = new StringContent($"\"{email}\"", Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(postLink, httpContent);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a POST request to a specified endpoint with a JSON payload and a bearer token.
        /// </summary>
        /// <typeparam name="T">The type of the payload object to be serialized to JSON.</typeparam>
        /// <param name="endpoint">The full URL of the API endpoint.</param>
        /// <param name="payload">The object to be serialized into the request body.</param>
        /// <param name="token">The authentication token to be included in the request header.</param>
        /// <returns>The HttpResponseMessage from the API if the request is successful.</returns>
        /// <exception cref="HttpRequestException">Thrown if the response status code is not a success code (2xx).</exception>
        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T payload, string token)
        {
            // Create a new HttpClient instance from the factory
            var client = _httpClientFactory.CreateClient();

            // Set the Authorization header with the bearer token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Serialize the payload object to a JSON string
            var jsonContent = JsonConvert.SerializeObject(payload);

            // Create the HttpContent object with the JSON string and set the Content-Type header
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request
                var response = await client.PostAsync(endpoint, httpContent);

                // This method throws an exception if the HTTP response status code is not in the 2xx range
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                throw; // Re-throw the exception to be handled by the calling code
            }
            catch (Exception ex)
            {
                throw; // Re-throw the exception
            }
        }
    }
}
