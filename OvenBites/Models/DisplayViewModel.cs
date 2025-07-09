using OvenBites.Api;

namespace OvenBites.Models
{
    public class DisplayViewModel
    {
        public DisplayViewModel(MemberDto memberDetailObject, string memberFullName, Uri memberLogoImagePath, List<PostDto> productList, List<MenuItemRoot>? menuItems, WebPageDto? homePage, WebPageDto? cookiesPage, WebPageDto? aboutPage, WebPageDto? contactPage, WebPageDto? termsPage, PostDto sliderPost, PostDto popupPost, Uri popupPostImagePath, List<PicDto> sliderPics, WebPageDto? currentPage)
        {
            MemberDetailObject = memberDetailObject;
            MemberFullName = memberFullName;
            MemberLogoImagePath = memberLogoImagePath;
            ProductList = productList;
            MenuItems = menuItems;
            HomePage = homePage;
            CookiesPage = cookiesPage;
            AboutPage = aboutPage;
            ContactPage = contactPage;
            TermsPage = termsPage;
            SliderPost = sliderPost;
            PopupPost = popupPost;
            PopupPostImagePath = popupPostImagePath;
            SliderPics = sliderPics;
            CurrentPage = currentPage;
        }

        public string MemberFullName { get; }
        public Uri MemberLogoImagePath { get; }
        public List<PostDto> ProductList { get; }
        public List<MenuItemRoot>? MenuItems { get; }
        public WebPageDto? HomePage { get; }
        public WebPageDto? CookiesPage { get; }
        public WebPageDto? AboutPage { get; }
        public WebPageDto? ContactPage { get; }
        public WebPageDto? TermsPage { get; }
        public PostDto SliderPost { get; }
        public Uri PopupPostImagePath { get; }
        public List<PicDto> SliderPics { get; }
        public WebPageDto? CurrentPage { get; private set; }
        public MemberDto MemberDetailObject { get; }
        public PostDto PopupPost { get; }
    }
}
