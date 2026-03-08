using DreamozTech.Models;

namespace DreamozTech.Service
{
    public interface IPostService
    {
        Task<bool> RegisterMemberMessageAsync(ContactFormModel contactRequest);
        Task<int?> GetMemberIdByEmailAsync(string email);
    }
}
