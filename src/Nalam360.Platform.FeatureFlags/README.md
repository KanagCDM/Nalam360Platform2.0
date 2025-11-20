# Nalam360 Platform - Feature Flags

Enterprise-grade feature flag system with advanced rollout strategies.

## Features

- 🎯 **Multiple Rollout Strategies**
  - Percentage rollout (consistent hashing)
  - User targeting with attribute rules
  - Time window activation
  - A/B testing experiments

- 🔌 **Flexible Providers**
  - In-memory (development/testing)
  - Remote (LaunchDarkly/Unleash/ConfigCat compatible)
  - Database-backed (custom implementation)

- 🚀 **Production-Ready**
  - Background cache refresh
  - Configurable polling intervals
  - Failure handling with fallbacks
  - Environment-specific configuration

## Installation

```bash
dotnet add package Nalam360.Platform.FeatureFlags
```

## Quick Start

### 1. Basic Setup (In-Memory)

```csharp
using Nalam360.Platform.FeatureFlags;

// Register services
builder.Services.AddFeatureFlags();

// Use in controllers/services
public class MyService
{
    private readonly IFeatureFlagService _featureFlags;

    public MyService(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public async Task<IActionResult> GetData()
    {
        if (await _featureFlags.IsEnabledAsync("new-ui"))
        {
            return GetNewUI();
        }
        return GetLegacyUI();
    }
}
```

### 2. Remote Provider (LaunchDarkly/Unleash)

```csharp
using Nalam360.Platform.FeatureFlags.Providers;

builder.Services.AddRemoteFeatureFlags(options =>
{
    options.Endpoint = "https://api.launchdarkly.com/v1";
    options.ApiKey = "your-api-key";
    options.Environment = "Production";
    options.RefreshInterval = TimeSpan.FromSeconds(30);
    options.EnableCaching = true;
});
```

### 3. Context-Based Evaluation

```csharp
// Create feature context
var context = new FeatureContext(
    UserId: "user-123",
    TenantId: "tenant-456",
    Properties: new Dictionary<string, object>
    {
        ["Role"] = "Admin",
        ["Region"] = "US",
        ["AccountAge"] = 365
    }
);

// Check feature with context
bool isEnabled = await _featureFlags.IsEnabledAsync("premium-features", context);
```

## Rollout Strategies

### Percentage Rollout

Gradually roll out features to a percentage of users:

```csharp
var flag = new FeatureFlag
{
    Name = "new-checkout",
    IsEnabled = true,
    StrategyType = RolloutStrategyTypes.Percentage,
    StrategyConfig = JsonSerializer.Serialize(new PercentageRolloutConfig(
        Percentage: 25, // 25% of users
        StickyAttribute: "UserId" // Consistent per user
    ))
};

await provider.SaveFeatureFlagAsync(flag);
```

### User Targeting

Target specific users or segments:

```csharp
var flag = new FeatureFlag
{
    Name = "beta-features",
    IsEnabled = true,
    StrategyType = RolloutStrategyTypes.UserTargeting,
    StrategyConfig = JsonSerializer.Serialize(new UserTargetingConfig(
        IncludeUserIds: new List<string> { "user-1", "user-2" },
        Rules: new List<TargetingRule>
        {
            new("Role", "Equals", "Admin"),
            new("Region", "In", new[] { "US", "CA" }),
            new("AccountAge", "GreaterThan", 30)
        }
    ))
};
```

### Time Window

Activate features during specific time periods:

```csharp
var flag = new FeatureFlag
{
    Name = "black-friday-sale",
    IsEnabled = true,
    StrategyType = RolloutStrategyTypes.TimeWindow,
    StrategyConfig = JsonSerializer.Serialize(new TimeWindowConfig(
        StartTime: new DateTimeOffset(2024, 11, 24, 0, 0, 0, TimeSpan.Zero),
        EndTime: new DateTimeOffset(2024, 11, 27, 23, 59, 59, TimeSpan.Zero)
    ))
};
```

### A/B Testing

Run experiments with multiple variants:

```csharp
var flag = new FeatureFlag
{
    Name = "homepage-experiment",
    IsEnabled = true,
    StrategyType = RolloutStrategyTypes.ABTest,
    StrategyConfig = JsonSerializer.Serialize(new ABTestConfig(
        ExperimentId: "homepage-v2",
        Variants: new Dictionary<string, int>
        {
            ["control"] = 50,  // 50% control group
            ["variant-a"] = 25, // 25% variant A
            ["variant-b"] = 25  // 25% variant B
        }
    ))
};

// Get assigned variant
var strategy = new ABTestStrategy(config);
var variant = strategy.GetVariant(userId);
```

## Advanced Usage

### Custom Provider

Implement database-backed provider:

```csharp
public class DatabaseFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IDbContext _db;

    public async Task<FeatureFlag?> GetFeatureFlagAsync(
        string featureName,
        CancellationToken ct = default)
    {
        return await _db.FeatureFlags
            .FirstOrDefaultAsync(f => f.Name == featureName, ct);
    }

    // Implement other methods...
}

// Register
builder.Services.AddFeatureFlags<DatabaseFeatureFlagProvider>();
```

### Combining Strategies

Use multiple strategies with priority:

```csharp
// 1. Check explicit user targeting first
// 2. If not targeted, check percentage rollout
// 3. If not in rollout, check time window

public async Task<bool> IsEnabledWithFallback(string featureName, FeatureContext context)
{
    var flag = await _provider.GetFeatureFlagAsync(featureName);
    
    if (flag == null || !flag.IsEnabled)
        return false;

    // Try user targeting first
    if (flag.StrategyType == RolloutStrategyTypes.UserTargeting)
    {
        var targeting = new UserTargetingStrategy(config);
        if (await targeting.EvaluateAsync(context))
            return true;
    }

    // Fallback to percentage
    if (flag.StrategyType == RolloutStrategyTypes.Percentage)
    {
        var percentage = new PercentageRolloutStrategy(config);
        return await percentage.EvaluateAsync(context);
    }

    return false;
}
```

