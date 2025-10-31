using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models.Shopify;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations;

/// <summary>
/// Repository for managing Shopify data
/// </summary>
public class ShopifyRepository : IShopifyRepository
{
    private readonly ApplicationDbContext _context;

    public ShopifyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Shop management
    public async Task<ShopifyShop?> GetShopByDomainAsync(string shopDomain)
    {
        return await _context.ShopifyShops
            .Include(s => s.Webhooks)
            .FirstOrDefaultAsync(s => s.ShopDomain == shopDomain);
    }

    public async Task<ShopifyShop?> GetShopByIdAsync(int shopId)
    {
        return await _context.ShopifyShops
            .Include(s => s.Webhooks)
            .FirstOrDefaultAsync(s => s.Id == shopId);
    }

    public async Task<List<ShopifyShop>> GetActiveShopsAsync()
    {
        return await _context.ShopifyShops
            .Where(s => s.IsActive)
            .OrderBy(s => s.ShopDomain)
            .ToListAsync();
    }

    public async Task<List<ShopifyShop>> GetAllShopsAsync()
    {
        return await _context.ShopifyShops
            .OrderBy(s => s.ShopDomain)
            .ToListAsync();
    }

    public async Task AddShopAsync(ShopifyShop shop)
    {
        await _context.ShopifyShops.AddAsync(shop);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateShopAsync(ShopifyShop shop)
    {
        shop.UpdatedAt = DateTime.UtcNow;
        _context.ShopifyShops.Update(shop);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteShopAsync(int shopId)
    {
        var shop = await GetShopByIdAsync(shopId);
        if (shop != null)
        {
            // Delete associated webhooks first
            await DeleteWebhooksByShopIdAsync(shopId);
            
            _context.ShopifyShops.Remove(shop);
            await _context.SaveChangesAsync();
        }
    }

    // Webhook management
    public async Task<List<ShopifyWebhook>> GetWebhooksByShopIdAsync(int shopId)
    {
        return await _context.ShopifyWebhooks
            .Where(w => w.ShopId == shopId)
            .OrderBy(w => w.Topic)
            .ToListAsync();
    }

    public async Task<ShopifyWebhook?> GetWebhookByIdAsync(int webhookId)
    {
        return await _context.ShopifyWebhooks
            .Include(w => w.Shop)
            .FirstOrDefaultAsync(w => w.Id == webhookId);
    }

    public async Task AddWebhookAsync(ShopifyWebhook webhook)
    {
        await _context.ShopifyWebhooks.AddAsync(webhook);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateWebhookAsync(ShopifyWebhook webhook)
    {
        webhook.UpdatedAt = DateTime.UtcNow;
        _context.ShopifyWebhooks.Update(webhook);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWebhookAsync(int webhookId)
    {
        var webhook = await GetWebhookByIdAsync(webhookId);
        if (webhook != null)
        {
            _context.ShopifyWebhooks.Remove(webhook);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteWebhooksByShopIdAsync(int shopId)
    {
        var webhooks = await GetWebhooksByShopIdAsync(shopId);
        if (webhooks.Any())
        {
            _context.ShopifyWebhooks.RemoveRange(webhooks);
            await _context.SaveChangesAsync();
        }
    }

    // Analytics and reporting
    public async Task<int> GetActiveShopCountAsync()
    {
        return await _context.ShopifyShops
            .CountAsync(s => s.IsActive);
    }

    public async Task<List<ShopifyShop>> GetRecentInstallationsAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.ShopifyShops
            .Where(s => s.InstalledAt >= cutoffDate)
            .OrderByDescending(s => s.InstalledAt)
            .ToListAsync();
    }

    public async Task<List<ShopifyShop>> GetShopsWithoutWebhooksAsync()
    {
        return await _context.ShopifyShops
            .Where(s => s.IsActive && !s.WebhooksConfigured)
            .OrderBy(s => s.ShopDomain)
            .ToListAsync();
    }
}