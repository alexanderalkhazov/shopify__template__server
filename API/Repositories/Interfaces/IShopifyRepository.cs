using API.Models.Shopify;

namespace API.Repositories.Interfaces;

/// <summary>
/// Repository for managing Shopify data
/// </summary>
public interface IShopifyRepository
{
    // Shop management
    Task<ShopifyShop?> GetShopByDomainAsync(string shopDomain);
    Task<ShopifyShop?> GetShopByIdAsync(int shopId);
    Task<List<ShopifyShop>> GetActiveShopsAsync();
    Task<List<ShopifyShop>> GetAllShopsAsync();
    Task AddShopAsync(ShopifyShop shop);
    Task UpdateShopAsync(ShopifyShop shop);
    Task DeleteShopAsync(int shopId);

    // Webhook management
    Task<List<ShopifyWebhook>> GetWebhooksByShopIdAsync(int shopId);
    Task<ShopifyWebhook?> GetWebhookByIdAsync(int webhookId);
    Task AddWebhookAsync(ShopifyWebhook webhook);
    Task UpdateWebhookAsync(ShopifyWebhook webhook);
    Task DeleteWebhookAsync(int webhookId);
    Task DeleteWebhooksByShopIdAsync(int shopId);

    // Analytics and reporting
    Task<int> GetActiveShopCountAsync();
    Task<List<ShopifyShop>> GetRecentInstallationsAsync(int days = 30);
    Task<List<ShopifyShop>> GetShopsWithoutWebhooksAsync();
}