using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CartService.Application.Interfaces;
using CartService.Application.DTOs;
using System.Security.Claims;

namespace CartService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in token");

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return Ok(cart);
        }

        [HttpPost("add-or-update-item")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItemDto item)
        {
            var result = await _cartService.AddOrUpdateItemAsync(GetUserId(), item);
            return Ok(result);
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(string productId)
        {
            await _cartService.RemoveItemAsync(GetUserId(), productId);
            return NoContent();
        }

        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return NoContent();
        }

        [HttpPost("increment/{productId}")]
        public async Task<ActionResult<CartDto>> Increment(string productId)
        {
            return Ok(await _cartService.IncrementItemQuantityAsync(GetUserId(), productId));
        }

        [HttpPost("decrement/{productId}")]
        public async Task<ActionResult<CartDto>> Decrement(string productId)
        {
            return Ok(await _cartService.DecrementItemQuantityAsync(GetUserId(), productId));
        }
    }
}