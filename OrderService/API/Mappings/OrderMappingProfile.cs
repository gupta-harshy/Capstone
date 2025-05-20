using AutoMapper;
using OrderService.Application.Entity;
using OrderService.Domain.Entities;

namespace OrderService.API.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Order, OrderDto>()
             .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
