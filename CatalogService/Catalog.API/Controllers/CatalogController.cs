using Microsoft.AspNetCore.Mvc;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using CatalogService.Catalog.Application.DTOs;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CatalogCreateDto dto)
    {
        var id = await _catalogService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CatalogUpdateDto dto)
    {
        dto.ProductId = id; // Set the ID from route
        await _catalogService.UpdateAsync(dto);
        return Ok("Updated");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _catalogService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var results = await _catalogService.SearchAsync(keyword);
        return Ok(results);
    }
}