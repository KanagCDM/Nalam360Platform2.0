namespace Nalam360.Platform.FeatureFlags;

/// <summary>
/// Feature flag service interface.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a feature is enabled for a specific context.
    /// </summary>
    Task<bool> IsEnabledAsync(
        string featureName,
        FeatureContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Feature context for targeted feature toggles.
/// </summary>
public record FeatureContext(
    string? UserId = null,
    string? TenantId = null,
    Dictionary<string, object>? Properties = null);
