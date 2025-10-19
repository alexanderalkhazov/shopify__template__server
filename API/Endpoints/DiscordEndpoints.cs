using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;

namespace API.Endpoints;

/// <summary>
/// Discord webhook testing endpoints
/// </summary>
public static class DiscordEndpoints
{
    public static void MapDiscordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discord")
            .WithTags("Discord")
            .WithOpenApi();

        group.MapPost("/test", async (IDiscordService discordService, [FromBody] TestMessageRequest request) =>
        {
            try
            {
                var success = await discordService.SendMessageAsync(request.Message);
                if (success)
                {
                    return Results.Ok(new { Success = true, Message = "Discord message sent successfully" });
                }
                else
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to send Discord message" });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending Discord message: {ex.Message}");
            }
        })
        .WithName("TestDiscordMessage")
        .WithSummary("Send a test message to Discord");

        group.MapPost("/test-embed", async (IDiscordService discordService, [FromBody] TestEmbedRequest request) =>
        {
            try
            {
                var success = await discordService.SendEmbedAsync(
                    request.Title,
                    request.Description,
                    request.Color ?? "3447003"
                );

                if (success)
                {
                    return Results.Ok(new { Success = true, Message = "Discord embed sent successfully" });
                }
                else
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to send Discord embed" });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending Discord embed: {ex.Message}");
            }
        })
        .WithName("TestDiscordEmbed")
        .WithSummary("Send a test embed to Discord");

        group.MapPost("/test-notification", async (IDiscordService discordService, [FromBody] TestNotificationRequest request) =>
        {
            try
            {
                var success = await discordService.SendNotificationAsync(
                    request.Title,
                    request.Message,
                    request.Fields
                );

                if (success)
                {
                    return Results.Ok(new { Success = true, Message = "Discord notification sent successfully" });
                }
                else
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to send Discord notification" });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending Discord notification: {ex.Message}");
            }
        })
        .WithName("TestDiscordNotification")
        .WithSummary("Send a test notification to Discord");

        group.MapPost("/test-error", async (IDiscordService discordService) =>
        {
            try
            {
                var success = await discordService.SendErrorNotificationAsync(
                    "Test error notification",
                    "This is a test error from the API to verify Discord integration is working"
                );

                if (success)
                {
                    return Results.Ok(new { Success = true, Message = "Discord error notification sent successfully" });
                }
                else
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to send Discord error notification" });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending Discord error notification: {ex.Message}");
            }
        })
        .WithName("TestDiscordError")
        .WithSummary("Send a test error notification to Discord");

        group.MapPost("/test-success", async (IDiscordService discordService) =>
        {
            try
            {
                var success = await discordService.SendSuccessNotificationAsync(
                    "Test success notification",
                    "This is a test success message from the API"
                );

                if (success)
                {
                    return Results.Ok(new { Success = true, Message = "Discord success notification sent successfully" });
                }
                else
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to send Discord success notification" });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending Discord success notification: {ex.Message}");
            }
        })
        .WithName("TestDiscordSuccess")
        .WithSummary("Send a test success notification to Discord");
    }
}

/// <summary>
/// Request model for testing Discord messages
/// </summary>
public record TestMessageRequest(string Message);

/// <summary>
/// Request model for testing Discord embeds
/// </summary>
public record TestEmbedRequest(string Title, string Description, string? Color = null);

/// <summary>
/// Request model for testing Discord notifications
/// </summary>
public record TestNotificationRequest(string Title, string Message, Dictionary<string, string>? Fields = null);