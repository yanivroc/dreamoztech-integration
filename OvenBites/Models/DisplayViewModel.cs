using OvenBites.Api;
using System.Collections.Generic;
using System;

namespace OvenBites.Models
{
    public class DisplayViewModel
    {
        public DisplayViewModel(
            MemberDto memberDetailObject,
            string memberFullName,
            string siteKey,
            string siteSecret,
            string applicationId,
            string locationId,
            Uri memberLogoImagePath,
            List<PostDto> productList,
            List<MenuItemRoot>? menuItems,
            WebPageDto? homePage,
            WebPageDto? cookiesPage,
            WebPageDto? aboutPage,
            WebPageDto? contactPage,
            WebPageDto? termsPage,
            PostDto sliderPost,
            PostDto popupPost,
            Uri popupPostImagePath,
            List<PicDto> sliderPics,
            WebPageDto? currentPage,
            PostDto? currentPost = null)
        {
            MemberDetailObject = memberDetailObject;
            MemberFullName = memberFullName;
            SiteKey = siteKey;
            SiteSecret = siteSecret;
            ApplicationId = applicationId;
            LocationId = locationId;
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
            CurrentPost = currentPost; 
        }

        public string MemberFullName { get; }
        public string SiteKey { get; }
        public string SiteSecret { get; }
        public string ApplicationId { get; }
        public string LocationId { get; }
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
        public PostDto? CurrentPost { get; } 
    }
}