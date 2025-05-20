using CartService.Application.DTOs;
using System.Threading.Tasks;

namespace CartService.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto?> GetCartAsync(string userId);
        Task<CartDto> AddOrUpdateItemAsync(string userId, CartItemDto item);
        Task<bool> RemoveItemAsync(string userId, string productId);
        Task<bool> ClearCartAsync(string userId);
        Task<CartDto> IncrementItemQuantityAsync(string userId, string productId);
        Task<CartDto> DecrementItemQuantityAsync(string userId, string productId);
    }
}