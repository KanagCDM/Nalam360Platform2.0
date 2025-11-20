namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// Remote feature flag provider configuration.
/// </summary>
public class RemoteProviderOptions
{
    /// <summary>
    /// Remote endpoint URL.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Refresh interval for polling remote flags.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Enable local caching.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration time.
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Environment name (e.g., "Production", "Staging").
    /// </summary>
    public string? Environment { get; set; }
}
