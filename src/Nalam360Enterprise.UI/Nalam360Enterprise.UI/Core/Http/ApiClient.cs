using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nalam360.Platform.Core.Results;

namespace Nalam360Enterprise.UI.Core.Http;

/// <summary>
/// Default implementation of IApiClient with retry logic and error handling
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    /// <summary>
    /// Initializes a new instance of the ApiClient class
    /// </summary>
    /// <param name="httpClient">The HTTP client</param>
    /// <param name="logger">The logger</param>
    /// <param name="maxRetries">Maximum number of retries (default: 3)</param>
    /// <param name="retryDelaySeconds">Delay between retries in seconds (default: 1)</param>
    public ApiClient(
        HttpClient httpClient,
        ILogger<ApiClient> logger,
        int maxRetries = 3,
        int retryDelaySeconds = 1)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc/>
    public async Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<T>> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<T>> PatchAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var content = CreateJsonContent(data);
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public void SetAuthorizationToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or whitespace", nameof(token));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _logger.LogDebug("Authorization token set");
    }

    /// <inheritdoc/>
    public void ClearAuthorizationToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _logger.LogDebug("Authorization token cleared");
    }

    /// <inheritdoc/>
    public void SetHeader(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Header name cannot be null or whitespace", nameof(name));
        }

        _httpClient.DefaultRequestHeaders.Remove(name);
        _httpClient.DefaultRequestHeaders.Add(name, value);
        _logger.LogDebug("Header set: {HeaderName}", name);
    }

    /// <inheritdoc/>
    public void RemoveHeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Header name cannot be null or whitespace", nameof(name));
        }

        _httpClient.DefaultRequestHeaders.Remove(name);
        _logger.LogDebug("Header removed: {HeaderName}", name);
    }

    /// <summary>
    /// Executes a request with retry logic
    /// </summary>
    private async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> action,
        CancellationToken cancellationToken)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < _maxRetries)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (ShouldRetry(ex, attempt))
            {
                lastException = ex;
                attempt++;
                _logger.LogWarning(ex, "API request failed (attempt {Attempt}/{MaxRetries})", attempt, _maxRetries);
                
                if (attempt < _maxRetries)
                {
                    await Task.Delay(_retryDelay * attempt, cancellationToken);
                }
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                attempt++;
                _logger.LogWarning(ex, "API request timed out (attempt {Attempt}/{MaxRetries})", attempt, _maxRetries);
                
                if (attempt < _maxRetries)
                {
                    await Task.Delay(_retryDelay * attempt, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API request failed with non-retryable error");
                return Result<T>.Failure(new Error("ApiError", ex.Message));
            }
        }

        _logger.LogError(lastException, "API request failed after {MaxRetries} attempts", _maxRetries);
        return Result<T>.Failure(new Error("ApiError", lastException?.Message ?? "Request failed after multiple retries"));
    }

    /// <summary>
    /// Processes the HTTP response and returns a Result
    /// </summary>
    private async Task<Result<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
                return data != null
                    ? Result<T>.Success(data)
                    : Result<T>.Failure(new Error("DeserializationError", "Failed to deserialize response"));
            }

            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "API request failed with status {StatusCode}: {ErrorMessage}",
                response.StatusCode,
                errorMessage);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound => Result<T>.Failure(new Error("NotFound", "The requested resource was not found")),
                System.Net.HttpStatusCode.Unauthorized => Result<T>.Failure(Error.Unauthorized("Authentication required")),
                System.Net.HttpStatusCode.Forbidden => Result<T>.Failure(Error.Forbidden("Access denied")),
                System.Net.HttpStatusCode.BadRequest => Result<T>.Failure(Error.Validation(errorMessage)),
                _ => Result<T>.Failure(new Error("ApiError", $"Request failed with status {response.StatusCode}"))
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize API response");
            return Result<T>.Failure(new Error("DeserializationError", "Failed to process server response"));
        }
    }

    /// <summary>
    /// Creates JSON content from an object
    /// </summary>
    private StringContent CreateJsonContent(object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Determines if a request should be retried based on the exception
    /// </summary>
    private bool ShouldRetry(Exception exception, int attempt)
    {
        if (attempt >= _maxRetries)
        {
            return false;
        }

        // Retry on network errors and timeouts
        return exception is HttpRequestException or TaskCanceledException;
    }
}
