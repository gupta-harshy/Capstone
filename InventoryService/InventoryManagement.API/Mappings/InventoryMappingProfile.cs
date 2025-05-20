using AutoMapper;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Entities;
using InventoryService.InventoryManagement.API.Models;

namespace InventoryService.InventoryManagement.API.Mappings
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            CreateMap<CreateInventoryDto, InventoryDto>();
            CreateMap<UpdateInventoryDto, InventoryDto>();
            CreateMap<Inventory, InventoryDto>();
        }
    }
}
