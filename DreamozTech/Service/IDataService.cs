using DreamozTech.Api;
using DreamozTech.Models;

namespace DreamozTech.Service
{
    public interface IDataService
    {
        Task<ResponseViewModel> GetProductResponseAsync();
        Task<ResponseViewModel> GetMemberResponseAsync();
        Task<ResponseViewModel> GetPostResponseAsync();
        Task<ResponseViewModel> GetWebResponseAsync();
        Task<List<PostDto>> GetIndividualProductPostsAsync();
        Task<MemberDto> GetMemberDetailsAsync();
        Task<List<MenuItemRoot>> GetMenuItemsAsync();
        Task<WebPageDto> GetWebPageAsync(string pagePath);
        Task<PostDto> GetPostByTitleAsync(string title);
        Task<List<WebPageDto>> GetWebPageListAsync();
    }
}