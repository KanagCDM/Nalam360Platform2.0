using Microsoft.Extensions.DependencyInjection;

namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// Extension methods for adding remote feature flag provider.
/// </summary>
public static class RemoteProviderExtensions
{
    /// <summary>
    /// Adds remote feature flag provider.
    /// </summary>
    public static IServiceCollection AddRemoteFeatureFlags(
        this IServiceCollection services,
        Action<RemoteProviderOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddHttpClient<RemoteFeatureFlagProvider>();
        services.AddSingleton<IFeatureFlagProvider, RemoteFeatureFlagProvider>();
        services.AddSingleton<IFeatureFlagService, AdvancedFeatureFlagService>();

        return services;
    }
}
