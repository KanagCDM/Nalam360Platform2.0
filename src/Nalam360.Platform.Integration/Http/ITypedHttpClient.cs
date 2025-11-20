namespace Nalam360.Platform.Integration.Http;

/// <summary>
/// Typed HTTP client interface for external API integration.
/// </summary>
/// <typeparam name="TClient">The client type.</typeparam>
public interface ITypedHttpClient<TClient>
{
    /// <summary>
    /// Gets data from an endpoint.
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts data to an endpoint.
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default);
}
