using API.Services.Interfaces;
using API.Jobs;
using Hangfire;
using Hangfire.Storage;

namespace API.Endpoints;

public static class JobEndpoints
{
    public static RouteGroupBuilder MapJobEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/enqueue/cache-warm", (IJobService jobService) =>
        {
            var jobId = jobService.EnqueueJob<SampleBackgroundJobs>(x => x.WarmCacheAsync());
            return Results.Ok(new { jobId, message = "Cache warming job enqueued", type = "fire-and-forget" });
        })
        .WithName("EnqueueCacheWarmJob")
        .WithSummary("Enqueue cache warming job")
        .WithDescription("Triggers an immediate cache warming background job")
        .WithOpenApi();

        group.MapPost("/enqueue/data-processing", (int batchSize, IJobService jobService) =>
        {
            var jobId = jobService.EnqueueJob<SampleBackgroundJobs>(x => x.ProcessDataBatchAsync(batchSize));
            return Results.Ok(new { jobId, message = $"Data processing job enqueued for {batchSize} items", type = "fire-and-forget" });
        })
        .WithName("EnqueueDataProcessingJob")
        .WithSummary("Enqueue data processing job")
        .WithDescription("Triggers a data batch processing job with specified batch size")
        .WithOpenApi();

        // Scheduled jobs
        group.MapPost("/schedule/notification", (string message, string recipient, int delayMinutes, IJobService jobService) =>
        {
            var delay = TimeSpan.FromMinutes(delayMinutes);
            var jobId = jobService.ScheduleJob<SampleBackgroundJobs>(x => x.SendDelayedNotificationAsync(message, recipient), delay);
            return Results.Ok(new
            {
                jobId,
                message = $"Notification scheduled for {recipient}",
                delayMinutes,
                scheduledFor = DateTime.UtcNow.Add(delay),
                type = "delayed"
            });
        })
        .WithName("ScheduleNotificationJob")
        .WithSummary("Schedule delayed notification")
        .WithDescription("Schedules a notification to be sent after specified delay")
        .WithOpenApi();

        group.MapPost("/schedule/cache-warm", (int delayMinutes, IJobService jobService) =>
        {
            var delay = TimeSpan.FromMinutes(delayMinutes);
            var jobId = jobService.ScheduleJob<SampleBackgroundJobs>(x => x.WarmCacheAsync(), delay);
            return Results.Ok(new
            {
                jobId,
                message = "Cache warming scheduled",
                delayMinutes,
                scheduledFor = DateTime.UtcNow.Add(delay),
                type = "delayed"
            });
        })
        .WithName("ScheduleCacheWarmJob")
        .WithSummary("Schedule delayed cache warming")
        .WithDescription("Schedules cache warming to run after specified delay")
        .WithOpenApi();

        group.MapPost("/recurring/create", (string jobId, string cronExpression, string jobType, IJobService jobService) =>
        {
            try
            {
                switch (jobType.ToLowerInvariant())
                {
                    case "cache-cleanup":
                        jobService.AddOrUpdateRecurringJob<SampleBackgroundJobs>(jobId, x => x.CleanupExpiredCacheAsync(), cronExpression);
                        break;
                    case "health-check":
                        jobService.AddOrUpdateRecurringJob<SampleBackgroundJobs>(jobId, x => x.SystemHealthCheckAsync(), cronExpression);
                        break;
                    case "cache-warm":
                        jobService.AddOrUpdateRecurringJob<SampleBackgroundJobs>(jobId, x => x.WarmCacheAsync(), cronExpression);
                        break;
                    default:
                        return Results.BadRequest(new { error = "Invalid job type. Supported types: cache-cleanup, health-check, cache-warm" });
                }

                return Results.Ok(new
                {
                    jobId,
                    cronExpression,
                    jobType,
                    message = "Recurring job created/updated successfully",
                    type = "recurring"
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateRecurringJob")
        .WithSummary("Create or update recurring job")
        .WithDescription("Creates or updates a recurring job with specified cron expression")
        .WithOpenApi();

        group.MapDelete("/recurring/{jobId}", (string jobId, IJobService jobService) =>
        {
            try
            {
                jobService.RemoveRecurringJob(jobId);
                return Results.Ok(new { jobId, message = "Recurring job removed successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("RemoveRecurringJob")
        .WithSummary("Remove recurring job")
        .WithDescription("Removes a recurring job by ID")
        .WithOpenApi();

        // Job monitoring
        group.MapGet("/{jobId}/status", async (string jobId, IJobService jobService) =>
        {
            try
            {
                var jobDetails = await jobService.GetJobDetailsAsync(jobId);
                return Results.Ok(jobDetails);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetJobStatus")
        .WithSummary("Get job status")
        .WithDescription("Retrieves status and details of a specific job")
        .WithOpenApi();

        group.MapGet("/recurring", async (IJobService jobService) =>
        {
            try
            {
                var recurringJobs = await jobService.GetRecurringJobsAsync();
                return Results.Ok(recurringJobs);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetRecurringJobs")
        .WithSummary("Get all recurring jobs")
        .WithDescription("Retrieves list of all recurring jobs")
        .WithOpenApi();

        group.MapDelete("/{jobId}", (string jobId, IJobService jobService) =>
        {
            try
            {
                var success = jobService.DeleteJob(jobId);
                if (success)
                {
                    return Results.Ok(new { jobId, message = "Job cancelled successfully" });
                }
                return Results.NotFound(new { jobId, message = "Job not found or already completed" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CancelJob")
        .WithSummary("Cancel job")
        .WithDescription("Cancels a pending or running job")
        .WithOpenApi();

        group.MapGet("/examples/cron", () =>
        {
            var examples = new
            {
                everyMinute = "* * * * *",
                every5Minutes = "*/5 * * * *",
                every30Minutes = "*/30 * * * *",
                hourly = "0 * * * *",
                every6Hours = "0 */6 * * *",
                daily = "0 0 * * *",
                weekly = "0 0 * * 0",
                monthly = "0 0 1 * *",
                description = "Common cron expressions for scheduling jobs"
            };
            return Results.Ok(examples);
        })
        .WithName("GetCronExamples")
        .WithSummary("Get cron expression examples")
        .WithDescription("Returns common cron expressions for reference")
        .WithOpenApi();

        group.MapGet("/dashboard-url", () =>
        {
            return Results.Ok(new
            {
                dashboardUrl = "/hangfire",
                description = "Access the Hangfire dashboard to monitor jobs visually",
                note = "Dashboard shows job history, recurring jobs, servers, and detailed job information"
            });
        })
        .WithName("GetHangfireDashboardUrl")
        .WithSummary("Get Hangfire dashboard URL")
        .WithDescription("Returns the URL to access the Hangfire dashboard")
        .WithOpenApi();

        group.MapGet("/status", () =>
        {
            try
            {
                var monitoring = JobStorage.Current.GetMonitoringApi();
                var stats = monitoring.GetStatistics();

                return Results.Ok(new
                {
                    isConnected = true,
                    storage = JobStorage.Current.GetType().Name,
                    statistics = new
                    {
                        enqueued = stats.Enqueued,
                        failed = stats.Failed,
                        processing = stats.Processing,
                        scheduled = stats.Scheduled,
                        succeeded = stats.Succeeded,
                        deleted = stats.Deleted,
                        recurringJobs = stats.Recurring,
                        servers = stats.Servers
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new
                {
                    isConnected = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                }.ToString());
            }
        })
        .WithName("GetHangfireStatus")
        .WithSummary("Get Hangfire connection status")
        .WithDescription("Returns Hangfire database connection status and statistics")
        .WithOpenApi();

        return group;
    }
}