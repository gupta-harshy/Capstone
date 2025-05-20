using System.Collections.Generic;

namespace CartService.Application.DTOs
{
    public class CartDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
    }
}