### Telemetry Integration

Track feature flag evaluations:

```csharp
public class TelemetryFeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagService _inner;
    private readonly ITelemetryClient _telemetry;

    public async Task<bool> IsEnabledAsync(
        string featureName,
        FeatureContext context,
        CancellationToken ct = default)
    {
        var result = await _inner.IsEnabledAsync(featureName, context, ct);

        _telemetry.TrackEvent("FeatureFlagEvaluated", new
        {
            FeatureName = featureName,
            UserId = context.UserId,
            Result = result,
            Timestamp = DateTimeOffset.UtcNow
        });

        return result;
    }
}
```

## Best Practices

### 1. Use Descriptive Names

```csharp
// ✅ Good
"enable-dark-mode"
"checkout-v2-rollout"
"premium-features-beta"

// ❌ Bad
"flag1"
"test"
"temp"
```

### 2. Set Expiration Dates

```csharp
var flag = new FeatureFlag
{
    Name = "holiday-promo",
    Tags = new List<string> { "temporary", "marketing" },
    Environment = "Production",
    // Add comment or metadata about removal date
};
```

### 3. Use Consistent Sticky Attributes

```csharp
// Always use same attribute for percentage rollouts
new PercentageRolloutConfig(
    Percentage: 50,
    StickyAttribute: "UserId" // Consistent across all features
);
```

### 4. Monitor Feature Flag Usage

```csharp
// Log feature flag evaluations
_logger.LogInformation(
    "Feature {FeatureName} evaluated to {Result} for user {UserId}",
    featureName,
    result,
    context.UserId
);
```

### 5. Clean Up Stale Flags

```csharp
// Regular audit process
var allFlags = await _provider.GetAllFeatureFlagsAsync();
var staleFlags = allFlags.Where(f => 
    f.UpdatedAt < DateTimeOffset.UtcNow.AddMonths(-6) &&
    f.IsEnabled == false
);

foreach (var flag in staleFlags)
{
    await _provider.DeleteFeatureFlagAsync(flag.Name);
}
```

## Configuration Examples

### ASP.NET Core Middleware

```csharp
app.Use(async (context, next) =>
{
    var featureFlags = context.RequestServices.GetRequiredService<IFeatureFlagService>();
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var featureContext = new FeatureContext(
        UserId: userId,
        Properties: new Dictionary<string, object>
        {
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["IpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? ""
        }
    );

    context.Items["FeatureContext"] = featureContext;
    await next();
});
```

### Razor Pages

```razor
@inject IFeatureFlagService FeatureFlags

@if (await FeatureFlags.IsEnabledAsync("show-banner", ViewContext.GetFeatureContext()))
{
    <div class="promo-banner">Limited Time Offer!</div>
}
```

### Blazor Components

```razor
@inject IFeatureFlagService FeatureFlags

@code {
    private bool _showNewFeature;

    protected override async Task OnInitializedAsync()
    {
        var context = new FeatureContext(UserId: CurrentUser.Id);
        _showNewFeature = await FeatureFlags.IsEnabledAsync("new-feature", context);
    }
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public async Task PercentageRollout_ConsistentForSameUser()
{
    var config = new PercentageRolloutConfig(50, "UserId");
    var strategy = new PercentageRolloutStrategy(config);

    var context = new FeatureContext(UserId: "test-user");

    var result1 = await strategy.EvaluateAsync(context);
    var result2 = await strategy.EvaluateAsync(context);

    Assert.Equal(result1, result2); // Same user always gets same result
}

[Fact]
public async Task UserTargeting_IncludeList_ReturnsTrue()
{
    var config = new UserTargetingConfig(
        IncludeUserIds: new List<string> { "user-1", "user-2" }
    );
    var strategy = new UserTargetingStrategy(config);

    var context = new FeatureContext(UserId: "user-1");
    var result = await strategy.EvaluateAsync(context);

    Assert.True(result);
}
```

### Integration Tests

```csharp
[Fact]
public async Task RemoteProvider_FetchesFromApi()
{
    var handler = new MockHttpMessageHandler();
    var httpClient = new HttpClient(handler);
    
    var options = Options.Create(new RemoteProviderOptions
    {
        Endpoint = "https://api.example.com",
        ApiKey = "test-key"
    });

    var provider = new RemoteFeatureFlagProvider(httpClient, options, logger);
    var flag = await provider.GetFeatureFlagAsync("test-feature");

    Assert.NotNull(flag);
}
```

## Performance Considerations

- **Caching**: Remote provider caches flags for 5 minutes by default
- **Polling**: Background refresh every 1 minute (configurable)
- **Hashing**: SHA256 for consistent user bucketing (deterministic)
- **Memory**: In-memory provider uses `ConcurrentDictionary` (thread-safe)

## Migration Guide

### From Simple Flags to Strategies

```csharp
// Before: Simple boolean flag
bool isEnabled = config.GetValue<bool>("FeatureFlags:NewUI");

// After: Strategy-based with context
var context = new FeatureContext(UserId: userId);
bool isEnabled = await _featureFlags.IsEnabledAsync("new-ui", context);
```

### From LaunchDarkly SDK

```csharp
// Before: LaunchDarkly
var user = User.WithKey(userId);
bool isEnabled = ldClient.BoolVariation("new-ui", user, false);

// After: Nalam360 Feature Flags
var context = new FeatureContext(UserId: userId);
bool isEnabled = await _featureFlags.IsEnabledAsync("new-ui", context);
```

## License

MIT License - See LICENSE file for details
