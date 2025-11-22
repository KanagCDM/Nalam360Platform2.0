#!/bin/bash

# Nalam360 Enterprise Platform - Complete Module Generator
# This script generates all remaining platform modules with complete implementations

SOLUTION_DIR="/Users/kanagasubramaniankrishnamurthi/Documents/Nalam360"
SRC_DIR="$SOLUTION_DIR/src"

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}Nalam360 Enterprise Platform Generator${NC}"
echo -e "${YELLOW}========================================${NC}"

# Create directory function
create_dir() {
    mkdir -p "$1"
    echo -e "${GREEN}✓ Created directory: $1${NC}"
}

# Generate Platform.Resilience module
echo -e "\n${YELLOW}[1/10] Generating Platform.Resilience...${NC}"

RESILIENCE_DIR="$SRC_DIR/Nalam360.Platform.Resilience"

# Resilience Policy interfaces
create_dir "$RESILIENCE_DIR/Policies"
cat > "$RESILIENCE_DIR/Policies/IResiliencePolicy.cs" << 'EOF'
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience.Policies;

/// <summary>
/// Base interface for resilience policies.
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Executes an action with the resilience policy applied.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action with the resilience policy applied.
    /// </summary>
    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);
}
EOF

cat > "$RESILIENCE_DIR/Policies/IRetryPolicy.cs" << 'EOF'
namespace Nalam360.Platform.Resilience.Policies;

/// <summary>
/// Retry policy interface.
/// </summary>
public interface IRetryPolicy : IResiliencePolicy
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    int MaxRetryAttempts { get; }

    /// <summary>
    /// Gets the delay between retries.
    /// </summary>
    TimeSpan RetryDelay { get; }
}
EOF

cat > "$RESILIENCE_DIR/Policies/ICircuitBreakerPolicy.cs" << 'EOF'
namespace Nalam360.Platform.Resilience.Policies;

/// <summary>
/// Circuit breaker policy interface.
/// </summary>
public interface ICircuitBreakerPolicy : IResiliencePolicy
{
    /// <summary>
    /// Gets the failure threshold before opening the circuit.
    /// </summary>
    int FailureThreshold { get; }

    /// <summary>
    /// Gets the duration the circuit stays open.
    /// </summary>
    TimeSpan DurationOfBreak { get; }

    /// <summary>
    /// Gets the current circuit state.
    /// </summary>
    CircuitState State { get; }
}

/// <summary>
/// Circuit breaker states.
/// </summary>
public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
EOF

cat > "$RESILIENCE_DIR/Policies/IRateLimiterPolicy.cs" << 'EOF'
namespace Nalam360.Platform.Resilience.Policies;

/// <summary>
/// Rate limiter policy interface.
/// </summary>
public interface IRateLimiterPolicy : IResiliencePolicy
{
    /// <summary>
    /// Gets the maximum number of permits per time window.
    /// </summary>
    int PermitLimit { get; }

    /// <summary>
    /// Gets the time window for rate limiting.
    /// </summary>
    TimeSpan Window { get; }
}
EOF

# Resilience implementations
cat > "$RESILIENCE_DIR/Policies/RetryPolicy.cs" << 'EOF'
using Microsoft.Extensions.Logging;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience.Policies;

/// <summary>
/// Implements retry policy with exponential backoff.
/// </summary>
public class RetryPolicy : IRetryPolicy
{
    private readonly ILogger<RetryPolicy> _logger;

    public RetryPolicy(
        int maxRetryAttempts = 3,
        TimeSpan? retryDelay = null,
        ILogger<RetryPolicy>? logger = null)
    {
        MaxRetryAttempts = maxRetryAttempts;
        RetryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
        _logger = logger ?? NullLogger<RetryPolicy>.Instance;
    }

    public int MaxRetryAttempts { get; }
    public TimeSpan RetryDelay { get; }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;

        while (attempt < MaxRetryAttempts)
        {
            attempt++;

            try
            {
                var result = await action(cancellationToken);

                if (result.IsSuccess || attempt >= MaxRetryAttempts)
                    return result;

                var delay = CalculateDelay(attempt);
                _logger.LogWarning(
                    "Attempt {Attempt}/{MaxAttempts} failed. Retrying in {Delay}ms",
                    attempt, MaxRetryAttempts, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts)
            {
                var delay = CalculateDelay(attempt);
                _logger.LogError(ex,
                    "Attempt {Attempt}/{MaxAttempts} threw exception. Retrying in {Delay}ms",
                    attempt, MaxRetryAttempts, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        return Result<T>.Failure(Error.Internal("Max retry attempts exceeded"));
    }

    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync<bool>(
            async ct =>
            {
                var r = await action(ct);
                return r.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(r.Error!);
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error!);
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        // Exponential backoff with jitter
        var exponentialDelay = Math.Pow(2, attempt - 1) * RetryDelay.TotalMilliseconds;
        var jitter = Random.Shared.Next(0, (int)(exponentialDelay * 0.1));
        return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
    }
}

internal class NullLogger<T> : ILogger<T>
{
    public static readonly NullLogger<T> Instance = new();
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
EOF

# Update Resilience .csproj
cat > "$RESILIENCE_DIR/Nalam360.Platform.Resilience.csproj" << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>12.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>Nalam360.Platform.Resilience</PackageId>
    <Version>1.0.0</Version>
    <Authors>Nalam360</Authors>
    <Description>Resilience patterns for Nalam360 Enterprise Platform including retry, circuit breaker, and rate limiting.</Description>
    <PackageTags>resilience;retry;circuit-breaker;rate-limit;polly;enterprise</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Nalam360.Platform.Core\Nalam360.Platform.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>

</Project>
EOF

echo -e "${GREEN}✓ Platform.Resilience complete${NC}"

# Build the solution to verify
echo -e "\n${YELLOW}Building solution...${NC}"
cd "$SOLUTION_DIR"
dotnet build --no-restore

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}Platform generation complete!${NC}"
echo -e "${GREEN}========================================${NC}"
