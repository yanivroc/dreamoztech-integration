using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OvenBites.Api;
using OvenBites.Models;
using OvenBites.Services;
using System.Diagnostics;

namespace OvenBites.Controllers
{
    public class CartController : Controller
    {
        private readonly IDataService _dataService;
        public CartController(IDataService dataService)
        {
            _dataService = dataService;
        }
        public async Task<IActionResult> Index(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                pageName = "home";
            }

            var memberDetailObject = await _dataService.GetMemberDetailsAsync();
            // Updated call and type to List<PostDto>
            var productPostsList = await _dataService.GetIndividualProductPostsAsync();
            var web = (await _dataService.GetWebResponseAsync())?.Webs?.FirstOrDefault();
            var sliderPost = await _dataService.GetPostByTitleAsync("Slider-Ovenbites");
            var popupPost = await _dataService.GetPostByTitleAsync("Flavor-of-the-Month");

            var domainPath = "https://dreamoztech.com/";
            var memberFullName = memberDetailObject?.MemberFullName?.Replace(" ", "") ?? string.Empty;
            var memberLogoImagePath = memberDetailObject != null ? new Uri(domainPath + memberDetailObject.ProfilePicture) : null;

            var currentPage = await _dataService.GetWebPageAsync(pageName);
            var homePage = await _dataService.GetWebPageAsync("Home");
            var cookiesPage = await _dataService.GetWebPageAsync("Cookies");
            var aboutPage = await _dataService.GetWebPageAsync("About");
            var contactPage = await _dataService.GetWebPageAsync("Contact");
            var termsPage = await _dataService.GetWebPageAsync("Terms");

            // Now currentPost should be found from productPostsList (which is List<PostDto>)
            var currentPost = productPostsList?.FirstOrDefault(x => x.BizDisplayTitle.ToLower() == pageName.ToLower());

            var popupPostImage = popupPost?.Pics?.FirstOrDefault();
            var popupPostImagePath = popupPostImage != null ? new Uri(popupPostImage.PicPath) : null;

            List<PicDto> sliderPics = sliderPost?.Pics?.ToList() ?? new List<PicDto>();
            List<MenuItemRoot> menuItems = web != null && !string.IsNullOrEmpty(web.MenuItems)
                                            ? JsonConvert.DeserializeObject<List<MenuItemRoot>>(web.MenuItems)
                                            : new List<MenuItemRoot>();

            DisplayViewModel vm = new DisplayViewModel(
                memberDetailObject,
                memberFullName,
                memberLogoImagePath,
                productPostsList,
                menuItems,
                homePage,
                cookiesPage,
                aboutPage,
                contactPage,
                termsPage,
                sliderPost,
                popupPost,
                popupPostImagePath,
                sliderPics,
                currentPage,
                currentPost
            );

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
