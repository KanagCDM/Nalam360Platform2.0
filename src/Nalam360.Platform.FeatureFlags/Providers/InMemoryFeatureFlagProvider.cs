using System.Collections.Concurrent;

namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// In-memory feature flag provider.
/// Useful for testing and development.
/// </summary>
public class InMemoryFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly ConcurrentDictionary<string, Models.FeatureFlag> _flags = new();

    public Task<Models.FeatureFlag?> GetFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default)
    {
        _flags.TryGetValue(featureName, out var flag);
        return Task.FromResult(flag);
    }

    public Task<IEnumerable<Models.FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Models.FeatureFlag>>(_flags.Values.ToList());
    }

    public Task<Models.FeatureFlag> SaveFeatureFlagAsync(Models.FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlag);

        if (featureFlag.Id == Guid.Empty)
        {
            featureFlag.Id = Guid.NewGuid();
            featureFlag.CreatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            featureFlag.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _flags[featureFlag.Name] = featureFlag;
        return Task.FromResult(featureFlag);
    }

    public Task DeleteFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default)
    {
        _flags.TryRemove(featureName, out _);
        return Task.CompletedTask;
    }
}
