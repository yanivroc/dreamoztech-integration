using DreamozTech.Api;
using System.Collections.Generic;
using System;

namespace DreamozTech.Models
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
            string googleMapKey,
            Uri memberLogoImagePath,
            Uri memberFaviconPath,
            List<PostDto> productList,
            List<MenuItemRoot>? menuItems,
            WebPageDto? homePage,
            WebPageDto? productsPage,
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
            GoogleMapKey = googleMapKey;
            MemberLogoImagePath = memberLogoImagePath;
            MemberFaviconPath = memberFaviconPath;
            ProductList = productList;
            MenuItems = menuItems;
            HomePage = homePage;
            ProductsPage = productsPage;
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
        public string GoogleMapKey { get; }
        public Uri MemberLogoImagePath { get; }
        public Uri MemberFaviconPath { get; }
        public List<PostDto> ProductList { get; }
        public List<MenuItemRoot>? MenuItems { get; }
        public WebPageDto? HomePage { get; }
        public WebPageDto? ProductsPage { get; }
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