using OvenBites.Models;

namespace OvenBites.Service
{
    public interface IPostService
    {
        Task<bool> RegisterMemberContactAsync(ContactFormModel contactRequest);
        Task<int?> GetMemberIdByEmailAsync(string email);
    }
}
