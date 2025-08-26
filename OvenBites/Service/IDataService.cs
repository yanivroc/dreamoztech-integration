using System.Collections.Generic;
using System.Threading.Tasks;
using OvenBites.Api;
using OvenBites.Models;

namespace OvenBites.Service
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