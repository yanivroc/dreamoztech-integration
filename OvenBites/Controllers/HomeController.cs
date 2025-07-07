using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OvenBites.Api;
using OvenBites.Models;

namespace OvenBites.Controllers
{
    public class HomeController : Controller
    {
        private readonly Api.ApiService _apiService;
        private readonly IMemoryCache _memoryCache;

        public HomeController(Api.ApiService apiService, IMemoryCache memoryCache)
        {
            _apiService = apiService;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index()
        {
            const string cacheKeyProduct = "Product";
            const string cacheKeyMember = "Member";
            const string cacheKeyPost = "Post";
            const string cacheKeyWeb = "Web";

            string token = await _apiService.GetTokenAsync();

            // Try to get Product from the cache
            if (!_memoryCache.TryGetValue(cacheKeyProduct, out var productResponse))
            {
                productResponse = await _apiService.GetProductsAsync(token);
                _memoryCache.Set(cacheKeyProduct, productResponse, TimeSpan.FromHours(24));
            }
            ViewData["Product"] = productResponse;

            // Try to get Member from the cache
            if (!_memoryCache.TryGetValue(cacheKeyMember, out var memberResponse))
            {
                memberResponse = await _apiService.GetMemberDetailsAsync(token);
                _memoryCache.Set(cacheKeyMember, memberResponse, TimeSpan.FromHours(24));
            }
            ViewData["Member"] = memberResponse;

            // Try to get Post from the cache
            if (!_memoryCache.TryGetValue(cacheKeyPost, out var postResponse))
            {
                postResponse = await _apiService.GetPostDetailsAsync(token);
                _memoryCache.Set(cacheKeyPost, postResponse, TimeSpan.FromHours(24));
            }
            ViewData["Post"] = postResponse;

            // Try to get Web from the cache
            if (!_memoryCache.TryGetValue(cacheKeyWeb, out var webResponse))
            {
                webResponse = await _apiService.GetWebDetailsAsync(token);
                _memoryCache.Set(cacheKeyWeb, webResponse, TimeSpan.FromHours(24));
            }
            ViewData["Web"] = webResponse;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
