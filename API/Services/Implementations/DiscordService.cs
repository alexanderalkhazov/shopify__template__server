using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using API.Services.Interfaces;
using API.Models.ThirdParty;

namespace API.Services.Implementations;

/// <summary>
/// Service for sending Discord webhook notifications
/// </summary>
public class DiscordService : IDiscordService
{
    private readonly HttpClient _httpClient;
    private readonly DiscordSettings _settings;
    private readonly ILogger<DiscordService> _logger;

    public DiscordService(HttpClient httpClient, IOptions<DiscordSettings> settings, ILogger<DiscordService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendMessageAsync(string message)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord notifications are disabled or webhook URL is not configured");
            return false;
        }

        try
        {
            var payload = new DiscordWebhookMessage
            {
                Content = message,
                Username = _settings.BotName,
                AvatarUrl = _settings.AvatarUrl
            };

            return await SendWebhookAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord message: {Message}", message);
            return false;
        }
    }

    public async Task<bool> SendEmbedAsync(string title, string description, string color = "3447003")
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord notifications are disabled or webhook URL is not configured");
            return false;
        }

        try
        {
            var embed = new DiscordEmbed
            {
                Title = title,
                Description = description,
                Color = color,
                Footer = new DiscordFooter
                {
                    Text = "Backend API",
                    IconUrl = _settings.AvatarUrl
                }
            };

            var payload = new DiscordWebhookMessage
            {
                Username = _settings.BotName,
                AvatarUrl = _settings.AvatarUrl,
                Embeds = new List<DiscordEmbed> { embed }
            };

            return await SendWebhookAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord embed: {Title}", title);
            return false;
        }
    }

    public async Task<bool> SendNotificationAsync(string title, string message, Dictionary<string, string>? fields = null)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord notifications are disabled or webhook URL is not configured");
            return false;
        }

        try
        {
            var embed = new DiscordEmbed
            {
                Title = title,
                Description = message,
                Color = "3447003", // Blue
                Footer = new DiscordFooter
                {
                    Text = "Backend API",
                    IconUrl = _settings.AvatarUrl
                }
            };

            if (fields != null)
            {
                embed.Fields = fields.Select(f => new DiscordField
                {
                    Name = f.Key,
                    Value = f.Value,
                    Inline = true
                }).ToList();
            }

            var payload = new DiscordWebhookMessage
            {
                Username = _settings.BotName,
                AvatarUrl = _settings.AvatarUrl,
                Embeds = new List<DiscordEmbed> { embed }
            };

            return await SendWebhookAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord notification: {Title}", title);
            return false;
        }
    }

    public async Task<bool> SendErrorNotificationAsync(string error, string? details = null)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord notifications are disabled or webhook URL is not configured");
            return false;
        }

        try
        {
            var embed = new DiscordEmbed
            {
                Title = "🚨 Error Occurred",
                Description = error,
                Color = "15158332", // Red
                Footer = new DiscordFooter
                {
                    Text = "Backend API Error",
                    IconUrl = _settings.AvatarUrl
                }
            };

            if (!string.IsNullOrEmpty(details))
            {
                embed.Fields.Add(new DiscordField
                {
                    Name = "Details",
                    Value = details.Length > 1024 ? details.Substring(0, 1021) + "..." : details,
                    Inline = false
                });
            }

            var payload = new DiscordWebhookMessage
            {
                Username = _settings.BotName,
                AvatarUrl = _settings.AvatarUrl,
                Embeds = new List<DiscordEmbed> { embed }
            };

            return await SendWebhookAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord error notification: {Error}", error);
            return false;
        }
    }

    public async Task<bool> SendSuccessNotificationAsync(string message, string? details = null)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord notifications are disabled or webhook URL is not configured");
            return false;
        }

        try
        {
            var embed = new DiscordEmbed
            {
                Title = "✅ Success",
                Description = message,
                Color = "3066993", // Green
                Footer = new DiscordFooter
                {
                    Text = "Backend API Success",
                    IconUrl = _settings.AvatarUrl
                }
            };

            if (!string.IsNullOrEmpty(details))
            {
                embed.Fields.Add(new DiscordField
                {
                    Name = "Details",
                    Value = details.Length > 1024 ? details.Substring(0, 1021) + "..." : details,
                    Inline = false
                });
            }

            var payload = new DiscordWebhookMessage
            {
                Username = _settings.BotName,
                AvatarUrl = _settings.AvatarUrl,
                Embeds = new List<DiscordEmbed> { embed }
            };

            return await SendWebhookAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord success notification: {Message}", message);
            return false;
        }
    }

    private async Task<bool> SendWebhookAsync(DiscordWebhookMessage payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_settings.WebhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Discord webhook sent successfully");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Discord webhook failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending Discord webhook");
            return false;
        }
    }
}