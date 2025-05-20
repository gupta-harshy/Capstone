namespace OrderService.Domain.APIClient;

public interface ICartApiClient
{
    Task<CartDto?> GetCartAsync(CancellationToken ct);
}
