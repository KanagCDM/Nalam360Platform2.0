# Feature Flags Quick Reference

## 🚀 Quick Start (30 seconds)

### 1. Install & Register
```csharp
// Program.cs
builder.Services.AddFeatureFlags(); // In-memory for dev
```

### 2. Inject & Use
```csharp
public class MyService
{
    private readonly IFeatureFlagService _flags;
    
    public async Task DoWork()
    {
        if (await _flags.IsEnabledAsync("new-feature"))
        {
            // New code path
        }
    }
}
```

## 📦 Installation

```bash
dotnet add package Nalam360.Platform.FeatureFlags
```

## 🔧 Provider Setup

### Development (In-Memory)
```csharp
services.AddFeatureFlags();
```

### Production (Remote)
```csharp
services.AddRemoteFeatureFlags(options =>
{
    options.Endpoint = "https://api.launchdarkly.com/v1";
    options.ApiKey = Configuration["FeatureFlags:ApiKey"];
    options.RefreshInterval = TimeSpan.FromSeconds(30);
});
```

### Custom (Database)
```csharp
services.AddFeatureFlags<YourDatabaseProvider>();
```

## 🎯 Strategy Cheat Sheet

### Simple On/Off
```csharp
var flag = new FeatureFlag
{
    Name = "dark-mode",
    IsEnabled = true  // Global flag
};
```

### Percentage Rollout (25% of users)
```csharp
var flag = new FeatureFlag
{
    Name = "new-checkout",
    IsEnabled = true,
    StrategyType = "Percentage",
    StrategyConfig = JsonSerializer.Serialize(new PercentageRolloutConfig(
        Percentage: 25,
        StickyAttribute: "UserId"  // Consistent per user
    ))
};
```

### Target Specific Users
```csharp
StrategyConfig = JsonSerializer.Serialize(new UserTargetingConfig(
    IncludeUserIds: new List<string> { "admin-1", "admin-2" }
));
```

### Target by Attributes
```csharp
StrategyConfig = JsonSerializer.Serialize(new UserTargetingConfig(
    Rules: new List<TargetingRule>
    {
        new("Role", "Equals", "Admin"),
        new("Country", "In", new[] { "US", "CA", "UK" }),
        new("AccountAge", "GreaterThan", 30)
    }
));
```

### Time-Limited Feature
```csharp
StrategyConfig = JsonSerializer.Serialize(new TimeWindowConfig(
    StartTime: DateTimeOffset.Parse("2024-12-01T00:00:00Z"),
    EndTime: DateTimeOffset.Parse("2024-12-31T23:59:59Z")
));
```

### A/B Test (3 variants)
```csharp
StrategyConfig = JsonSerializer.Serialize(new ABTestConfig(
    ExperimentId: "homepage-redesign",
    Variants: new Dictionary<string, int>
    {
        ["control"] = 33,
        ["variant-a"] = 33,
        ["variant-b"] = 34
    }
));
```

## 🎨 Usage Patterns

### Basic Check
```csharp
bool isEnabled = await _flags.IsEnabledAsync("feature-name");
```

### With User Context
```csharp
var context = new FeatureContext(
    UserId: currentUser.Id,
    TenantId: currentUser.TenantId
);

bool isEnabled = await _flags.IsEnabledAsync("feature-name", context);
```

### With Custom Properties
```csharp
var context = new FeatureContext(
    UserId: userId,
    Properties: new Dictionary<string, object>
    {
        ["Role"] = "Premium",
        ["Region"] = "EU",
        ["SignupDate"] = DateTime.Parse("2024-01-01")
    }
);
```

### ASP.NET Core Middleware
```csharp
app.Use(async (context, next) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var featureContext = new FeatureContext(UserId: userId);
    context.Items["FeatureContext"] = featureContext;
    await next();
});
```

### Razor Pages
```razor
@inject IFeatureFlagService FeatureFlags

@if (await FeatureFlags.IsEnabledAsync("promo-banner"))
{
    <div class="banner">Special Offer!</div>
}
```

### Blazor Components
```razor
@inject IFeatureFlagService FeatureFlags

@code {
    private bool _newFeatureEnabled;
    
    protected override async Task OnInitializedAsync()
    {
        var context = new FeatureContext(UserId: User.Id);
        _newFeatureEnabled = await FeatureFlags.IsEnabledAsync("new-ui", context);
    }
}
```

## 🧪 Testing

### Unit Test
```csharp
[Fact]
public async Task FeatureFlag_ReturnsTrue_WhenEnabled()
{
    var provider = new InMemoryFeatureFlagProvider();
    await provider.SaveFeatureFlagAsync(new FeatureFlag
    {
        Name = "test-feature",
        IsEnabled = true
    });
    
    var service = new AdvancedFeatureFlagService(provider);
    var result = await service.IsEnabledAsync("test-feature");
    
    Assert.True(result);
}
```

