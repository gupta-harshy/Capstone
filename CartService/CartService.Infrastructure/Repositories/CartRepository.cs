
using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CartService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public CartRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            var json = await _cache.GetStringAsync($"cart:{userId}");
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Cart>(json, _options);
        }

        public async Task SaveCartAsync(Cart cart)
        {
            var json = JsonSerializer.Serialize(cart);
            await _cache.SetStringAsync(GetRedisKey(cart.UserId), json);
        }

        public async Task DeleteCartAsync(string userId)
        {
            await _cache.RemoveAsync(GetRedisKey(userId));
        }

        private string GetRedisKey(string userId) => $"cart:{userId}";
    }
}
