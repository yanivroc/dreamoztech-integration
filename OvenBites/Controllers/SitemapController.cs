using Microsoft.AspNetCore.Mvc;
using OvenBites.Service;
using SimpleMvcSitemap;

namespace OvenBites.Controllers
{
    public class SitemapController : Controller
    {
        private readonly IDataService _dataService;

        public SitemapController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [Route("sitemap.xml")]
        public async Task<IActionResult> Index()
        {
            var nodes = new List<SitemapNode>();

            // Get base URL
            var memberDetailObject = await _dataService.GetMemberDetailsAsync();
            var websiteName = memberDetailObject.Website;
            nodes.Add(new SitemapNode(websiteName)); // Add the base website URL

            // Add product pages
            var productPostsList = await _dataService.GetIndividualProductPostsAsync();
            foreach (var page in productPostsList)
            {
                // Correctly construct the URL, ensuring a single slash.
                nodes.Add(new SitemapNode($"{websiteName}/{page.BizDisplayTitle}"));
            }

            // Add web pages
            var webPageList = await _dataService.GetWebPageListAsync();
            foreach (var page in webPageList)
            {
                // Correctly construct the URL.
                nodes.Add(new SitemapNode($"{websiteName}/{page.PagePath}"));
            }

            return new SimpleMvcSitemap.SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }
    }
}