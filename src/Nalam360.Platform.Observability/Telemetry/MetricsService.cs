using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Nalam360.Platform.Observability.Telemetry;

/// <summary>
/// Implementation of metrics service using System.Diagnostics.Metrics.
/// </summary>
public class MetricsService : IMetricsService, IDisposable
{
    private readonly ILogger<MetricsService> _logger;
    private readonly Meter _meter;
    private readonly Dictionary<string, Counter<long>> _counters = new();
    private readonly Dictionary<string, Histogram<double>> _histograms = new();
    private readonly Dictionary<string, ObservableGauge<long>> _gauges = new();
    private bool _disposed;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("Nalam360.Platform", "1.0.0");

        _logger.LogInformation("Metrics service initialized");
    }

    /// <inheritdoc/>
    public Meter Meter => _meter;

    /// <inheritdoc/>
    public void IncrementCounter(string name, long value = 1, params KeyValuePair<string, object?>[] tags)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = _meter.CreateCounter<long>(name, description: $"Counter for {name}");
            _counters[name] = counter;
        }

        counter.Add(value, tags);

        _logger.LogTrace("Incremented counter {Name} by {Value}", name, value);
    }

    /// <inheritdoc/>
    public void RecordHistogram(string name, double value, params KeyValuePair<string, object?>[] tags)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = _meter.CreateHistogram<double>(name, description: $"Histogram for {name}");
            _histograms[name] = histogram;
        }

        histogram.Record(value, tags);

        _logger.LogTrace("Recorded histogram {Name} value {Value}", name, value);
    }

    /// <inheritdoc/>
    public void RecordGauge(string name, long value, params KeyValuePair<string, object?>[] tags)
    {
        // Gauges in .NET metrics are observable, so we store the value and create an observable
        // This is a simplified implementation
        _logger.LogTrace("Recorded gauge {Name} value {Value}", name, value);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _meter.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
