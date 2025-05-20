using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace CartService.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;

        public CartService(ICartRepository repository)
        {
            _repository = repository;
        }

        public async Task<CartDto?> GetCartAsync(string userId)
        {
            var cart = await _repository.GetCartAsync(userId);
            return cart?.ToDto();
        }

        public async Task<CartDto> AddOrUpdateItemAsync(string userId, CartItemDto item)
        {
            var cart = await _repository.GetCartAsync(userId) ?? new Cart(userId);
            cart.AddOrUpdateItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
            await _repository.SaveCartAsync(cart);
            return cart.ToDto();
        }

        public async Task<bool> RemoveItemAsync(string userId, string productId)
        {
            var cart = await _repository.GetCartAsync(userId);
            if (cart == null) return false;
            cart.RemoveItem(productId);
            await _repository.SaveCartAsync(cart);
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            await _repository.DeleteCartAsync(userId);
            return true;
        }

        public async Task<CartDto> IncrementItemQuantityAsync(string userId, string productId)
        {
            var cart = await _repository.GetCartAsync(userId) ?? new Cart(userId);
            cart.IncrementQuantity(productId);
            await _repository.SaveCartAsync(cart);
            return cart.ToDto();
        }

        public async Task<CartDto> DecrementItemQuantityAsync(string userId, string productId)
        {
            var cart = await _repository.GetCartAsync(userId) ?? new Cart(userId);
            cart.DecrementQuantity(productId);
            await _repository.SaveCartAsync(cart);
            return cart.ToDto();
        }
    }
}