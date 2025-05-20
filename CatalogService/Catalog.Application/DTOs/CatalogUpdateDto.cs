namespace CatalogService.Catalog.Application.DTOs
{
    public class CatalogUpdateDto
    {
        public string ProductId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public string Category { get; set; } = default!;
    }
}
