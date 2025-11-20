using Microsoft.Extensions.Caching.Memory;

namespace Nalam360.Platform.Caching;

/// <summary>
/// In-memory cache implementation.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // MemoryCache doesn't support pattern-based removal efficiently
        // This would require tracking keys separately
        throw new NotSupportedException("Pattern-based removal not supported for MemoryCache");
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}
