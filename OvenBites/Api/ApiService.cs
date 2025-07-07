using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace OvenBites.Api
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
    }
}
