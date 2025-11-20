namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// Feature flag provider interface.
/// Abstracts the source of feature flag configurations.
/// </summary>
public interface IFeatureFlagProvider
{
    /// <summary>
    /// Gets a feature flag by name.
    /// </summary>
    Task<Models.FeatureFlag?> GetFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    Task<IEnumerable<Models.FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a feature flag.
    /// </summary>
    Task<Models.FeatureFlag> SaveFeatureFlagAsync(Models.FeatureFlag featureFlag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a feature flag.
    /// </summary>
    Task DeleteFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default);
}
