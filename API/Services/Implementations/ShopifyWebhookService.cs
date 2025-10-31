using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using API.Models.Shopify;
using API.Services.Interfaces;
using API.Repositories.Interfaces;

namespace API.Services.Implementations;

/// <summary>
/// Shopify webhook management service
/// </summary>
public class ShopifyWebhookService : IShopifyWebhookService
{
    private readonly ShopifySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IShopifyRepository _shopifyRepository;
    private readonly IDiscordService _discordService;
    private readonly ILogger<ShopifyWebhookService> _logger;

    public ShopifyWebhookService(
        IOptions<ShopifySettings> settings,
        HttpClient httpClient,
        IShopifyRepository shopifyRepository,
        IDiscordService discordService,
        ILogger<ShopifyWebhookService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _shopifyRepository = shopifyRepository;
        _discordService = discordService;
        _logger = logger;
    }

    public async Task<bool> CreateWebhookAsync(string shopDomain, string accessToken, string topic, string address)
    {
        try
        {
            var webhookRequest = new ShopifyWebhookRequest
            {
                Webhook = new ShopifyWebhookData
                {
                    Topic = topic,
                    Address = address,
                    Format = "json"
                }
            };

            var json = JsonSerializer.Serialize(webhookRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

            var response = await _httpClient.PostAsync(
                $"https://{shopDomain}/admin/api/{_settings.ApiVersion}/webhooks.json",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var webhookResponse = JsonSerializer.Deserialize<ShopifyWebhookResponse>(responseContent);

                if (webhookResponse?.Webhook != null)
                {
                    // Store webhook info in database
                    var shop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
                    if (shop != null)
                    {
                        var webhook = new ShopifyWebhook
                        {
                            ShopId = shop.Id,
                            WebhookId = webhookResponse.Webhook.Id.ToString(),
                            Topic = topic,
                            Address = address,
                            IsActive = true
                        };

                        await _shopifyRepository.AddWebhookAsync(webhook);
                    }

                    _logger.LogInformation("Created webhook {Topic} for shop {ShopDomain}", topic, shopDomain);
                    return true;
                }
            }

            _logger.LogError("Failed to create webhook {Topic} for shop {ShopDomain}. Status: {StatusCode}", 
                topic, shopDomain, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating webhook {Topic} for shop {ShopDomain}", topic, shopDomain);
            return false;
        }
    }

    public async Task<bool> CreateRequiredWebhooksAsync(string shopDomain, string accessToken)
    {
        if (!_settings.EnableWebhooks || !_settings.RequiredWebhooks.Any())
        {
            _logger.LogInformation("Webhooks disabled or no required webhooks configured");
            return true;
        }

        var successCount = 0;
        var baseUrl = _settings.AppUrl.TrimEnd('/');
        var webhookEndpoint = _settings.WebhookEndpoint.TrimStart('/');

        foreach (var topic in _settings.RequiredWebhooks)
        {
            var address = $"{baseUrl}/{webhookEndpoint}/{topic.Replace("/", "-")}";
            var success = await CreateWebhookAsync(shopDomain, accessToken, topic, address);
            
            if (success)
            {
                successCount++;
            }
        }

        var allSuccess = successCount == _settings.RequiredWebhooks.Count;
        
        // Update shop webhooks configured status
        var shop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
        if (shop != null)
        {
            shop.WebhooksConfigured = allSuccess;
            await _shopifyRepository.UpdateShopAsync(shop);
        }

        _logger.LogInformation("Created {SuccessCount}/{TotalCount} webhooks for shop {ShopDomain}", 
            successCount, _settings.RequiredWebhooks.Count, shopDomain);

        return allSuccess;
    }

    public async Task ProcessWebhookAsync(string topic, string shopDomain, string payload)
    {
        try
        {
            _logger.LogInformation("Processing webhook {Topic} for shop {ShopDomain}", topic, shopDomain);

            // Update shop last activity
            var shop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
            if (shop != null)
            {
                shop.LastActivity = DateTime.UtcNow;
                await _shopifyRepository.UpdateShopAsync(shop);
            }

            // Route to appropriate handler
            switch (topic.ToLowerInvariant())
            {
                case "orders/create":
                case "orders/updated":
                case "orders/paid":
                case "orders/cancelled":
                    var order = JsonSerializer.Deserialize<ShopifyOrderWebhook>(payload);
                    if (order != null)
                    {
                        await HandleOrderWebhookAsync(topic, order, shopDomain);
                    }
                    break;

                case "products/create":
                case "products/update":
                    var product = JsonSerializer.Deserialize<ShopifyProductWebhook>(payload);
                    if (product != null)
                    {
                        await HandleProductWebhookAsync(topic, product, shopDomain);
                    }
                    break;

                case "app/uninstalled":
                    await HandleAppUninstalledAsync(shopDomain);
                    break;

                default:
                    _logger.LogWarning("Unhandled webhook topic: {Topic}", topic);
                    break;
            }

            _logger.LogInformation("Successfully processed webhook {Topic} for shop {ShopDomain}", topic, shopDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook {Topic} for shop {ShopDomain}", topic, shopDomain);
            
            // Send error notification to Discord
            await _discordService.SendErrorNotificationAsync(
                $"Webhook Processing Error - {topic}",
                $"Failed to process webhook for shop {shopDomain}: {ex.Message}"
            );
            
            throw;
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_settings.WebhookSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(key);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToBase64String(computedHash);

            return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    public async Task HandleOrderWebhookAsync(string topic, ShopifyOrderWebhook order, string shopDomain)
    {
        try
        {
            var fields = new Dictionary<string, string>
            {
                { "Order ID", order.Id.ToString() },
                { "Order Number", order.OrderNumber.ToString() },
                { "Total", $"{order.TotalPrice} {order.Currency}" },
                { "Status", order.FinancialStatus ?? "Unknown" },
                { "Customer", order.Customer?.Email ?? "Guest" }
            };

            var message = topic switch
            {
                "orders/create" => $"New order created in {shopDomain}",
                "orders/updated" => $"Order updated in {shopDomain}",
                "orders/paid" => $"Order payment received in {shopDomain}",
                "orders/cancelled" => $"Order cancelled in {shopDomain}",
                _ => $"Order event ({topic}) in {shopDomain}"
            };

            await _discordService.SendNotificationAsync(
                $"Shopify Order Event - {topic}",
                message,
                fields
            );

            _logger.LogInformation("Handled order webhook {Topic} for order {OrderId} in shop {ShopDomain}", 
                topic, order.Id, shopDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling order webhook {Topic} for order {OrderId}", topic, order.Id);
            throw;
        }
    }

    public async Task HandleProductWebhookAsync(string topic, ShopifyProductWebhook product, string shopDomain)
    {
        try
        {
            var fields = new Dictionary<string, string>
            {
                { "Product ID", product.Id.ToString() },
                { "Title", product.Title },
                { "Type", product.ProductType },
                { "Vendor", product.Vendor },
                { "Status", product.Status },
                { "Variants", product.Variants.Count.ToString() }
            };

            var message = topic switch
            {
                "products/create" => $"New product created in {shopDomain}",
                "products/update" => $"Product updated in {shopDomain}",
                _ => $"Product event ({topic}) in {shopDomain}"
            };

            await _discordService.SendNotificationAsync(
                $"Shopify Product Event - {topic}",
                message,
                fields
            );

            _logger.LogInformation("Handled product webhook {Topic} for product {ProductId} in shop {ShopDomain}", 
                topic, product.Id, shopDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling product webhook {Topic} for product {ProductId}", topic, product.Id);
            throw;
        }
    }

    public async Task HandleAppUninstalledAsync(string shopDomain)
    {
        try
        {
            // Mark shop as uninstalled
            var shop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
            if (shop != null)
            {
                shop.IsActive = false;
                shop.UninstalledAt = DateTime.UtcNow;
                shop.UpdatedAt = DateTime.UtcNow;
                
                await _shopifyRepository.UpdateShopAsync(shop);

                // Send notification
                await _discordService.SendErrorNotificationAsync(
                    "Shopify App Uninstalled",
                    $"Shop {shopDomain} has uninstalled the app"
                );

                _logger.LogInformation("App uninstalled for shop {ShopDomain}", shopDomain);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling app uninstalled for shop {ShopDomain}", shopDomain);
            throw;
        }
    }
}