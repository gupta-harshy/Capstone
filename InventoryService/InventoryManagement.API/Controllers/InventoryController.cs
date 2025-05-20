using AutoMapper;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryService.InventoryManagement.API.Models;
using InventoryService.InventoryManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;

        public InventoryController(IInventoryService inventoryService, IMapper mapper)
        {
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _inventoryService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var inventory = await _inventoryService.GetByIdAsync(id);
            return inventory == null ? NotFound() : Ok(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInventoryDto createInventoryDto)
        {
            var dto = _mapper.Map<InventoryDto>(createInventoryDto);
            var id = await _inventoryService.CreateAsync(dto);
            dto.Id = id;
            return CreatedAtAction(nameof(Create), new { id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateInventoryDto updateInventoryDto)
        {
            var dto = _mapper.Map<InventoryDto>(updateInventoryDto);
            await _inventoryService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _inventoryService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// POST api/inventory/reserve
        /// </summary>
        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve([FromBody] List<InventoryRequestDto> items, CancellationToken ct)
        {
            var result = await _inventoryService.ReserveAsync(items, ct);
            if (!result.Success) return Conflict(result);
            return Ok(result);
        }

        /// <summary>
        /// POST api/inventory/release
        /// </summary>
        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] List<InventoryRequestDto> items, CancellationToken ct)
        {
            await _inventoryService.ReleaseAsync(items, ct);
            return NoContent();
        }

    }
}