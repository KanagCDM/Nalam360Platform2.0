using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Resilience.Polly;

namespace Nalam360.Platform.Resilience.DependencyInjection;

/// <summary>
/// Extension methods for registering resilience services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds retry policy to the service collection.
    /// </summary>
    public static IServiceCollection AddRetryPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RetryOptions>(
            configuration.GetSection("Resilience:Retry"));

        services.AddScoped<IRetryPolicy, PollyRetryPolicy>();

        return services;
    }

    /// <summary>
    /// Adds retry policy to the service collection with options.
    /// </summary>
    public static IServiceCollection AddRetryPolicy(
        this IServiceCollection services,
        Action<RetryOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<IRetryPolicy, PollyRetryPolicy>();

        return services;
    }

    /// <summary>
    /// Adds circuit breaker to the service collection.
    /// </summary>
    public static IServiceCollection AddCircuitBreaker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CircuitBreakerOptions>(
            configuration.GetSection("Resilience:CircuitBreaker"));

        services.AddSingleton<ICircuitBreaker, PollyCircuitBreaker>();

        return services;
    }

    /// <summary>
    /// Adds circuit breaker to the service collection with options.
    /// </summary>
    public static IServiceCollection AddCircuitBreaker(
        this IServiceCollection services,
        Action<CircuitBreakerOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<ICircuitBreaker, PollyCircuitBreaker>();

        return services;
    }

    /// <summary>
    /// Adds rate limiter to the service collection.
    /// </summary>
    public static IServiceCollection AddRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimiterOptions>(
            configuration.GetSection("Resilience:RateLimiter"));

        services.AddSingleton<IRateLimiter, PollyRateLimiter>();

        return services;
    }

    /// <summary>
    /// Adds rate limiter to the service collection with options.
    /// </summary>
    public static IServiceCollection AddRateLimiter(
        this IServiceCollection services,
        Action<RateLimiterOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IRateLimiter, PollyRateLimiter>();

        return services;
    }

    /// <summary>
    /// Adds bulkhead to the service collection.
    /// </summary>
    public static IServiceCollection AddBulkhead(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BulkheadOptions>(
            configuration.GetSection("Resilience:Bulkhead"));

        services.AddSingleton<IBulkhead, PollyBulkhead>();

        return services;
    }

    /// <summary>
    /// Adds bulkhead to the service collection with options.
    /// </summary>
    public static IServiceCollection AddBulkhead(
        this IServiceCollection services,
        Action<BulkheadOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IBulkhead, PollyBulkhead>();

        return services;
    }

    /// <summary>
    /// Adds all resilience patterns to the service collection.
    /// </summary>
    public static IServiceCollection AddResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRetryPolicy(configuration);
        services.AddCircuitBreaker(configuration);
        services.AddRateLimiter(configuration);
        services.AddBulkhead(configuration);

        return services;
    }
}
