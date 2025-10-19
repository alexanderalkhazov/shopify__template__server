namespace API.Models.ThirdParty;

/// <summary>
/// Discord webhook configuration settings
/// </summary>
public class DiscordSettings
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string BotName { get; set; } = "Backend API";
    public string AvatarUrl { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Discord webhook message payload
/// </summary>
public class DiscordWebhookMessage
{
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public List<DiscordEmbed> Embeds { get; set; } = new();
}

/// <summary>
/// Discord embed for rich messages
/// </summary>
public class DiscordEmbed
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "3447003"; // Blue color as default
    public List<DiscordField> Fields { get; set; } = new();
    public DiscordFooter? Footer { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
}

/// <summary>
/// Discord embed field
/// </summary>
public class DiscordField
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Inline { get; set; } = false;
}

/// <summary>
/// Discord embed footer
/// </summary>
public class DiscordFooter
{
    public string Text { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
}