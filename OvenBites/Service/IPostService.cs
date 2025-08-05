using OvenBites.Models;

namespace OvenBites.Service
{
    public interface IPostService
    {
        Task<bool> RegisterMemberMessageAsync(ContactFormModel contactRequest);
        Task<int?> GetMemberIdByEmailAsync(string email);
    }
}
