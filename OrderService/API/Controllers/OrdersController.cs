using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Entity;
using OrderService.Application.Services;
using OrderService.Domain.Enum;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrdersController(IOrderService orderService, IMapper mapper)
    {
        _orderService = orderService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var id = await _orderService.PlaceOrderAsync(HttpContext, ct);
        return Accepted(new { orderId = id });
    }

    /// <summary>
    /// GET api/orders/{orderId}/status
    /// </summary>
    
    [HttpGet("{orderId:guid}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(Guid orderId, CancellationToken ct)
    {
        var status = await _orderService.GetOrderStatusAsync(orderId, ct);
        return Ok(new { orderId, status = status.ToString() });
    }

    /// <summary>
    /// GET api/orders/{orderId}
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct)
    {
        var order = await _orderService.GetOrderAsync(orderId, ct);
        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(orderDto);
    }

    /// <summary>
    /// PUT api/orders/{orderId}/status
    /// Body: { "status": "PaymentProcessed" }
    /// </summary>
    //[Authorize]
    [HttpPut("{orderId:guid}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusDto dto,
        CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            return BadRequest($"Invalid status '{dto.Status}'");

        await _orderService.UpdateOrderStatusAsync(orderId, newStatus, ct);
        return NoContent();
    }
}
