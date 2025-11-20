using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Nalam360.Platform.Observability.Telemetry;

/// <summary>
/// Implementation of tracing service using System.Diagnostics.Activity.
/// </summary>
public class TracingService : ITracingService
{
    private readonly ILogger<TracingService> _logger;
    private readonly ActivitySource _activitySource;

    public TracingService(ILogger<TracingService> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("Nalam360.Platform", "1.0.0");

        _logger.LogInformation("Tracing service initialized");
    }

    /// <inheritdoc/>
    public Activity? CurrentActivity => Activity.Current;

    /// <inheritdoc/>
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = _activitySource.StartActivity(name, kind);

        if (activity != null)
        {
            _logger.LogTrace("Started activity: {ActivityName} ({TraceId})", name, activity.TraceId);
        }

        return activity;
    }

    /// <inheritdoc/>
    public void AddTag(string key, object? value)
    {
        CurrentActivity?.SetTag(key, value);
        _logger.LogTrace("Added tag {Key}={Value} to current activity", key, value);
    }

    /// <inheritdoc/>
    public void AddEvent(string name, params KeyValuePair<string, object?>[] tags)
    {
        if (CurrentActivity != null)
        {
            var activityEvent = new ActivityEvent(name, tags: new ActivityTagsCollection(tags));
            CurrentActivity.AddEvent(activityEvent);
            
            _logger.LogTrace("Added event {EventName} to current activity", name);
        }
    }

    /// <inheritdoc/>
    public void SetStatus(ActivityStatusCode status, string? description = null)
    {
        if (CurrentActivity != null)
        {
            CurrentActivity.SetStatus(status, description);
            _logger.LogTrace("Set activity status to {Status}", status);
        }
    }
}
