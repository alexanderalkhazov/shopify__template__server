using System.Linq.Expressions;
using API.Services.Interfaces;
using Hangfire;
using Hangfire.Storage;

namespace API.Services.Implementations;

public class JobService : IJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public JobService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    // Fire-and-forget jobs
    public string EnqueueJob(Expression<Func<Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue(methodCall);
    }

    public string EnqueueJob<T>(Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue<T>(methodCall);
    }

    // Delayed jobs
    public string ScheduleJob(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClient.Schedule(methodCall, delay);
    }

    public string ScheduleJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClient.Schedule<T>(methodCall, delay);
    }

    public string ScheduleJob(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClient.Schedule(methodCall, enqueueAt);
    }

    public string ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClient.Schedule<T>(methodCall, enqueueAt);
    }

    // Recurring jobs
    public void AddOrUpdateRecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null)
    {
        var options = new RecurringJobOptions
        {
            TimeZone = timeZone ?? TimeZoneInfo.Utc
        };
        _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression, options);
    }

    public void AddOrUpdateRecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null)
    {
        var options = new RecurringJobOptions
        {
            TimeZone = timeZone ?? TimeZoneInfo.Utc
        };
        _recurringJobManager.AddOrUpdate<T>(jobId, methodCall, cronExpression, options);
    }

    // Job management
    public bool DeleteJob(string jobId)
    {
        return _backgroundJobClient.Delete(jobId);
    }

    public void RemoveRecurringJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
    }

    // Job status and monitoring (properly implemented)
    public async Task<object?> GetJobDetailsAsync(string jobId)
    {
        await Task.CompletedTask;

        using var connection = JobStorage.Current.GetConnection();
        var jobData = connection.GetJobData(jobId);

        if (jobData == null)
        {
            return null;
        }

        return new
        {
            jobId,
            job = jobData.Job?.ToString(),
            state = jobData.State,
            createdAt = jobData.CreatedAt
        };
    }

    public async Task<List<object>> GetRecurringJobsAsync()
    {
        await Task.CompletedTask;

        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        return recurringJobs.Select(job => new
        {
            jobId = job.Id,
            cronExpression = job.Cron,
            nextExecution = job.NextExecution,
            lastExecution = job.LastExecution,
            queue = job.Queue,
            job = job.Job?.ToString(),
            createdAt = job.CreatedAt,
            timeZoneId = job.TimeZoneId,
            error = job.Error
        }).Cast<object>().ToList();
    }
}