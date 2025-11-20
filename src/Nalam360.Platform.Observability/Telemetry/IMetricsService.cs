using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Nalam360.Platform.Observability.Telemetry;

/// <summary>
/// Service for managing application metrics.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Records a counter increment.
    /// </summary>
    void IncrementCounter(string name, long value = 1, params KeyValuePair<string, object?>[] tags);

    /// <summary>
    /// Records a histogram value.
    /// </summary>
    void RecordHistogram(string name, double value, params KeyValuePair<string, object?>[] tags);

    /// <summary>
    /// Records a gauge value.
    /// </summary>
    void RecordGauge(string name, long value, params KeyValuePair<string, object?>[] tags);

    /// <summary>
    /// Gets the meter instance.
    /// </summary>
    Meter Meter { get; }
}
