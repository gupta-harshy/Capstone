using OrderService.Domain.Enum;

namespace OrderService.Application.Entity
{
    public class OrderDto
    {
        public List<OrderItemDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
