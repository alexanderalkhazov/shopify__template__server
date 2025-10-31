using API.Models.Shopify;

namespace API.Services.Interfaces;

/// <summary>
/// Service for managing Shopify app installations and OAuth
/// </summary>
public interface IShopifyAuthService
{
    /// <summary>
    /// Generate OAuth authorization URL for shop installation
    /// </summary>
    string GetAuthorizationUrl(string shopDomain, string state = "");

    /// <summary>
    /// Exchange authorization code for access token
    /// </summary>
    Task<ShopifyOAuthTokenResponse?> ExchangeCodeForTokenAsync(string shopDomain, string code);

    /// <summary>
    /// Verify HMAC signature for webhook or OAuth
    /// </summary>
    bool VerifyHmacSignature(string data, string signature);

    /// <summary>
    /// Install shop and store access token
    /// </summary>
    Task<ShopifyShop> InstallShopAsync(string shopDomain, string accessToken, string scopes);

    /// <summary>
    /// Get shop information from Shopify API
    /// </summary>
    Task<ShopifyShopInfo?> GetShopInfoAsync(string shopDomain, string accessToken);

    /// <summary>
    /// Check if shop has required scopes
    /// </summary>
    bool HasRequiredScopes(string[] currentScopes, string[] requiredScopes);

    /// <summary>
    /// Uninstall shop (mark as inactive)
    /// </summary>
    Task UninstallShopAsync(string shopDomain);
}

/// <summary>
/// Service for managing Shopify webhooks
/// </summary>
public interface IShopifyWebhookService
{
    /// <summary>
    /// Create webhook on Shopify
    /// </summary>
    Task<bool> CreateWebhookAsync(string shopDomain, string accessToken, string topic, string address);

    /// <summary>
    /// Create all required webhooks for a shop
    /// </summary>
    Task<bool> CreateRequiredWebhooksAsync(string shopDomain, string accessToken);

    /// <summary>
    /// Process incoming webhook
    /// </summary>
    Task ProcessWebhookAsync(string topic, string shopDomain, string payload);

    /// <summary>
    /// Verify webhook signature
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature);

    /// <summary>
    /// Handle order webhook
    /// </summary>
    Task HandleOrderWebhookAsync(string topic, ShopifyOrderWebhook order, string shopDomain);

    /// <summary>
    /// Handle product webhook
    /// </summary>
    Task HandleProductWebhookAsync(string topic, ShopifyProductWebhook product, string shopDomain);

    /// <summary>
    /// Handle app uninstalled webhook
    /// </summary>
    Task HandleAppUninstalledAsync(string shopDomain);
}

/// <summary>
/// Service for interacting with Shopify API
/// </summary>
public interface IShopifyApiService
{
    /// <summary>
    /// Get products from shop
    /// </summary>
    Task<List<ShopifyProductWebhook>> GetProductsAsync(string shopDomain, string accessToken, int limit = 50);

    /// <summary>
    /// Get orders from shop
    /// </summary>
    Task<List<ShopifyOrderWebhook>> GetOrdersAsync(string shopDomain, string accessToken, int limit = 50);

    /// <summary>
    /// Create product in shop
    /// </summary>
    Task<ShopifyProductWebhook?> CreateProductAsync(string shopDomain, string accessToken, object productData);

    /// <summary>
    /// Update product in shop
    /// </summary>
    Task<ShopifyProductWebhook?> UpdateProductAsync(string shopDomain, string accessToken, long productId, object productData);

    /// <summary>
    /// Get shop information
    /// </summary>
    Task<ShopifyShopInfo?> GetShopAsync(string shopDomain, string accessToken);

    /// <summary>
    /// Make authenticated API request
    /// </summary>
    Task<T?> MakeApiRequestAsync<T>(string shopDomain, string accessToken, string endpoint, HttpMethod method, object? data = null);
}