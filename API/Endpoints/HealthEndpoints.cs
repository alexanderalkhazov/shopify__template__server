using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Services.Interfaces;

namespace API.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ApplicationDbContext dbContext, ICacheService cacheService) =>
        {
            try
            {
                var dbConnected = await dbContext.Database.CanConnectAsync();

                var redisConnected = false;
                try
                {
                    await cacheService.SetAsync("health_check", "test", TimeSpan.FromSeconds(5));
                    var testValue = await cacheService.GetAsync<string>("health_check");
                    redisConnected = testValue == "test";
                    await cacheService.RemoveAsync("health_check");
                }
                catch
                {
                    redisConnected = false;
                }

                return Results.Ok(new
                {
                    status = dbConnected && redisConnected ? "Healthy" : "Degraded",
                    database = dbConnected ? "Connected" : "Disconnected",
                    redis = redisConnected ? "Connected" : "Disconnected",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new
                {
                    status = "Unhealthy",
                    database = "Unknown",
                    redis = "Unknown",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                }.ToString());
            }
        })
        .WithName("HealthCheck")
        .WithSummary("Health check endpoint")
        .WithDescription("Checks if the API, database, and Redis connections are healthy")
        .WithOpenApi();

        group.MapGet("/redis", async (ICacheService cacheService) =>
        {
            try
            {
                await cacheService.SetAsync("redis_health_check", "test", TimeSpan.FromSeconds(5));
                var testValue = await cacheService.GetAsync<string>("redis_health_check");
                var isConnected = testValue == "test";
                await cacheService.RemoveAsync("redis_health_check");

                return Results.Ok(new
                {
                    status = isConnected ? "Healthy" : "Unhealthy",
                    redis = isConnected ? "Connected" : "Disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new
                {
                    status = "Unhealthy",
                    redis = "Disconnected",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                }.ToString());
            }
        })
        .WithName("RedisHealthCheck")
        .WithSummary("Redis health check endpoint")
        .WithDescription("Checks if Redis connection is healthy")
        .WithOpenApi();

        return group;
    }
}