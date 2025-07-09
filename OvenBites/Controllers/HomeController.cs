using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<IActionResult> Index(string pageName)
        {
            const string cacheKeyProduct = "Product";
            const string cacheKeyMember = "Member";
            const string cacheKeyPost = "Post";
            const string cacheKeyWeb = "Web";

            if (string.IsNullOrEmpty(pageName))
            {
                pageName = "home";
            }

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

            var domainPath = "https://dreamoztech.com/";
            var productJson = ViewData["Product"] as string;
            var memberJson = ViewData["Member"] as string;
            var postJson = ViewData["Post"] as string;
            var webJson = ViewData["Web"] as string;
            //member
            var rootObject = JsonConvert.DeserializeObject<ResponseViewModel>(memberJson);
            var memberDetailObject = rootObject?.Member;
            var memberFullName = memberDetailObject.MemberFullName.Replace(" ", "");
            var memberLogoImagePath = new Uri(domainPath + memberDetailObject.ProfilePicture);
            //product
            var productDetailObject = JsonConvert.DeserializeObject<ResponseViewModel>(productJson);
            var productList = productDetailObject.Products.Posts.ToList();
            var currentPost = productList.FirstOrDefault(x=>x.BizDisplayTitle.ToLower() == pageName.ToLower());
            //web
            var webDetailObject = JsonConvert.DeserializeObject<ResponseViewModel>(webJson);
            var web = webDetailObject.Webs.FirstOrDefault();
            List<MenuItemRoot> menuItems = JsonConvert.DeserializeObject<List<MenuItemRoot>>(web.MenuItems);
            var homePage = web.WebPages.FirstOrDefault(x => x.PagePath == "Home");
            var cookiesPage = web.WebPages.FirstOrDefault(x => x.PagePath == "Cookies");
            var aboutPage = web.WebPages.FirstOrDefault(x => x.PagePath == "About");
            var contactPage = web.WebPages.FirstOrDefault(x => x.PagePath == "Contact");
            var termsPage = web.WebPages.FirstOrDefault(x => x.PagePath == "Terms");
            var currentPage = web.WebPages.FirstOrDefault(x => x.PagePath.ToLower() == pageName.ToLower()); 
            //post
            var postDetailObject = JsonConvert.DeserializeObject<ResponseViewModel>(postJson);
            var sliderPost = postDetailObject.Posts.FirstOrDefault(x => x.BizDisplayTitle == "Slider-Ovenbites");
            var popupPost = postDetailObject.Posts.FirstOrDefault(x => x.BizDisplayTitle == "Flavor-of-the-Month");
            var popupPostImage = popupPost.Pics.FirstOrDefault();
            var popupPostImagePath = new Uri(popupPostImage.PicPath);
            List<PicDto> sliderPics = sliderPost.Pics.ToList();

            DisplayViewModel vm = new DisplayViewModel(memberDetailObject, memberFullName, memberLogoImagePath, productList, menuItems, homePage, cookiesPage, aboutPage, contactPage, termsPage, sliderPost, popupPost, popupPostImagePath, sliderPics, currentPage, currentPost);  
            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
