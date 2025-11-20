using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.FeatureFlags.Providers;

namespace Nalam360.Platform.FeatureFlags.DependencyInjection;

/// <summary>
/// Dependency injection extensions for feature flags.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds feature flag services with in-memory provider.
    /// </summary>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services)
    {
        return services.AddFeatureFlags<InMemoryFeatureFlagProvider>();
    }

    /// <summary>
    /// Adds feature flag services with custom provider.
    /// </summary>
    public static IServiceCollection AddFeatureFlags<TProvider>(this IServiceCollection services)
        where TProvider : class, IFeatureFlagProvider
    {
        services.AddSingleton<IFeatureFlagProvider, TProvider>();
        services.AddSingleton<IFeatureFlagService, AdvancedFeatureFlagService>();

        return services;
    }

    /// <summary>
    /// Adds feature flag services with custom provider instance.
    /// </summary>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IFeatureFlagProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        services.AddSingleton(provider);
        services.AddSingleton<IFeatureFlagService, AdvancedFeatureFlagService>();

        return services;
    }
}
