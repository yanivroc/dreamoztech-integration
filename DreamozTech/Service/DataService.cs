using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using DreamozTech.Api;
using DreamozTech.Models;

namespace DreamozTech.Service
{
    public class DataService : IDataService
    {
        private readonly ApiService _apiService;

        public DataService(ApiService apiService)
        {
            _apiService = apiService;
        }

        private async Task<string> GetAndCacheApiResponseAsync(Func<string, Task<string>> apiCall)
        {
            string token = await _apiService.GetTokenAsync();
            string responseJson = await apiCall(token);
            return responseJson;
        }

        public async Task<ResponseViewModel> GetProductResponseAsync()
        {
            string productJson = await GetAndCacheApiResponseAsync(async (token) => await _apiService.GetProductsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(productJson);
        }

        public async Task<ResponseViewModel> GetMemberResponseAsync()
        {
            string memberJson = await GetAndCacheApiResponseAsync(async (token) => await _apiService.GetMemberDetailsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(memberJson);
        }

        public async Task<ResponseViewModel> GetPostResponseAsync()
        {
            string postJson = await GetAndCacheApiResponseAsync(async (token) => await _apiService.GetPostDetailsAsync(token));
            return JsonConvert.DeserializeObject<ResponseViewModel>(postJson);
        }

        public async Task<ResponseViewModel> GetWebResponseAsync()
        {
            string webJson = await GetAndCacheApiResponseAsync(async (token) => await _apiService.GetWebDetailsAsync(token));
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