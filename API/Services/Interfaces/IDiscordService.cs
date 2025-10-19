using API.Models.ThirdParty;

namespace API.Services.Interfaces;

/// <summary>
/// Service for sending Discord webhook notifications
/// </summary>
public interface IDiscordService
{
    Task<bool> SendMessageAsync(string message);
    Task<bool> SendEmbedAsync(string title, string description, string color = "3447003");
    Task<bool> SendNotificationAsync(string title, string message, Dictionary<string, string>? fields = null);
    Task<bool> SendErrorNotificationAsync(string error, string? details = null);
    Task<bool> SendSuccessNotificationAsync(string message, string? details = null);
}