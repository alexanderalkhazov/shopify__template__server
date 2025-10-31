using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;
using API.Models.Shopify;
using API.Repositories.Interfaces;
using System.Text;

namespace API.Endpoints;

/// <summary>
/// Shopify OAuth, webhook, and API endpoints
/// </summary>
public static class ShopifyEndpoints
{
    public static void MapShopifyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shopify")
            .WithTags("Shopify")
            .WithOpenApi();

        // OAuth endpoints
        group.MapGet("/auth", (
            [FromQuery] string shop,
            [FromQuery] string? state,
            IShopifyAuthService authService) =>
        {
            try
            {
                if (string.IsNullOrEmpty(shop))
                {
                    return Results.BadRequest(new { error = "Shop parameter is required" });
                }

                // Normalize shop domain
                var shopDomain = shop.Contains(".myshopify.com") ? shop : $"{shop}.myshopify.com";
                
                var authUrl = authService.GetAuthorizationUrl(shopDomain, state ?? "");
                
                return Results.Ok(new 
                { 
                    authUrl,
                    shopDomain,
                    message = "Redirect user to this URL to begin OAuth flow"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error generating auth URL: {ex.Message}");
            }
        })
        .WithName("InitiateShopifyAuth")
        .WithSummary("Initiate Shopify OAuth flow")
        .WithDescription("Generate OAuth authorization URL for shop installation");

        group.MapGet("/auth/callback", async (
            [FromQuery] string shop,
            [FromQuery] string code,
            [FromQuery] string? state,
            [FromQuery] string? hmac,
            IShopifyAuthService authService,
            IShopifyWebhookService webhookService) =>
        {
            try
            {
                if (string.IsNullOrEmpty(shop) || string.IsNullOrEmpty(code))
                {
                    return Results.BadRequest(new { error = "Shop and code parameters are required" });
                }

                // Normalize shop domain
                var shopDomain = shop.Contains(".myshopify.com") ? shop : $"{shop}.myshopify.com";

                // Exchange code for access token
                var tokenResponse = await authService.ExchangeCodeForTokenAsync(shopDomain, code);
                
                if (tokenResponse == null)
                {
                    return Results.BadRequest(new { error = "Failed to exchange code for access token" });
                }

                // Install shop
                var installedShop = await authService.InstallShopAsync(shopDomain, tokenResponse.AccessToken, tokenResponse.Scope);

                // Create webhooks
                var webhooksCreated = await webhookService.CreateRequiredWebhooksAsync(shopDomain, tokenResponse.AccessToken);

                return Results.Ok(new
                {
                    success = true,
                    shop = new
                    {
                        domain = installedShop.ShopDomain,
                        name = installedShop.ShopName,
                        email = installedShop.ShopEmail,
                        scopes = installedShop.GetScopesArray(),
                        webhooksConfigured = webhooksCreated
                    },
                    message = "Shop installed successfully"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error processing OAuth callback: {ex.Message}");
            }
        })
        .WithName("ShopifyAuthCallback")
        .WithSummary("Handle Shopify OAuth callback")
        .WithDescription("Process OAuth callback and complete shop installation");

        // Webhook endpoints
        group.MapPost("/webhooks/{topic}", async (
            string topic,
            [FromHeader(Name = "X-Shopify-Shop-Domain")] string shopDomain,
            [FromHeader(Name = "X-Shopify-Hmac-Sha256")] string signature,
            HttpRequest request,
            IShopifyWebhookService webhookService) =>
        {
            try
            {
                // Read the request body
                using var reader = new StreamReader(request.Body, Encoding.UTF8);
                var payload = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(payload))
                {
                    return Results.BadRequest(new { error = "Empty payload" });
                }

                // Verify webhook signature
                if (!webhookService.VerifyWebhookSignature(payload, signature))
                {
                    return Results.Unauthorized();
                }

                // Process webhook
                var webhookTopic = topic.Replace("-", "/");
                await webhookService.ProcessWebhookAsync(webhookTopic, shopDomain, payload);

                return Results.Ok(new { success = true, message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error processing webhook: {ex.Message}");
            }
        })
        .WithName("ProcessShopifyWebhook")
        .WithSummary("Process Shopify webhook")
        .WithDescription("Handle incoming Shopify webhooks");

        // API endpoints
        group.MapGet("/shops", async (IShopifyRepository shopifyRepository) =>
        {
            try
            {
                var shops = await shopifyRepository.GetActiveShopsAsync();
                
                var shopsData = shops.Select(s => new
                {
                    id = s.Id,
                    domain = s.ShopDomain,
                    name = s.ShopName,
                    email = s.ShopEmail,
                    owner = s.ShopOwner,
                    plan = s.PlanName,
                    country = s.CountryCode,
                    currency = s.Currency,
                    scopes = s.GetScopesArray(),
                    webhooksConfigured = s.WebhooksConfigured,
                    installedAt = s.InstalledAt,
                    lastActivity = s.LastActivity
                }).ToList();

                return Results.Ok(new { shops = shopsData, count = shopsData.Count });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting shops: {ex.Message}");
            }
        })
        .WithName("GetShops")
        .WithSummary("Get all installed shops")
        .WithDescription("Retrieve list of all installed Shopify shops");

        group.MapGet("/shops/{shopDomain}/products", async (
            string shopDomain,
            [FromQuery] int limit,
            IShopifyRepository shopifyRepository,
            IShopifyApiService apiService) =>
        {
            try
            {
                var shop = await shopifyRepository.GetShopByDomainAsync(shopDomain);
                if (shop == null || !shop.IsActive)
                {
                    return Results.NotFound(new { error = "Shop not found or inactive" });
                }

                var products = await apiService.GetProductsAsync(shopDomain, shop.AccessToken, limit > 0 ? limit : 50);
                
                return Results.Ok(new { products, count = products.Count });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting products: {ex.Message}");
            }
        })
        .WithName("GetShopProducts")
        .WithSummary("Get products from shop")
        .WithDescription("Retrieve products from a specific Shopify shop");

        group.MapGet("/shops/{shopDomain}/orders", async (
            string shopDomain,
            [FromQuery] int limit,
            IShopifyRepository shopifyRepository,
            IShopifyApiService apiService) =>
        {
            try
            {
                var shop = await shopifyRepository.GetShopByDomainAsync(shopDomain);
                if (shop == null || !shop.IsActive)
                {
                    return Results.NotFound(new { error = "Shop not found or inactive" });
                }

                var orders = await apiService.GetOrdersAsync(shopDomain, shop.AccessToken, limit > 0 ? limit : 50);
                
                return Results.Ok(new { orders, count = orders.Count });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting orders: {ex.Message}");
            }
        })
        .WithName("GetShopOrders")
        .WithSummary("Get orders from shop")
        .WithDescription("Retrieve orders from a specific Shopify shop");

        group.MapPost("/shops/{shopDomain}/webhooks/setup", async (
            string shopDomain,
            IShopifyRepository shopifyRepository,
            IShopifyWebhookService webhookService) =>
        {
            try
            {
                var shop = await shopifyRepository.GetShopByDomainAsync(shopDomain);
                if (shop == null || !shop.IsActive)
                {
                    return Results.NotFound(new { error = "Shop not found or inactive" });
                }

                var success = await webhookService.CreateRequiredWebhooksAsync(shopDomain, shop.AccessToken);
                
                return Results.Ok(new 
                { 
                    success,
                    message = success ? "Webhooks configured successfully" : "Some webhooks failed to configure"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error setting up webhooks: {ex.Message}");
            }
        })
        .WithName("SetupShopWebhooks")
        .WithSummary("Setup webhooks for shop")
        .WithDescription("Create required webhooks for a specific shop");

        // Analytics endpoints
        group.MapGet("/analytics/overview", async (IShopifyRepository shopifyRepository) =>
        {
            try
            {
                var activeShopCount = await shopifyRepository.GetActiveShopCountAsync();
                var recentInstalls = await shopifyRepository.GetRecentInstallationsAsync(30);
                var shopsWithoutWebhooks = await shopifyRepository.GetShopsWithoutWebhooksAsync();

                return Results.Ok(new
                {
                    activeShops = activeShopCount,
                    recentInstalls = recentInstalls.Count,
                    shopsWithoutWebhooks = shopsWithoutWebhooks.Count,
                    recentInstallations = recentInstalls.Select(s => new
                    {
                        domain = s.ShopDomain,
                        name = s.ShopName,
                        installedAt = s.InstalledAt
                    }).Take(10).ToList()
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting analytics: {ex.Message}");
            }
        })
        .WithName("GetShopifyAnalytics")
        .WithSummary("Get Shopify app analytics")
        .WithDescription("Retrieve analytics and overview of installed shops");

        // Utility endpoints
        group.MapGet("/scopes/check/{shopDomain}", async (
            string shopDomain,
            IShopifyRepository shopifyRepository,
            IShopifyAuthService authService) =>
        {
            try
            {
                var shop = await shopifyRepository.GetShopByDomainAsync(shopDomain);
                if (shop == null)
                {
                    return Results.NotFound(new { error = "Shop not found" });
                }

                // You would need to add required scopes to settings
                var requiredScopes = new[] { "read_products", "write_products", "read_orders" }; // Example
                var currentScopes = shop.GetScopesArray();
                var hasRequired = authService.HasRequiredScopes(currentScopes, requiredScopes);

                return Results.Ok(new
                {
                    shopDomain = shop.ShopDomain,
                    currentScopes,
                    requiredScopes,
                    hasRequiredScopes = hasRequired,
                    missingScopes = requiredScopes.Except(currentScopes).ToArray()
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error checking scopes: {ex.Message}");
            }
        })
        .WithName("CheckShopScopes")
        .WithSummary("Check shop OAuth scopes")
        .WithDescription("Verify if shop has required OAuth scopes");
    }
}