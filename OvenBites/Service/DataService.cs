using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OvenBites.Api;
using OvenBites.Models;

namespace OvenBites.Service
{
    public class DataService : IDataService
    {
        private readonly ApiService _apiService;
        private readonly IMemoryCache _memoryCache;
        private const string CacheKeyPrefix = "OvenBites_";

        public DataService(ApiService apiService, IMemoryCache memoryCache)
        {
            _apiService = apiService;
            _memoryCache = memoryCache;
        }

        private async Task<string> GetAndCacheApiResponseAsync(string cacheKey, Func<string, Task<string>> apiCall)
        {
            //if (!_memoryCache.TryGetValue(cacheKey, out string responseJson))
            //{
            //    string token = await _apiService.GetTokenAsync();
            //    responseJson = await apiCall(token);
            //    _memoryCache.Set(cacheKey, responseJson, TimeSpan.FromHours(24));
            //}
            string token = await _apiService.GetTokenAsync();
            string responseJson = await apiCall(token);
            return responseJson;
        }

        public async Task<ResponseViewModel> GetProductResponseAsync()
        {
            string productJson = await GetAndCacheApiResponseAsync(CacheKeyPrefix + "Product",
                                                                   async (token) => await _apiService.GetProductsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(productJson);
        }

        public async Task<ResponseViewModel> GetMemberResponseAsync()
        {
            string memberJson = await GetAndCacheApiResponseAsync(CacheKeyPrefix + "Member",
                                                                  async (token) => await _apiService.GetMemberDetailsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(memberJson);
        }

        public async Task<ResponseViewModel> GetPostResponseAsync()
        {
            string postJson = await GetAndCacheApiResponseAsync(CacheKeyPrefix + "Post",
                                                                async (token) => await _apiService.GetPostDetailsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(postJson);
        }

        public async Task<ResponseViewModel> GetWebResponseAsync()
        {
            string webJson = await GetAndCacheApiResponseAsync(CacheKeyPrefix + "Web",
                                                               async (token) => await _apiService.GetWebDetailsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(webJson);
        }

        // Renamed and changed return type to List<PostDto>
        public async Task<List<PostDto>> GetIndividualProductPostsAsync()
        {
            var response = await GetProductResponseAsync();
            // Directly return the Posts list from the ProductDto within the response
            return response?.Products?.Posts?.ToList() ?? new List<PostDto>();
        }

        public async Task<MemberDto> GetMemberDetailsAsync()
        {
            var response = await GetMemberResponseAsync();
            return response?.Member;
        }

        public async Task<List<MenuItemRoot>> GetMenuItemsAsync()
        {
            var response = await GetWebResponseAsync();
            return response?.Webs?.FirstOrDefault() != null
                   ? JsonConvert.DeserializeObject<List<MenuItemRoot>>(response.Webs.FirstOrDefault().MenuItems)
                   : new List<MenuItemRoot>();
        }

        public async Task<WebPageDto> GetWebPageAsync(string pagePath)
        {
            var response = await GetWebResponseAsync();
            return response?.Webs?.FirstOrDefault()?.WebPages?.FirstOrDefault(x => x.PagePath.ToLower() == pagePath.ToLower());
        }

        public async Task<PostDto> GetPostByTitleAsync(string title)
        {
            var response = await GetPostResponseAsync();
            return response?.Posts?.FirstOrDefault(x => x.BizDisplayTitle == title);
        }

        public async Task<List<WebPageDto>> GetWebPageListAsync()
        {
            var response = await GetWebResponseAsync();
            return response?.Webs?.FirstOrDefault()?.WebPages.ToList();
        }
    }
}