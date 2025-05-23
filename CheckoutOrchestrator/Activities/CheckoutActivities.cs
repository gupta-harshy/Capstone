using CheckoutOrchestrator.Entities;
using CommonDependencies;
using StackExchange.Redis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace CheckoutOrchestrator.Activities;

public class CheckoutActivities : ICheckoutActivities
{
    private readonly IHttpClientFactory _factory;
    public CheckoutActivities(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    [Activity]
    public async Task<AuthResponse> GetAuthTokenAsync(Guid userId)
    {
        var _authClient = _factory.CreateClient("auth");
        var response = await _authClient.PostAsJsonAsync("/api/Auth/generate-token-userid", userId);
        response.EnsureSuccessStatusCode();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(options);
        return authResponse ?? throw new ApplicationException("Failed to deserialize the authentication response.");
    }

    [Activity]
    public async Task<OrderResultDto> GetOrderDetailsAsync(Guid orderId)
    {
        var _orderClient = _factory.CreateClient("order");
        var json = await _orderClient.GetFromJsonAsync<dynamic>($"/api/orders/{orderId}");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        var order = JsonSerializer.Deserialize<OrderResultDto>(json, options);
        if (order == null)
            throw new ApplicationException($"Order {orderId} not found");
        return order;
    }

    [Activity]
    public async Task ReserveInventoryAsync(List<InventoryRequestDto> items)
    {
        var _inventoryClient = _factory.CreateClient("inventory");
        var resp = await _inventoryClient.PostAsJsonAsync("/api/Inventory/reserve", items);
        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var result = await resp.Content.ReadFromJsonAsync<ReserveInventoryResultDto>()!;
            throw new ApplicationFailureException(
                string.Join(", ", (result != null) ? result.Shortages.Select(s =>
                    $"{s.ProductId}: needed {s.RequestedQty}, avail {s.AvailableQty}") : "No Records"),
                    "InventoryReservationFailed", 
                    true);
        }
        resp.EnsureSuccessStatusCode();
    }

    [Activity]
    public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
    {
        var _orderClient = _factory.CreateClient("order");
        // Prepare the DTO
        var dto = new { Status = newStatus };

        // Call the Order Service
        var resp = await _orderClient.PutAsJsonAsync($"/api/orders/{orderId}/status", dto);

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new ApplicationFailureException($"Order {orderId} status '{newStatus}' is invalid: {error}", "InvalidOrderStatus");
        }

        if (resp.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new ApplicationFailureException(
                $"Failed to update status for order {orderId} to '{newStatus}'. " +
                $"HTTP {(int)resp.StatusCode}: {body}",
                "UpdateOrderStatusFailed",
                nonRetryable: false);
        }
        // 204 No Content → success
    }

    [Activity]
    public async Task ProcessPaymentAsync(Guid orderId, decimal amount)
    {
        // 1) Simulate variable processing time (0–10 minutes)
        Random _rng = new();
        var delayMs = _rng.Next(0, 200_000);
        await Task.Delay(delayMs);

        // 2) 2/3 chance of success
        if (_rng.Next(3) < 2)
        {
            return;
        }

        // 3) Simulated failure
        throw new ApplicationFailureException ($"Mock payment failure for order {orderId}", "PaymentFailed", true);
    }

    /// <summary>
    /// Release (add back) each requested qty in one batch.
    /// </summary>
    [Activity]
    public async Task ReleaseInventoryAsync(List<InventoryRequestDto> items)
    {
        var _inventoryClient = _factory.CreateClient("inventory");
        var resp = await _inventoryClient
            .PostAsJsonAsync("/api/inventory/release", items);

        // Inventory release endpoint should return 204 NoContent on success
        if (resp.StatusCode != HttpStatusCode.NoContent)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new ApplicationFailureException($"Inventory release failed: {error}", "ReleaseInventoryFailed");
        }
    }


    [Activity]
    public async Task SendOrderConfirmedEventAsync(Guid orderId)
    {
        // Publish event
        await Task.CompletedTask;
    }

    [Activity]
    public async Task ClearUserCart(string token)
    {
        var _cartClient = _factory.CreateClient("cart");
        _cartClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _cartClient.PostAsJsonAsync("/api/Cart/clear", new StringContent(string.Empty));
        response.EnsureSuccessStatusCode();
    }
}
