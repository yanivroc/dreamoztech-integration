using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DreamozTech.Api;
using DreamozTech.Models;
using DreamozTech.Service;
using System.Diagnostics;

namespace DreamozTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataService _dataService;
        private readonly string _recaptchaSiteKey;
        private readonly string _recaptchaSecretKey;
        private readonly string _applicationId;
        private readonly string _locationId;
        private readonly IConfiguration _configuration;
        public HomeController(IDataService dataService, IConfiguration configuration)
        {
            _dataService = dataService;
            _configuration = configuration;
            _recaptchaSiteKey = _configuration["GoogleReCaptcha:SiteKey"];
            _recaptchaSecretKey = _configuration["GoogleReCaptcha:SecretKey"];
            _applicationId = _configuration["Square:ApplicationId"];
            _locationId = _configuration["Square:LocationId"];
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
            productPostsList = productPostsList.Where(x => x.BizEnable == true).OrderBy(x => x.BizName).ToList(); // Filter enabled products
            var web = (await _dataService.GetWebResponseAsync())?.Webs?.FirstOrDefault();
            var sliderPost = await _dataService.GetPostByTitleAsync("Slider-Dreamoz-Tech");
            var popupPost = await _dataService.GetPostByTitleAsync("Product-Of-The-Month");

            var domainPath = _configuration["General:DomainPath"];
            var googleMapKey = _configuration["GoogleMap:MapKey"];
            var memberFullName = memberDetailObject?.MemberFullName?.Replace(" ", "") ?? string.Empty;
            var memberLogoImagePath = memberDetailObject != null ? new Uri(domainPath + memberDetailObject.ProfilePicture) : null;
            var memberFaviconPath = web != null ? new Uri(domainPath + web.LogoFavicon) : null;

            var currentPage = await _dataService.GetWebPageAsync(pageName);
            var homePage = await _dataService.GetWebPageAsync("Home");
            var cookiesPage = await _dataService.GetWebPageAsync("Products");
            var aboutPage = await _dataService.GetWebPageAsync("About");
            var contactPage = await _dataService.GetWebPageAsync("Contact");
            var termsPage = await _dataService.GetWebPageAsync("Terms");

            // Now currentPost should be found from productPostsList (which is List<PostDto>)
            var currentPost = productPostsList?.FirstOrDefault(x => x.BizDisplayTitle.ToLower() == pageName.ToLower());

            if (currentPage == null && currentPost == null)
            {
                return RedirectToAction("Index", new { pageName = "home" });
            }

            if (currentPage == null)
            {
                currentPage = new WebPageDto();
                currentPage.PageTitle = currentPost.BizName;
                currentPage.SeoKeywords = currentPost.MetaKey;
                currentPage.SeoDescription = currentPost.MetaDesc;
            }

            var popupPostImage = popupPost?.Pics?.FirstOrDefault();
            var popupPostImagePath = popupPostImage != null ? new Uri(popupPostImage.PicPath) : null;

            List<PicDto> sliderPics = sliderPost?.Pics?.ToList() ?? new List<PicDto>();
            List<MenuItemRoot> menuItems = web != null && !string.IsNullOrEmpty(web.MenuItems)
                                            ? JsonConvert.DeserializeObject<List<MenuItemRoot>>(web.MenuItems)
                                            : new List<MenuItemRoot>();

            DisplayViewModel vm = new DisplayViewModel(
                memberDetailObject,
                memberFullName,
                _recaptchaSiteKey,
                _recaptchaSecretKey,
                _applicationId,
                _locationId,
                googleMapKey,
                memberLogoImagePath,
                memberFaviconPath,
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