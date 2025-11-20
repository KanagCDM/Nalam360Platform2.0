using Nalam360.Platform.FeatureFlags.Models;

namespace Nalam360.Platform.FeatureFlags.Strategies;

/// <summary>
/// User targeting strategy - enables feature for specific users or user segments.
/// </summary>
public class UserTargetingStrategy : RolloutStrategy
{
    private readonly UserTargetingConfig _config;

    public override string Name => RolloutStrategyTypes.UserTargeting;

    public UserTargetingStrategy(UserTargetingConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    public override Task<bool> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        var userId = context.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(false);
        }

        // Check explicit exclusions first
        if (_config.ExcludeUserIds?.Contains(userId) == true)
        {
            return Task.FromResult(false);
        }

        // Check explicit inclusions
        if (_config.IncludeUserIds?.Contains(userId) == true)
        {
            return Task.FromResult(true);
        }

        // Evaluate targeting rules
        if (_config.Rules != null)
        {
            foreach (var rule in _config.Rules)
            {
                if (EvaluateRule(rule, context))
                {
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
    }

    private static bool EvaluateRule(TargetingRule rule, FeatureContext context)
    {
        // Get attribute value from context
        var attributeValue = GetAttributeValue(rule.Attribute, context);

        if (attributeValue == null)
        {
            return false;
        }

        return rule.Operator switch
        {
            "Equals" => attributeValue.Equals(rule.Value),
            "NotEquals" => !attributeValue.Equals(rule.Value),
            "Contains" => attributeValue.ToString()?.Contains(rule.Value.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "StartsWith" => attributeValue.ToString()?.StartsWith(rule.Value.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "EndsWith" => attributeValue.ToString()?.EndsWith(rule.Value.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "In" => rule.Value is IEnumerable<object> list && list.Contains(attributeValue),
            "NotIn" => rule.Value is IEnumerable<object> list2 && !list2.Contains(attributeValue),
            "GreaterThan" => CompareNumeric(attributeValue, rule.Value) > 0,
            "GreaterThanOrEqual" => CompareNumeric(attributeValue, rule.Value) >= 0,
            "LessThan" => CompareNumeric(attributeValue, rule.Value) < 0,
            "LessThanOrEqual" => CompareNumeric(attributeValue, rule.Value) <= 0,
            _ => false
        };
    }

    private static object? GetAttributeValue(string attribute, FeatureContext context)
    {
        return attribute switch
        {
            "UserId" => context.UserId,
            "TenantId" => context.TenantId,
            _ => context.Properties?.TryGetValue(attribute, out var value) == true ? value : null
        };
    }

    private static int CompareNumeric(object left, object right)
    {
        if (left is IComparable leftComp && right is IComparable rightComp)
        {
            try
            {
                return leftComp.CompareTo(rightComp);
            }
            catch
            {
                return 0;
            }
        }

        return 0;
    }
}
