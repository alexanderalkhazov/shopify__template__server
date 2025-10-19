using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Repositories.Interfaces;
using API.Repositories.Implementations;
using API.Services.Interfaces;
using API.Services.Implementations;
using API.Jobs;
using API.Endpoints;
using API.Models.ThirdParty;
using StackExchange.Redis;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using System.Text.Json.Serialization;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Database Configuration
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Hangfire Configuration
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")), new PostgreSqlStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(10),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                TransactionSynchronisationTimeout = TimeSpan.FromMinutes(5),
                SchemaName = "hangfire"
            }));

        builder.Services.AddHangfireServer(options =>
        {
            options.ServerName = Environment.MachineName;
            options.WorkerCount = Math.Max(Environment.ProcessorCount, 20);
            options.Queues = new[] { "default", "critical", "background" };
        });

        // Configure Redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(connectionString!);
        });

        // Service Registration
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<IJobService, JobService>();
        builder.Services.AddScoped<IDiscordService, DiscordService>();
        builder.Services.AddScoped<SampleBackgroundJobs>();

        // Configure AutoMapper
        builder.Services.AddAutoMapper(typeof(Program));

        // Configure Discord settings
        builder.Services.Configure<DiscordSettings>(
            builder.Configuration.GetSection("Discord"));

        // Add HttpClient for Discord service
        builder.Services.AddHttpClient();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.WriteIndented = true;
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IEntityRepository, EntityRepository>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Backend Test API",
                Version = "v1",
                Description = "A comprehensive API template with EF Core, PostgreSQL, Redis Caching, and Hangfire Background Jobs"
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend Test API v1");
            });
        }

        app.UseHttpsRedirection();

        // Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new IDashboardAuthorizationFilter[] { new HangfireAuthorizationFilter() }
        });

        app.MapApiEndpoints();

        // Setup recurring jobs
        SetupRecurringJobs();

        app.Run();
    }

    private static void SetupRecurringJobs()
    {
        // Setup sample recurring jobs
        RecurringJob.AddOrUpdate<SampleBackgroundJobs>(
            "cache-cleanup",
            job => job.CleanupExpiredCacheAsync(),
            "0 */6 * * *", // Every 6 hours
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        RecurringJob.AddOrUpdate<SampleBackgroundJobs>(
            "system-health-check",
            job => job.SystemHealthCheckAsync(),
            "*/5 * * * *", // Every 5 minutes
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }
}

// Simple authorization filter for Hangfire Dashboard (development only)
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, implement proper authorization
        return true;
    }
}