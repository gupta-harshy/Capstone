namespace CatalogService.Catalog.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<string> CreateInventoryAsync(string productName, int quantity, decimal price);
    }
}
