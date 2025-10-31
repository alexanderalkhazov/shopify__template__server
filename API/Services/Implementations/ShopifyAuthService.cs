using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using API.Models.Shopify;
using API.Services.Interfaces;
using API.Repositories.Interfaces;

namespace API.Services.Implementations;

/// <summary>
/// Shopify OAuth and authentication service
/// </summary>
public class ShopifyAuthService : IShopifyAuthService
{
    private readonly ShopifySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IShopifyRepository _shopifyRepository;
    private readonly ILogger<ShopifyAuthService> _logger;

    public ShopifyAuthService(
        IOptions<ShopifySettings> settings,
        HttpClient httpClient,
        IShopifyRepository shopifyRepository,
        ILogger<ShopifyAuthService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _shopifyRepository = shopifyRepository;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string shopDomain, string state = "")
    {
        var scopes = Uri.EscapeDataString(_settings.Scopes);
        var redirectUri = Uri.EscapeDataString(_settings.RedirectUrl);
        var nonce = Guid.NewGuid().ToString();

        var url = $"https://{shopDomain}/admin/oauth/authorize" +
                  $"?client_id={_settings.ClientId}" +
                  $"&scope={scopes}" +
                  $"&redirect_uri={redirectUri}" +
                  $"&state={state}" +
                  $"&grant_options[]=";

        return url;
    }

    public async Task<ShopifyOAuthTokenResponse?> ExchangeCodeForTokenAsync(string shopDomain, string code)
    {
        try
        {
            var requestData = new
            {
                client_id = _settings.ClientId,
                client_secret = _settings.ClientSecret,
                code = code
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://{shopDomain}/admin/oauth/access_token", 
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ShopifyOAuthTokenResponse>(responseContent);
            }

            _logger.LogError("Failed to exchange code for token. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token for shop {ShopDomain}", shopDomain);
            return null;
        }
    }

    public bool VerifyHmacSignature(string data, string signature)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_settings.ClientSecret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(key);
            var computedHash = hmac.ComputeHash(dataBytes);
            var computedSignature = Convert.ToBase64String(computedHash);

            return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying HMAC signature");
            return false;
        }
    }

    public async Task<ShopifyShop> InstallShopAsync(string shopDomain, string accessToken, string scopes)
    {
        try
        {
            // Get shop info from Shopify
            var shopInfo = await GetShopInfoAsync(shopDomain, accessToken);

            // Check if shop already exists
            var existingShop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
            
            if (existingShop != null)
            {
                // Update existing shop
                existingShop.AccessToken = accessToken;
                existingShop.Scopes = scopes;
                existingShop.IsActive = true;
                existingShop.UninstalledAt = null;
                existingShop.UpdatedAt = DateTime.UtcNow;
                existingShop.LastActivity = DateTime.UtcNow;

                if (shopInfo?.Shop != null)
                {
                    existingShop.ShopName = shopInfo.Shop.Name;
                    existingShop.ShopEmail = shopInfo.Shop.Email;
                    existingShop.ShopOwner = shopInfo.Shop.ShopOwner;
                    existingShop.PlanName = shopInfo.Shop.PlanName;
                    existingShop.CountryCode = shopInfo.Shop.CountryCode;
                    existingShop.Currency = shopInfo.Shop.Currency;
                }

                await _shopifyRepository.UpdateShopAsync(existingShop);
                return existingShop;
            }
            else
            {
                // Create new shop
                var newShop = new ShopifyShop
                {
                    ShopDomain = shopDomain,
                    AccessToken = accessToken,
                    Scopes = scopes,
                    IsActive = true,
                    InstalledAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                if (shopInfo?.Shop != null)
                {
                    newShop.ShopName = shopInfo.Shop.Name;
                    newShop.ShopEmail = shopInfo.Shop.Email;
                    newShop.ShopOwner = shopInfo.Shop.ShopOwner;
                    newShop.PlanName = shopInfo.Shop.PlanName;
                    newShop.CountryCode = shopInfo.Shop.CountryCode;
                    newShop.Currency = shopInfo.Shop.Currency;
                }

                await _shopifyRepository.AddShopAsync(newShop);
                return newShop;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing shop {ShopDomain}", shopDomain);
            throw;
        }
    }

    public async Task<ShopifyShopInfo?> GetShopInfoAsync(string shopDomain, string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

            var response = await _httpClient.GetAsync(
                $"https://{shopDomain}/admin/api/{_settings.ApiVersion}/shop.json");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ShopifyShopInfo>(content);
            }

            _logger.LogError("Failed to get shop info. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shop info for {ShopDomain}", shopDomain);
            return null;
        }
    }

    public bool HasRequiredScopes(string[] currentScopes, string[] requiredScopes)
    {
        return requiredScopes.All(required => 
            currentScopes.Contains(required, StringComparer.OrdinalIgnoreCase));
    }

    public async Task UninstallShopAsync(string shopDomain)
    {
        try
        {
            var shop = await _shopifyRepository.GetShopByDomainAsync(shopDomain);
            if (shop != null)
            {
                shop.IsActive = false;
                shop.UninstalledAt = DateTime.UtcNow;
                shop.UpdatedAt = DateTime.UtcNow;
                
                await _shopifyRepository.UpdateShopAsync(shop);
                
                _logger.LogInformation("Shop {ShopDomain} uninstalled", shopDomain);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uninstalling shop {ShopDomain}", shopDomain);
            throw;
        }
    }
}