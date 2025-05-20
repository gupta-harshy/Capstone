using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckoutOrchestrator.Entities;

namespace CheckoutOrchestrator.Mapping
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap<OrderItemReseultDto, InventoryRequestDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

            CreateMap<InventoryRequestDto, OrderItemReseultDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());
        }


    }
}
