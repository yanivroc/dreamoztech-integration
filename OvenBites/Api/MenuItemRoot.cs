using Newtonsoft.Json;

namespace OvenBites.Api
{
    public class MenuItemRoot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("http")]
        public string Http { get; set; }
    }
}
