using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Nalam360.Platform.Caching;

/// <summary>
/// Redis-based distributed cache implementation.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisCacheOptions _options;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        IOptions<RedisCacheOptions> options)
    {
        _redis = redis;
        _database = redis.GetDatabase(_options.Database);
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var value = await _database.StringGetAsync(prefixedKey);

            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var serialized = JsonSerializer.Serialize(value);
            var ttl = expiration ?? _options.DefaultExpiration;

            await _database.StringSetAsync(prefixedKey, serialized, ttl);
            _logger.LogDebug("Cache set for key: {Key}, TTL: {TTL}", key, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached != null)
            return cached;

        var value = await factory(ct);
        await SetAsync(key, value, expiration, ct);
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            await _database.KeyDeleteAsync(prefixedKey);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        try
        {
            var prefixedPattern = GetPrefixedKey(pattern);
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: prefixedPattern).ToArray();

            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("Cache removed for pattern: {Pattern}, Count: {Count}", pattern, keys.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            return await _database.KeyExistsAsync(prefixedKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    private string GetPrefixedKey(string key)
    {
        return string.IsNullOrEmpty(_options.KeyPrefix)
            ? key
            : $"{_options.KeyPrefix}:{key}";
    }
}

/// <summary>
/// Configuration options for Redis cache.
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Redis database number (0-15).
    /// </summary>
    public int Database { get; set; } = 0;

    /// <summary>
    /// Key prefix for all cache entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "nalam360";

    /// <summary>
    /// Default expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Connection timeout in milliseconds.
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Sync timeout in milliseconds.
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Enable connection retry.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
}
