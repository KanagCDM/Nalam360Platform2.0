using Nalam360.Platform.FeatureFlags.Models;

namespace Nalam360.Platform.FeatureFlags.Strategies;

/// <summary>
/// Time window strategy - enables feature only during specified time periods.
/// Useful for scheduled feature releases, maintenance windows, or promotional periods.
/// </summary>
public class TimeWindowStrategy : RolloutStrategy
{
    private readonly TimeWindowConfig _config;

    public override string Name => RolloutStrategyTypes.TimeWindow;

    public TimeWindowStrategy(TimeWindowConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    public override Task<bool> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Check start time
        if (_config.StartTime.HasValue && now < _config.StartTime.Value)
        {
            return Task.FromResult(false);
        }

        // Check end time
        if (_config.EndTime.HasValue && now > _config.EndTime.Value)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
