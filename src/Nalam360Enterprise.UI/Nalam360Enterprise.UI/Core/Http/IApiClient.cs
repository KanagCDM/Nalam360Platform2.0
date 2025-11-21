using Nalam360.Platform.Core.Results;

namespace Nalam360Enterprise.UI.Core.Http;

/// <summary>
/// HTTP client wrapper with Result pattern and automatic error handling
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends a GET request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the response data</returns>
    Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the response data</returns>
    Task<Result<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the response data</returns>
    Task<Result<T>> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the response data</returns>
    Task<Result<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PATCH request
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="data">The request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the response data</returns>
    Task<Result<T>> PatchAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the authorization header for subsequent requests
    /// </summary>
    /// <param name="token">The authorization token</param>
    void SetAuthorizationToken(string token);

    /// <summary>
    /// Clears the authorization header
    /// </summary>
    void ClearAuthorizationToken();

    /// <summary>
    /// Sets a custom header for subsequent requests
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    void SetHeader(string name, string value);

    /// <summary>
    /// Removes a custom header
    /// </summary>
    /// <param name="name">Header name</param>
    void RemoveHeader(string name);
}
