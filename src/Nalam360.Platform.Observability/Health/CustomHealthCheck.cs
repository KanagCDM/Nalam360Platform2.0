using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nalam360.Platform.Observability.Health;

/// <summary>
/// Custom health check base class.
/// </summary>
public abstract class CustomHealthCheck : IHealthCheck
{
    public abstract Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default);
}
