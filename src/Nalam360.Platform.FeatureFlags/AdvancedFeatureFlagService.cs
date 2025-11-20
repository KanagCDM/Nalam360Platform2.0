using System.Text.Json;
using Nalam360.Platform.FeatureFlags.Models;
using Nalam360.Platform.FeatureFlags.Strategies;

namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// Advanced feature flag service with rollout strategies.
/// </summary>
public class AdvancedFeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagProvider _provider;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdvancedFeatureFlagService(IFeatureFlagProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return await IsEnabledAsync(featureName, new FeatureContext(), cancellationToken);
    }

    public async Task<bool> IsEnabledAsync(
        string featureName,
        FeatureContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);
        ArgumentNullException.ThrowIfNull(context);

        var featureFlag = await _provider.GetFeatureFlagAsync(featureName, cancellationToken);

        if (featureFlag == null)
        {
            // Feature flag doesn't exist - default to disabled
            return false;
        }

        // Check global enable flag first
        if (!featureFlag.IsEnabled)
        {
            return false;
        }

        // If no strategy is configured, use global flag
        if (string.IsNullOrEmpty(featureFlag.StrategyType) || string.IsNullOrEmpty(featureFlag.StrategyConfig))
        {
            return true;
        }

        // Create and evaluate strategy
        var strategy = CreateStrategy(featureFlag.StrategyType, featureFlag.StrategyConfig);
        
        if (strategy == null)
        {
            // Unknown strategy - default to global flag
            return featureFlag.IsEnabled;
        }

        return await strategy.EvaluateAsync(context, cancellationToken);
    }

    private RolloutStrategy? CreateStrategy(string strategyType, string strategyConfig)
    {
        try
        {
            return strategyType switch
            {
                RolloutStrategyTypes.Percentage => CreatePercentageStrategy(strategyConfig),
                RolloutStrategyTypes.UserTargeting => CreateUserTargetingStrategy(strategyConfig),
                RolloutStrategyTypes.TimeWindow => CreateTimeWindowStrategy(strategyConfig),
                RolloutStrategyTypes.ABTest => CreateABTestStrategy(strategyConfig),
                _ => null
            };
        }
        catch
        {
            // Invalid configuration - return null to use global flag
            return null;
        }
    }

    private PercentageRolloutStrategy CreatePercentageStrategy(string config)
    {
        var strategyConfig = JsonSerializer.Deserialize<PercentageRolloutConfig>(config, _jsonOptions)
            ?? throw new InvalidOperationException("Invalid percentage rollout configuration");

        return new PercentageRolloutStrategy(strategyConfig);
    }

    private UserTargetingStrategy CreateUserTargetingStrategy(string config)
    {
        var strategyConfig = JsonSerializer.Deserialize<UserTargetingConfig>(config, _jsonOptions)
            ?? throw new InvalidOperationException("Invalid user targeting configuration");

        return new UserTargetingStrategy(strategyConfig);
    }

    private TimeWindowStrategy CreateTimeWindowStrategy(string config)
    {
        var strategyConfig = JsonSerializer.Deserialize<TimeWindowConfig>(config, _jsonOptions)
            ?? throw new InvalidOperationException("Invalid time window configuration");

        return new TimeWindowStrategy(strategyConfig);
    }

    private ABTestStrategy CreateABTestStrategy(string config)
    {
        var strategyConfig = JsonSerializer.Deserialize<ABTestConfig>(config, _jsonOptions)
            ?? throw new InvalidOperationException("Invalid A/B test configuration");

        return new ABTestStrategy(strategyConfig);
    }
}
