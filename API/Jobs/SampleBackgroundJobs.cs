using API.Services.Interfaces;
using Hangfire;

namespace API.Jobs;

public class SampleBackgroundJobs
{
    private readonly ICacheService _cacheService;
    private readonly IDiscordService _discordService;
    private readonly ILogger<SampleBackgroundJobs> _logger;

    public SampleBackgroundJobs(
        ICacheService cacheService,
        IDiscordService discordService,
        ILogger<SampleBackgroundJobs> logger)
    {
        _cacheService = cacheService;
        _discordService = discordService;
        _logger = logger;
    }

    /// <summary>
    /// Fire-and-forget job example: Cache warming
    /// </summary>
    public async Task WarmCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting cache warming job at {Timestamp}", DateTime.UtcNow);

            // Simulate cache warming operations
            await _cacheService.SetAsync("cache:warmed", true, TimeSpan.FromHours(1));
            await _cacheService.SetAsync("cache:warm_timestamp", DateTime.UtcNow, TimeSpan.FromHours(1));

            // Simulate some work
            await Task.Delay(1000);

            _logger.LogInformation("Cache warming completed successfully at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system health check");
            throw;
        }
    }

    /// <summary>
    /// Send Discord notification job example
    /// </summary>
    [Queue("default")]
    public async Task SendDiscordNotificationAsync(string title, string message, string type = "info")
    {
        try
        {
            _logger.LogInformation("Sending Discord notification: {Title}", title);

            bool success = type.ToLower() switch
            {
                "error" => await _discordService.SendErrorNotificationAsync(title, message),
                "success" => await _discordService.SendSuccessNotificationAsync(title, message),
                _ => await _discordService.SendNotificationAsync(title, message)
            };

            if (success)
            {
                _logger.LogInformation("Discord notification sent successfully: {Title}", title);
            }
            else
            {
                _logger.LogWarning("Failed to send Discord notification: {Title}", title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord notification: {Title}", title);
            throw;
        }
    }

    /// <summary>
    /// System monitoring job that sends Discord alerts
    /// </summary>
    [Queue("critical")]
    public async Task SystemMonitoringWithDiscordAsync()
    {
        try
        {
            _logger.LogInformation("Running system monitoring with Discord alerts");

            // Check cache health - simple test
            var cacheHealthy = true;
            try
            {
                await _cacheService.SetAsync("health_test", DateTime.UtcNow, TimeSpan.FromMinutes(1));
                var testValue = await _cacheService.GetAsync<DateTime>("health_test");
                cacheHealthy = testValue != default;
            }
            catch
            {
                cacheHealthy = false;
            }

            // Check memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            var memoryInMB = memoryUsage / (1024 * 1024);

            // Alert if memory usage is high (> 500MB for demo)
            if (memoryInMB > 500)
            {
                await _discordService.SendErrorNotificationAsync(
                    "High Memory Usage Alert",
                    $"Application memory usage is {memoryInMB} MB"
                );
            }

            // Alert if cache is unhealthy
            if (!cacheHealthy)
            {
                await _discordService.SendErrorNotificationAsync(
                    "Cache Health Alert",
                    "Redis cache is not responding properly"
                );
            }

            // Send daily health summary
            var fields = new Dictionary<string, string>
            {
                { "Cache Status", cacheHealthy ? "✅ Healthy" : "❌ Unhealthy" },
                { "Memory Usage", $"{memoryInMB} MB" },
                { "Uptime", TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss") }
            };

            await _discordService.SendNotificationAsync(
                "System Health Report",
                "Regular system health monitoring report",
                fields
            );

            _logger.LogInformation("System monitoring with Discord completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system monitoring");
            await _discordService.SendErrorNotificationAsync(
                "System Monitoring Failed",
                $"An error occurred during system monitoring: {ex.Message}"
            );
            throw;
        }
    }

    /// <summary>
    /// Recurring job example: Cleanup expired cache entries
    /// </summary>
    public async Task CleanupExpiredCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting cache cleanup job at {Timestamp}", DateTime.UtcNow);

            // Simulate cleanup operations
            await _cacheService.RemoveByPatternAsync("temp:*");
            await _cacheService.SetAsync("cleanup:last_run", DateTime.UtcNow, TimeSpan.FromDays(1));

            _logger.LogInformation("Cache cleanup completed at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache cleanup job failed");
            throw;
        }
    }

    /// <summary>
    /// Delayed job example: Send notification after delay
    /// </summary>
    public async Task SendDelayedNotificationAsync(string message, string recipient)
    {
        try
        {
            _logger.LogInformation("Sending delayed notification to {Recipient}: {Message}", recipient, message);

            // Simulate notification sending
            await Task.Delay(500);

            // Cache the notification record
            var notificationKey = $"notification:{recipient}:{DateTime.UtcNow:yyyyMMddHHmmss}";
            await _cacheService.SetAsync(notificationKey, new { message, recipient, sentAt = DateTime.UtcNow }, TimeSpan.FromDays(7));

            _logger.LogInformation("Delayed notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send delayed notification");
            throw;
        }
    }

    /// <summary>
    /// Data processing job example
    /// </summary>
    public async Task ProcessDataBatchAsync(int batchSize = 100)
    {
        try
        {
            _logger.LogInformation("Starting data batch processing for {BatchSize} items", batchSize);

            // Simulate data processing
            for (int i = 0; i < batchSize; i++)
            {
                if (i % 10 == 0)
                {
                    await Task.Delay(50); // Simulate processing delay
                    _logger.LogDebug("Processed {Count}/{Total} items", i, batchSize);
                }
            }

            // Cache processing result
            var resultKey = $"batch_processing:{DateTime.UtcNow:yyyyMMddHHmmss}";
            await _cacheService.SetAsync(resultKey, new
            {
                batchSize,
                processedAt = DateTime.UtcNow,
                status = "completed"
            }, TimeSpan.FromHours(6));

            _logger.LogInformation("Data batch processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data batch processing failed");
            throw;
        }
    }

    /// <summary>
    /// Health check job for system monitoring
    /// </summary>
    public async Task SystemHealthCheckAsync()
    {
        try
        {
            _logger.LogInformation("Running system health check");

            // Check cache service
            var cacheHealthy = await _cacheService.ExistsAsync("health_check");

            // Simulate additional health checks
            await Task.Delay(200);

            var healthReport = new
            {
                timestamp = DateTime.UtcNow,
                cacheService = cacheHealthy ? "healthy" : "degraded",
                systemMemory = GC.GetTotalMemory(false),
                uptime = Environment.TickCount64
            };

            await _cacheService.SetAsync("system:health_report", healthReport, TimeSpan.FromMinutes(5));

            _logger.LogInformation("System health check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System health check failed");
            throw;
        }
    }
}