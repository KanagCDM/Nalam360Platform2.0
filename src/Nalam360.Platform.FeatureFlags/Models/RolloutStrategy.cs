namespace Nalam360.Platform.FeatureFlags.Models;

/// <summary>
/// Base class for rollout strategies.
/// </summary>
public abstract class RolloutStrategy
{
    /// <summary>
    /// Strategy name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Evaluates whether the feature should be enabled for the given context.
    /// </summary>
    public abstract Task<bool> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Percentage rollout strategy configuration.
/// </summary>
public record PercentageRolloutConfig(
    int Percentage, // 0-100
    string? StickyAttribute = null); // Optional attribute for consistent hashing (e.g., "UserId")

/// <summary>
/// User targeting strategy configuration.
/// </summary>
public record UserTargetingConfig(
    List<string>? IncludeUserIds = null,
    List<string>? ExcludeUserIds = null,
    List<TargetingRule>? Rules = null);

/// <summary>
/// Targeting rule for user attributes.
/// </summary>
public record TargetingRule(
    string Attribute,
    string Operator, // "Equals", "Contains", "StartsWith", "In", "GreaterThan", etc.
    object Value);

/// <summary>
/// Time window strategy configuration.
/// </summary>
public record TimeWindowConfig(
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null);

/// <summary>
/// A/B test configuration.
/// </summary>
public record ABTestConfig(
    string ExperimentId,
    Dictionary<string, int> Variants); // Variant name -> percentage allocation
