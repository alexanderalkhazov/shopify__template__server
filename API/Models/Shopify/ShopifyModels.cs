using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Models.Shopify;

/// <summary>
/// Shopify configuration settings
/// </summary>
public class ShopifySettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string AppUrl { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-10";
    public string WebhookEndpoint { get; set; } = "/api/shopify/webhooks";
    public bool EnableWebhooks { get; set; } = true;
    public List<string> RequiredWebhooks { get; set; } = new();

    public string[] GetScopesArray() => Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim()).ToArray();
}

/// <summary>
/// Shopify shop installation record
/// </summary>
[Table("shopify_shops")]
public class ShopifyShop
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("shop_domain")]
    [MaxLength(255)]
    public string ShopDomain { get; set; } = string.Empty;

    [Required]
    [Column("access_token")]
    [MaxLength(255)]
    public string AccessToken { get; set; } = string.Empty;

    [Column("scopes")]
    [MaxLength(1000)]
    public string Scopes { get; set; } = string.Empty;

    [Column("shop_name")]
    [MaxLength(255)]
    public string? ShopName { get; set; }

    [Column("shop_email")]
    [MaxLength(255)]
    public string? ShopEmail { get; set; }

    [Column("shop_owner")]
    [MaxLength(255)]
    public string? ShopOwner { get; set; }

    [Column("plan_name")]
    [MaxLength(100)]
    public string? PlanName { get; set; }

    [Column("country_code")]
    [MaxLength(3)]
    public string? CountryCode { get; set; }

    [Column("currency")]
    [MaxLength(3)]
    public string? Currency { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("webhooks_configured")]
    public bool WebhooksConfigured { get; set; } = false;

    [Column("installed_at")]
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

    [Column("last_activity")]
    public DateTime? LastActivity { get; set; }

    [Column("uninstalled_at")]
    public DateTime? UninstalledAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<ShopifyWebhook> Webhooks { get; set; } = new();

    public string[] GetScopesArray() => Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim()).ToArray();
}

/// <summary>
/// Shopify webhook record for tracking
/// </summary>
[Table("shopify_webhooks")]
public class ShopifyWebhook
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("shop_id")]
    public int ShopId { get; set; }

    [Required]
    [Column("webhook_id")]
    [MaxLength(50)]
    public string WebhookId { get; set; } = string.Empty;

    [Required]
    [Column("topic")]
    [MaxLength(100)]
    public string Topic { get; set; } = string.Empty;

    [Required]
    [Column("address")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ShopId")]
    public ShopifyShop Shop { get; set; } = null!;
}

/// <summary>
/// Shopify webhook payload base
/// </summary>
public class ShopifyWebhookPayload
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Shopify order webhook payload
/// </summary>
public class ShopifyOrderWebhook : ShopifyWebhookPayload
{
    [JsonPropertyName("order_number")]
    public int OrderNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("total_price")]
    public string TotalPrice { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("financial_status")]
    public string? FinancialStatus { get; set; }

    [JsonPropertyName("fulfillment_status")]
    public string? FulfillmentStatus { get; set; }

    [JsonPropertyName("customer")]
    public ShopifyCustomer? Customer { get; set; }

    [JsonPropertyName("line_items")]
    public List<ShopifyLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Shopify product webhook payload
/// </summary>
public class ShopifyProductWebhook : ShopifyWebhookPayload
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("product_type")]
    public string ProductType { get; set; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("variants")]
    public List<ShopifyVariant> Variants { get; set; } = new();
}

/// <summary>
/// Shopify customer data
/// </summary>
public class ShopifyCustomer
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
}

/// <summary>
/// Shopify line item data
/// </summary>
public class ShopifyLineItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("variant_id")]
    public long VariantId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;
}

/// <summary>
/// Shopify product variant data
/// </summary>
public class ShopifyVariant
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("inventory_quantity")]
    public int InventoryQuantity { get; set; }
}

/// <summary>
/// Shopify OAuth token response
/// </summary>
public class ShopifyOAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

/// <summary>
/// Shopify shop info response
/// </summary>
public class ShopifyShopInfo
{
    [JsonPropertyName("shop")]
    public ShopifyShopDetails Shop { get; set; } = new();
}

public class ShopifyShopDetails
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("myshopify_domain")]
    public string MyshopifyDomain { get; set; } = string.Empty;

    [JsonPropertyName("shop_owner")]
    public string ShopOwner { get; set; } = string.Empty;

    [JsonPropertyName("plan_name")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Shopify webhook creation request
/// </summary>
public class ShopifyWebhookRequest
{
    [JsonPropertyName("webhook")]
    public ShopifyWebhookData Webhook { get; set; } = new();
}

public class ShopifyWebhookData
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";
}

/// <summary>
/// Shopify webhook response
/// </summary>
public class ShopifyWebhookResponse
{
    [JsonPropertyName("webhook")]
    public ShopifyWebhookInfo Webhook { get; set; } = new();
}

public class ShopifyWebhookInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}