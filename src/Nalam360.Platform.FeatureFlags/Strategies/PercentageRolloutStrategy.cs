using System.Security.Cryptography;
using System.Text;
using Nalam360.Platform.FeatureFlags.Models;

namespace Nalam360.Platform.FeatureFlags.Strategies;

/// <summary>
/// Percentage rollout strategy - enables feature for a specified percentage of users.
/// Uses consistent hashing to ensure same user always gets same result.
/// </summary>
public class PercentageRolloutStrategy : RolloutStrategy
{
    private readonly PercentageRolloutConfig _config;

    public override string Name => RolloutStrategyTypes.Percentage;

    public PercentageRolloutStrategy(PercentageRolloutConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        if (config.Percentage is < 0 or > 100)
        {
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(config));
        }

        _config = config;
    }

    public override Task<bool> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        // If 0%, always disabled
        if (_config.Percentage == 0)
        {
            return Task.FromResult(false);
        }

        // If 100%, always enabled
        if (_config.Percentage == 100)
        {
            return Task.FromResult(true);
        }

        // Determine sticky attribute value (for consistent hashing)
        var stickyValue = GetStickyValue(context);
        
        if (string.IsNullOrEmpty(stickyValue))
        {
            // No sticky attribute - fallback to random (non-sticky behavior)
            var randomPercentage = Random.Shared.Next(0, 100);
            return Task.FromResult(randomPercentage < _config.Percentage);
        }

        // Use consistent hashing to determine if user is in rollout percentage
        var hash = ComputeHash(stickyValue);
        var bucket = hash % 100; // Map to 0-99 bucket

        return Task.FromResult(bucket < _config.Percentage);
    }

    private string? GetStickyValue(FeatureContext context)
    {
        if (string.IsNullOrEmpty(_config.StickyAttribute))
        {
            // Default to UserId if available
            return context.UserId;
        }

        // Try to get value from context properties
        if (context.Properties?.TryGetValue(_config.StickyAttribute, out var value) == true)
        {
            return value?.ToString();
        }

        // Try standard context properties
        return _config.StickyAttribute switch
        {
            "UserId" => context.UserId,
            "TenantId" => context.TenantId,
            _ => null
        };
    }

    private static int ComputeHash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = SHA256.HashData(bytes);
        
        // Use first 4 bytes as integer
        return Math.Abs(BitConverter.ToInt32(hashBytes, 0));
    }
}
