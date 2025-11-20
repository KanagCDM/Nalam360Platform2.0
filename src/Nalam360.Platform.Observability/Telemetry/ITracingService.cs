using System.Diagnostics;

namespace Nalam360.Platform.Observability.Telemetry;

/// <summary>
/// Service for managing distributed tracing with OpenTelemetry.
/// </summary>
public interface ITracingService
{
    /// <summary>
    /// Starts a new activity (span).
    /// </summary>
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Gets the current activity.
    /// </summary>
    Activity? CurrentActivity { get; }

    /// <summary>
    /// Adds a tag to the current activity.
    /// </summary>
    void AddTag(string key, object? value);

    /// <summary>
    /// Adds an event to the current activity.
    /// </summary>
    void AddEvent(string name, params KeyValuePair<string, object?>[] tags);

    /// <summary>
    /// Sets the status of the current activity.
    /// </summary>
    void SetStatus(ActivityStatusCode status, string? description = null);
}
