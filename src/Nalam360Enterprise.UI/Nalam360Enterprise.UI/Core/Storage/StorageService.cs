using Microsoft.JSInterop;

namespace Nalam360Enterprise.UI.Core.Storage;

/// <summary>
/// Service for interacting with browser storage (LocalStorage and SessionStorage)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Gets an item from local storage
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="key">The storage key</param>
    /// <returns>The stored value or default</returns>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>
    /// Sets an item in local storage
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="key">The storage key</param>
    /// <param name="value">The value to store</param>
    Task SetItemAsync<T>(string key, T value);

    /// <summary>
    /// Removes an item from local storage
    /// </summary>
    /// <param name="key">The storage key</param>
    Task RemoveItemAsync(string key);

    /// <summary>
    /// Clears all items from local storage
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets an item from session storage
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="key">The storage key</param>
    /// <returns>The stored value or default</returns>
    Task<T?> GetSessionItemAsync<T>(string key);

    /// <summary>
    /// Sets an item in session storage
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="key">The storage key</param>
    /// <param name="value">The value to store</param>
    Task SetSessionItemAsync<T>(string key, T value);

    /// <summary>
    /// Removes an item from session storage
    /// </summary>
    /// <param name="key">The storage key</param>
    Task RemoveSessionItemAsync(string key);

    /// <summary>
    /// Clears all items from session storage
    /// </summary>
    Task ClearSessionAsync();
}

/// <summary>
/// Default implementation of IStorageService using JavaScript interop
/// </summary>
public class StorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("Nalam360Interop.localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.localStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.localStorage.clear");
    }

    public async Task<T?> GetSessionItemAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("Nalam360Interop.sessionStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetSessionItemAsync<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.sessionStorage.setItem", key, json);
    }

    public async Task RemoveSessionItemAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }

        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.sessionStorage.removeItem", key);
    }

    public async Task ClearSessionAsync()
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.sessionStorage.clear");
    }
}