### Mock Provider
```csharp
var mockProvider = new Mock<IFeatureFlagProvider>();
mockProvider
    .Setup(p => p.GetFeatureFlagAsync("test", default))
    .ReturnsAsync(new FeatureFlag { Name = "test", IsEnabled = true });
```

## 🔍 Targeting Operators

| Operator | Example | Description |
|----------|---------|-------------|
| `Equals` | `("Country", "Equals", "US")` | Exact match |
| `NotEquals` | `("Status", "NotEquals", "Banned")` | Not equal |
| `Contains` | `("Email", "Contains", "@company.com")` | Substring |
| `StartsWith` | `("UserId", "StartsWith", "admin_")` | Prefix match |
| `EndsWith` | `("Domain", "EndsWith", ".gov")` | Suffix match |
| `In` | `("Region", "In", ["US", "CA"])` | In list |
| `NotIn` | `("Plan", "NotIn", ["Free", "Trial"])` | Not in list |
| `GreaterThan` | `("Age", "GreaterThan", 18)` | Numeric > |
| `GreaterThanOrEqual` | `("Credits", "GreaterThanOrEqual", 100)` | Numeric >= |
| `LessThan` | `("LoginCount", "LessThan", 5)` | Numeric < |
| `LessThanOrEqual` | `("FailedAttempts", "LessThanOrEqual", 3)` | Numeric <= |

## 📊 Best Practices

### ✅ DO
- Use descriptive flag names: `enable-dark-mode`, `checkout-v2-rollout`
- Set expiration dates for temporary flags
- Use consistent sticky attributes (UserId)
- Monitor flag evaluations with telemetry
- Remove stale flags regularly
- Document flag purpose and owner

### ❌ DON'T
- Don't use generic names: `flag1`, `test`, `temp`
- Don't leave flags in code forever (technical debt)
- Don't use flags for secrets/config (use Key Vault)
- Don't forget to test both flag states
- Don't create flags without cleanup plan

## 🔐 Security Notes

- **API Keys**: Store in Azure Key Vault or User Secrets
- **Remote Endpoints**: Use HTTPS only
- **Context Properties**: Don't include sensitive data
- **Caching**: Consider data sensitivity in cache duration

## 🚦 Common Patterns

### Gradual Rollout
```csharp
// Week 1: 10%
config.Percentage = 10;

// Week 2: 25%  
config.Percentage = 25;

// Week 3: 50%
config.Percentage = 50;

// Week 4: 100% (full rollout)
config.Percentage = 100;

// Week 5: Remove flag from code
```

### Beta Testing
```csharp
new UserTargetingConfig(
    IncludeUserIds: betaTesterIds,
    Rules: new List<TargetingRule>
    {
        new("Role", "Equals", "BetaTester")
    }
)
```

### Regional Rollout
```csharp
new UserTargetingConfig(
    Rules: new List<TargetingRule>
    {
        new("Country", "In", new[] { "US", "CA" })
    }
)
```

### Premium Features
```csharp
new UserTargetingConfig(
    Rules: new List<TargetingRule>
    {
        new("SubscriptionTier", "In", new[] { "Premium", "Enterprise" })
    }
)
```

## 📈 Performance

- **Cache Hit**: < 1ms
- **Remote Fetch**: ~100-200ms (first request)
- **Strategy Eval**: < 5ms
- **Memory**: ~1KB per flag
- **Polling**: 1 minute default

## 🔗 Integration Examples

### LaunchDarkly
```csharp
services.AddRemoteFeatureFlags(options =>
{
    options.Endpoint = "https://app.launchdarkly.com/sdk/latest-all";
    options.ApiKey = ldSdkKey;
});
```

### Unleash
```csharp
services.AddRemoteFeatureFlags(options =>
{
    options.Endpoint = "https://unleash.herokuapp.com/api";
    options.ApiKey = unleashKey;
});
```

### ConfigCat
```csharp
services.AddRemoteFeatureFlags(options =>
{
    options.Endpoint = "https://api.configcat.com";
    options.ApiKey = configCatSdkKey;
});
```

## 📚 Resources

- **Full Documentation**: `src/Nalam360.Platform.FeatureFlags/README.md`
- **Examples**: `examples/Nalam360.Platform.Example.Api`
- **Tests**: `tests/Nalam360.Platform.Tests/FeatureFlags/`

## 🆘 Troubleshooting

### Flag Not Found
```csharp
// Returns false by default
var result = await _flags.IsEnabledAsync("non-existent"); // false
```

### Remote Provider Not Refreshing
```csharp
// Check refresh interval
options.RefreshInterval = TimeSpan.FromSeconds(30);
options.EnableCaching = true; // Must be enabled
```

### Strategy Not Working
```csharp
// Verify JSON serialization
var json = JsonSerializer.Serialize(config);
flag.StrategyConfig = json; // Must be valid JSON
```

### Percentage Not Consistent
```csharp
// Use sticky attribute
new PercentageRolloutConfig(
    Percentage: 50,
    StickyAttribute: "UserId" // Required for consistency
)
```

---

**Need Help?** Check the full README or example application!
