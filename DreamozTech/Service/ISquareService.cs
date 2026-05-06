using DreamozTech.Models;

namespace DreamozTech.Service
{
    public interface ISquareService
    {
        Task<List<SquareProduct>> GetAllSquareProductsAsync();
    }
}
