using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Nalam360.Platform.Caching;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds in-memory caching services.
    /// </summary>
    public static IServiceCollection AddMemoryCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheService, MemoryCacheService>();
        return services;
    }

    /// <summary>
    /// Adds Redis distributed caching services.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        Action<RedisCacheOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = new RedisCacheOptions();
            configureOptions(options);

            var configuration = ConfigurationOptions.Parse(options.ConnectionString);
            configuration.ConnectTimeout = options.ConnectTimeout;
            configuration.SyncTimeout = options.SyncTimeout;
            configuration.AbortOnConnectFail = options.AbortOnConnectFail;

            if (!string.IsNullOrEmpty(options.Password))
            {
                configuration.Password = options.Password;
            }

            return ConnectionMultiplexer.Connect(configuration);
        });

        services.TryAddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>
    /// Adds Redis distributed caching services with connection string.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddRedisCaching(options =>
        {
            options.ConnectionString = connectionString;
        });
    }
}
