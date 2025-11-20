using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience;

/// <summary>
/// Abstraction for circuit breaker pattern.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Executes an action with circuit breaker protection.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action with circuit breaker protection.
    /// </summary>
    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    CircuitState State { get; }
}

/// <summary>
/// Circuit breaker state.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// Circuit is closed, requests pass through.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open, requests are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is half-open, testing if service recovered.
    /// </summary>
    HalfOpen
}
