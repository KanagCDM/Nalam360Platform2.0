namespace Nalam360.Platform.FeatureFlags.Models;

/// <summary>
/// Represents a feature flag configuration.
/// </summary>
public class FeatureFlag
{
    /// <summary>
    /// Unique identifier for the feature flag.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Feature name (unique key).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the feature is enabled globally.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Rollout strategy configuration (JSON).
    /// </summary>
    public string? StrategyConfig { get; set; }

    /// <summary>
    /// Strategy type (e.g., "Percentage", "UserTargeting", "TimeWindow").
    /// </summary>
    public string? StrategyType { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Last modified timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Created by user.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Last modified by user.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Environment (e.g., "Development", "Staging", "Production").
    /// </summary>
    public string? Environment { get; set; }
}

/// <summary>
/// Rollout strategy types.
/// </summary>
public static class RolloutStrategyTypes
{
    public const string Percentage = "Percentage";
    public const string UserTargeting = "UserTargeting";
    public const string TimeWindow = "TimeWindow";
    public const string ABTest = "ABTest";
    public const string Custom = "Custom";
}
