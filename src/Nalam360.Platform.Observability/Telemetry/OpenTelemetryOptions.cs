namespace Nalam360.Platform.Observability.Telemetry;

/// <summary>
/// Configuration options for OpenTelemetry.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the service name for telemetry.
    /// </summary>
    public string ServiceName { get; set; } = "Nalam360.Platform";

    /// <summary>
    /// Gets or sets the service version.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the OTLP exporter endpoint.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets whether to export to console.
    /// </summary>
    public bool ExportToConsole { get; set; } = false;

    /// <summary>
    /// Gets or sets whether tracing is enabled.
    /// </summary>
    public bool TracingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether metrics are enabled.
    /// </summary>
    public bool MetricsEnabled { get; set; } = true;
}
