using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CommonDependencies
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientService(HttpClient httpClient, 
                            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;                       
        }

        /// <summary>
        /// Generic method for making GET requests
        /// </summary>
        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var token = ExtractToken();
                if (!string.IsNullOrEmpty(token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

                return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new Exception($"GET request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Generic method for making POST requests
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {                
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var token = ExtractToken();
                if (!string.IsNullOrEmpty(token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

                var responseContent = await response.Content.ReadAsStringAsync();
                if (typeof(TResponse) == typeof(string))
                {
                    // Return raw string directly
                    return (TResponse)(object)responseContent;
                }
                
                return JsonSerializer.Deserialize<TResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new Exception($"POST request failed: {ex.Message}");
            }
        }

        private string ExtractToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return string.Empty;
        }
    }
}