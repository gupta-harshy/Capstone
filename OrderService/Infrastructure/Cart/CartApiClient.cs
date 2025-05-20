using System.Net.Http.Json;
using CommonDependencies;
using OrderService.Domain.APIClient;

namespace OrderService.Infrastructure.Cart;

public class CartApiClient : ICartApiClient
{
    private readonly HttpClientService _httpClientService;
    private readonly IConfiguration _configuration;

    public CartApiClient(HttpClientService httpClientService, IConfiguration configuration)
    {
        _httpClientService = httpClientService;
        this._configuration = configuration;
    }
        
    public async Task<CartDto?> GetCartAsync(CancellationToken ct)
    {
        string url = $"{_configuration["CartService:BaseUrl"]}/api/cart";
        var resp = await _httpClientService.GetAsync<CartDto?>(url);
        return resp;
    }
}
