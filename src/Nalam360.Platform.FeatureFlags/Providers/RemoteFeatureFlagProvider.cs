using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nalam360.Platform.FeatureFlags.Providers;

/// <summary>
/// Remote feature flag provider with polling and caching.
/// Compatible with LaunchDarkly, Unleash, ConfigCat, and custom REST APIs.
/// </summary>
public class RemoteFeatureFlagProvider : IFeatureFlagProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RemoteProviderOptions _options;
    private readonly ILogger<RemoteFeatureFlagProvider> _logger;
    private readonly ConcurrentDictionary<string, Models.FeatureFlag> _cache = new();
    private readonly Timer _refreshTimer;
    private bool _disposed;

    public RemoteFeatureFlagProvider(
        HttpClient httpClient,
        IOptions<RemoteProviderOptions> options,
        ILogger<RemoteFeatureFlagProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();

        // Start background refresh if caching is enabled
        if (_options.EnableCaching)
        {
            _refreshTimer = new Timer(
                RefreshCacheCallback,
                null,
                TimeSpan.Zero,
                _options.RefreshInterval);
        }
        else
        {
            _refreshTimer = new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite);
        }
    }

    public async Task<Models.FeatureFlag?> GetFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default)
    {
        // Try cache first
        if (_options.EnableCaching && _cache.TryGetValue(featureName, out var cachedFlag))
        {
            return cachedFlag;
        }

        // Fetch from remote
        try
        {
            var flag = await FetchFeatureFlagAsync(featureName, cancellationToken);

            if (flag != null && _options.EnableCaching)
            {
                _cache[featureName] = flag;
            }

            return flag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch feature flag '{FeatureName}' from remote provider", featureName);
            return null;
        }
    }

    public async Task<IEnumerable<Models.FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
    {
        // Try cache first
        if (_options.EnableCaching && _cache.Any())
        {
            return _cache.Values.ToList();
        }

        // Fetch from remote
        try
        {
            var flags = await FetchAllFeatureFlagsAsync(cancellationToken);

            if (_options.EnableCaching)
            {
                _cache.Clear();
                foreach (var flag in flags)
                {
                    _cache[flag.Name] = flag;
                }
            }

            return flags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch all feature flags from remote provider");
            return Array.Empty<Models.FeatureFlag>();
        }
    }

    public async Task<Models.FeatureFlag> SaveFeatureFlagAsync(Models.FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlag);

        try
        {
            var endpoint = $"{_options.Endpoint}/flags";
            var response = await _httpClient.PostAsJsonAsync(endpoint, featureFlag, cancellationToken);
            response.EnsureSuccessStatusCode();

            var savedFlag = await response.Content.ReadFromJsonAsync<Models.FeatureFlag>(cancellationToken)
                ?? throw new InvalidOperationException("Remote provider returned null response");

            // Update cache
            if (_options.EnableCaching)
            {
                _cache[savedFlag.Name] = savedFlag;
            }

            return savedFlag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save feature flag '{FeatureName}' to remote provider", featureFlag.Name);
            throw;
        }
    }

    public async Task DeleteFeatureFlagAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_options.Endpoint}/flags/{Uri.EscapeDataString(featureName)}";
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Remove from cache
            if (_options.EnableCaching)
            {
                _cache.TryRemove(featureName, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete feature flag '{FeatureName}' from remote provider", featureName);
            throw;
        }
    }

    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_options.Endpoint))
        {
            _httpClient.BaseAddress = new Uri(_options.Endpoint);
        }

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        if (!string.IsNullOrEmpty(_options.Environment))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Environment", _options.Environment);
        }
    }

    private async Task<Models.FeatureFlag?> FetchFeatureFlagAsync(string featureName, CancellationToken cancellationToken)
    {
        var endpoint = $"{_options.Endpoint}/flags/{Uri.EscapeDataString(featureName)}";
        return await _httpClient.GetFromJsonAsync<Models.FeatureFlag>(endpoint, cancellationToken);
    }

    private async Task<IEnumerable<Models.FeatureFlag>> FetchAllFeatureFlagsAsync(CancellationToken cancellationToken)
    {
        var endpoint = $"{_options.Endpoint}/flags";
        var flags = await _httpClient.GetFromJsonAsync<List<Models.FeatureFlag>>(endpoint, cancellationToken);
        return flags ?? new List<Models.FeatureFlag>();
    }

    private async void RefreshCacheCallback(object? state)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _logger.LogDebug("Refreshing feature flags cache from remote provider");
            await GetAllFeatureFlagsAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh feature flags cache");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _refreshTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
