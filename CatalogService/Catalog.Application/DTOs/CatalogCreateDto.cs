namespace Catalog.Application.DTOs;

public class CatalogCreateDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; } = default!;
}