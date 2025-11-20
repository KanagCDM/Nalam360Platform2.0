using System.Security.Cryptography;
using System.Text;
using Nalam360.Platform.FeatureFlags.Models;

namespace Nalam360.Platform.FeatureFlags.Strategies;

/// <summary>
/// A/B testing strategy - assigns users to experiment variants based on configured percentages.
/// Uses consistent hashing to ensure same user always gets same variant.
/// </summary>
public class ABTestStrategy : RolloutStrategy
{
    private readonly ABTestConfig _config;

    public override string Name => RolloutStrategyTypes.ABTest;

    public ABTestStrategy(ABTestConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.Variants == null || config.Variants.Count == 0)
        {
            throw new ArgumentException("At least one variant must be specified", nameof(config));
        }

        var totalPercentage = config.Variants.Values.Sum();
        if (totalPercentage != 100)
        {
            throw new ArgumentException($"Variant percentages must sum to 100 (currently: {totalPercentage})", nameof(config));
        }

        _config = config;
    }

    public override Task<bool> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        var userId = context.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            // No user context - cannot assign to variant
            return Task.FromResult(false);
        }

        // Get assigned variant
        var variant = GetVariant(userId);

        // Store variant in context for telemetry/tracking
        if (context.Properties != null)
        {
            context.Properties[$"__variant_{_config.ExperimentId}"] = variant ?? string.Empty;
        }

        // Return true if assigned to any variant (actual variant can be retrieved from context)
        return Task.FromResult(!string.IsNullOrEmpty(variant));
    }

    /// <summary>
    /// Gets the variant assigned to the user.
    /// </summary>
    public string? GetVariant(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        // Use consistent hashing with experiment ID to ensure different experiments
        // assign same user to potentially different variants
        var hashInput = $"{_config.ExperimentId}:{userId}";
        var hash = ComputeHash(hashInput);
        var bucket = hash % 100; // Map to 0-99 bucket

        // Assign variant based on bucket
        var cumulativePercentage = 0;
        foreach (var (variantName, percentage) in _config.Variants.OrderBy(v => v.Key))
        {
            cumulativePercentage += percentage;
            if (bucket < cumulativePercentage)
            {
                return variantName;
            }
        }

        // Fallback to first variant (should never reach here if percentages sum to 100)
        return _config.Variants.Keys.First();
    }

    private static int ComputeHash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = SHA256.HashData(bytes);

        // Use first 4 bytes as integer
        return Math.Abs(BitConverter.ToInt32(hashBytes, 0));
    }
}
