using CartService.Application.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace CartService.Domain.Entities
{
    public class Cart
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new();

        public Cart(string userId)
        {
            UserId = userId;
        }

        public void AddOrUpdateItem(string productId, string productName, decimal price, int quantity)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity = quantity;
            }
            else
            {
                Items.Add(new CartItem(productId, productName, price, quantity));
            }
        }

        public void RemoveItem(string productId) =>
            Items.RemoveAll(i => i.ProductId == productId);

        public void UpdateQuantity(string productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
                item.Quantity = quantity;
        }

        public void Clear() => Items.Clear();
        public decimal GetTotal() => Items.Sum(i => i.Total);

        public void IncrementQuantity(string productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
                item.Quantity++;
        }

        public void DecrementQuantity(string productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null && item.Quantity > 1)
                item.Quantity--;
        }

        public CartDto ToDto() => new CartDto
        {
            UserId = UserId,
            Items = Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}