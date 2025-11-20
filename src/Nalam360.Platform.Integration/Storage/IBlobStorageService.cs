using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// Abstraction for blob storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a blob to storage.
    /// </summary>
    Task<Result<string>> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a blob from storage.
    /// </summary>
    Task<Result<Stream>> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a blob from storage.
    /// </summary>
    Task<Result> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a blob exists.
    /// </summary>
    Task<Result<bool>> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists blobs in a container.
    /// </summary>
    Task<Result<IReadOnlyList<string>>> ListBlobsAsync(
        string containerName,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a shared access URL for a blob.
    /// </summary>
    Task<Result<string>> GetSharedAccessUrlAsync(
        string containerName,
        string blobName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}
