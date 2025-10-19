using System.Linq.Expressions;

namespace API.Services.Interfaces;

public interface IJobService
{
    // Fire-and-forget jobs
    string EnqueueJob(Expression<Func<Task>> methodCall);
    string EnqueueJob<T>(Expression<Func<T, Task>> methodCall);

    // Delayed jobs
    string ScheduleJob(Expression<Func<Task>> methodCall, TimeSpan delay);
    string ScheduleJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    string ScheduleJob(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);
    string ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    // Recurring jobs
    void AddOrUpdateRecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null);
    void AddOrUpdateRecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null);

    // Job management
    bool DeleteJob(string jobId);
    void RemoveRecurringJob(string jobId);

    // Job status and monitoring
    Task<object?> GetJobDetailsAsync(string jobId);
    Task<List<object>> GetRecurringJobsAsync();
}