namespace Nalam360.Platform.Observability;

/// <summary>
/// Distributed tracing context interface.
/// </summary>
public interface ITraceContext
{
    /// <summary>
    /// Gets the trace identifier.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets the span identifier.
    /// </summary>
    string SpanId { get; }

    /// <summary>
    /// Starts a new span.
    /// </summary>
    IDisposable StartSpan(string operationName);

    /// <summary>
    /// Adds a tag to the current span.
    /// </summary>
    void AddTag(string key, string value);

    /// <summary>
    /// Records an exception.
    /// </summary>
    void RecordException(Exception exception);
}
