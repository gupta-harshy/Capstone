using CartService.Domain.Entities;
using System.Threading.Tasks;

namespace CartService.Domain.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(string userId);
        Task SaveCartAsync(Cart cart);
        Task DeleteCartAsync(string userId);
    }
}