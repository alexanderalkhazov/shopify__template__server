using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using API.Models.Shopify;
using API.Services.Interfaces;

namespace API.Services.Implementations;

/// <summary>
/// Shopify API service for making authenticated requests
/// </summary>
public class ShopifyApiService : IShopifyApiService
{
    private readonly ShopifySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ShopifyApiService> _logger;

    public ShopifyApiService(
        IOptions<ShopifySettings> settings,
        HttpClient httpClient,
        ILogger<ShopifyApiService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ShopifyProductWebhook>> GetProductsAsync(string shopDomain, string accessToken, int limit = 50)
    {
        try
        {
            var response = await MakeApiRequestAsync<JsonElement>(
                shopDomain, 
                accessToken, 
                $"products.json?limit={limit}", 
                HttpMethod.Get);

            if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("products", out var productsElement))
            {
                var products = JsonSerializer.Deserialize<List<ShopifyProductWebhook>>(productsElement.GetRawText());
                return products ?? new List<ShopifyProductWebhook>();
            }

            return new List<ShopifyProductWebhook>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for shop {ShopDomain}", shopDomain);
            throw;
        }
    }

    public async Task<List<ShopifyOrderWebhook>> GetOrdersAsync(string shopDomain, string accessToken, int limit = 50)
    {
        try
        {
            var response = await MakeApiRequestAsync<JsonElement>(
                shopDomain, 
                accessToken, 
                $"orders.json?limit={limit}", 
                HttpMethod.Get);

            if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("orders", out var ordersElement))
            {
                var orders = JsonSerializer.Deserialize<List<ShopifyOrderWebhook>>(ordersElement.GetRawText());
                return orders ?? new List<ShopifyOrderWebhook>();
            }

            return new List<ShopifyOrderWebhook>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for shop {ShopDomain}", shopDomain);
            throw;
        }
    }

    public async Task<ShopifyProductWebhook?> CreateProductAsync(string shopDomain, string accessToken, object productData)
    {
        try
        {
            var response = await MakeApiRequestAsync<JsonElement>(
                shopDomain, 
                accessToken, 
                "products.json", 
                HttpMethod.Post, 
                new { product = productData });

            if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("product", out var productElement))
            {
                return JsonSerializer.Deserialize<ShopifyProductWebhook>(productElement.GetRawText());
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product for shop {ShopDomain}", shopDomain);
            throw;
        }
    }

    public async Task<ShopifyProductWebhook?> UpdateProductAsync(string shopDomain, string accessToken, long productId, object productData)
    {
        try
        {
            var response = await MakeApiRequestAsync<JsonElement>(
                shopDomain, 
                accessToken, 
                $"products/{productId}.json", 
                HttpMethod.Put, 
                new { product = productData });

            if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("product", out var productElement))
            {
                return JsonSerializer.Deserialize<ShopifyProductWebhook>(productElement.GetRawText());
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId} for shop {ShopDomain}", productId, shopDomain);
            throw;
        }
    }

    public async Task<ShopifyShopInfo?> GetShopAsync(string shopDomain, string accessToken)
    {
        try
        {
            var response = await MakeApiRequestAsync<ShopifyShopInfo>(
                shopDomain, 
                accessToken, 
                "shop.json", 
                HttpMethod.Get);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shop info for {ShopDomain}", shopDomain);
            throw;
        }
    }

    public async Task<T?> MakeApiRequestAsync<T>(string shopDomain, string accessToken, string endpoint, HttpMethod method, object? data = null)
    {
        try
        {
            var url = $"https://{shopDomain}/admin/api/{_settings.ApiVersion}/{endpoint}";
            
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-Shopify-Access-Token", accessToken);

            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (typeof(T) == typeof(JsonElement))
                {
                    var jsonDocument = JsonDocument.Parse(content);
                    return (T)(object)jsonDocument.RootElement;
                }
                
                return JsonSerializer.Deserialize<T>(content) ?? default(T);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Shopify API request failed. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                
                return default(T);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making Shopify API request to {Endpoint}", endpoint);
            throw;
        }
    }
}