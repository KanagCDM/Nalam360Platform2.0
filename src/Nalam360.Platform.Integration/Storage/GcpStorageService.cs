using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// Google Cloud Storage implementation of blob storage service.
/// </summary>
public class GcpStorageService : IBlobStorageService
{
    private readonly StorageClient _client;
    private readonly ILogger<GcpStorageService> _logger;
    private readonly GcpStorageOptions _options;
    private readonly UrlSigner _urlSigner;

    public GcpStorageService(
        ILogger<GcpStorageService> logger,
        IOptions<GcpStorageOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        // Initialize Storage Client
        _client = string.IsNullOrEmpty(_options.CredentialsJson)
            ? StorageClient.Create()
            : StorageClient.Create(Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(_options.CredentialsJson));

        // Initialize URL Signer
        _urlSigner = string.IsNullOrEmpty(_options.CredentialsJson)
            ? UrlSigner.FromServiceAccountPath(_options.ServiceAccountKeyPath)
            : UrlSigner.FromServiceAccountCredential(
                Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(_options.CredentialsJson)
                    .UnderlyingCredential as Google.Apis.Auth.OAuth2.ServiceAccountCredential);
    }

    public async Task<Result<string>> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Uploading blob {BlobName} to bucket {BucketName}", blobName, containerName);

            var obj = await _client.UploadObjectAsync(
                bucket: containerName,
                objectName: blobName,
                contentType: contentType ?? _options.DefaultContentType,
                source: content,
                options: new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.Private },
                cancellationToken: ct);

            _logger.LogInformation("Successfully uploaded blob {BlobName}", blobName);
            return Result<string>.Success(obj.MediaLink);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "GCP API error uploading blob {BlobName}: {Error}", blobName, ex.Message);
            return Result<string>.Failure(Error.Internal("GcpStorageError", $"Failed to upload blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob {BlobName}", blobName);
            return Result<string>.Failure(Error.Unexpected("UploadError", ex.Message));
        }
    }

    public async Task<Result<Stream>> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Downloading blob {BlobName} from bucket {BucketName}", blobName, containerName);

            var stream = new MemoryStream();
            await _client.DownloadObjectAsync(
                bucket: containerName,
                objectName: blobName,
                destination: stream,
                options: null,
                cancellationToken: ct);

            stream.Position = 0;
            _logger.LogInformation("Successfully downloaded blob {BlobName}", blobName);
            return Result<Stream>.Success(stream);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Blob {BlobName} not found in bucket {BucketName}", blobName, containerName);
            return Result<Stream>.Failure(Error.NotFound("Blob", blobName));
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "GCP API error downloading blob {BlobName}: {Error}", blobName, ex.Message);
            return Result<Stream>.Failure(Error.Internal("GcpStorageError", $"Failed to download blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading blob {BlobName}", blobName);
            return Result<Stream>.Failure(Error.Unexpected("DownloadError", ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting blob {BlobName} from bucket {BucketName}", blobName, containerName);

            await _client.DeleteObjectAsync(
                bucket: containerName,
                objectName: blobName,
                options: null,
                cancellationToken: ct);

            _logger.LogInformation("Successfully deleted blob {BlobName}", blobName);
            return Result.Success();
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Blob {BlobName} not found in bucket {BucketName}", blobName, containerName);
            return Result.Success(); // Idempotent - already deleted
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "GCP API error deleting blob {BlobName}: {Error}", blobName, ex.Message);
            return Result.Failure(Error.Internal("GcpStorageError", $"Failed to delete blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobName}", blobName);
            return Result.Failure(Error.Unexpected("DeleteError", ex.Message));
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken ct = default)
    {
        try
        {
            var obj = await _client.GetObjectAsync(
                bucket: containerName,
                objectName: blobName,
                options: null,
                cancellationToken: ct);

            return Result<bool>.Success(obj != null);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking blob existence {BlobName}", blobName);
            return Result<bool>.Failure(Error.Unexpected("ExistsError", ex.Message));
        }
    }

    public async Task<Result> CopyAsync(
        string sourceContainer,
        string sourceBlobName,
        string destinationContainer,
        string destinationBlobName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Copying blob from {SourceBucket}/{SourceBlob} to {DestBucket}/{DestBlob}",
                sourceContainer, sourceBlobName, destinationContainer, destinationBlobName);

            await _client.CopyObjectAsync(
                sourceBucket: sourceContainer,
                sourceObjectName: sourceBlobName,
                destinationBucket: destinationContainer,
                destinationObjectName: destinationBlobName,
                options: null,
                cancellationToken: ct);

            _logger.LogInformation("Successfully copied blob to {DestBucket}/{DestBlob}",
                destinationContainer, destinationBlobName);
            return Result.Success();
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "GCP API error copying blob: {Error}", ex.Message);
            return Result.Failure(Error.Internal("GcpStorageError", $"Failed to copy blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying blob");
            return Result.Failure(Error.Unexpected("CopyError", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<string>>> ListBlobsAsync(
        string containerName,
        string? prefix = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Listing blobs in bucket {BucketName} with prefix {Prefix}", containerName, prefix);

            var blobs = new List<string>();
            var objects = _client.ListObjectsAsync(containerName, prefix);

            await foreach (var obj in objects.WithCancellation(ct))
            {
                blobs.Add(obj.Name);
            }

            _logger.LogInformation("Found {Count} blobs in bucket {BucketName}", blobs.Count, containerName);
            return Result<IReadOnlyList<string>>.Success(blobs);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "GCP API error listing blobs: {Error}", ex.Message);
            return Result<IReadOnlyList<string>>.Failure(Error.Internal("GcpStorageError", $"Failed to list blobs: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs");
            return Result<IReadOnlyList<string>>.Failure(Error.Unexpected("ListError", ex.Message));
        }
    }

    public async Task<Result<string>> GetSharedAccessUrlAsync(
        string containerName,
        string blobName,
        TimeSpan expiration,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating signed URL for blob {BlobName}, expiration: {Expiration}",
                blobName, expiration);

            var url = await _urlSigner.SignAsync(
                bucket: containerName,
                objectName: blobName,
                duration: expiration,
                cancellationToken: ct);

            _logger.LogInformation("Successfully generated signed URL for blob {BlobName}", blobName);
            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signed URL for blob {BlobName}", blobName);
            return Result<string>.Failure(Error.Unexpected("SignedUrlError", ex.Message));
        }
    }
}

/// <summary>
/// Configuration options for Google Cloud Storage.
/// </summary>
public class GcpStorageOptions
{
    /// <summary>
    /// GCP project ID.
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Service account credentials JSON content.
    /// </summary>
    public string? CredentialsJson { get; set; }

    /// <summary>
    /// Path to service account key file (alternative to CredentialsJson).
    /// </summary>
    public string? ServiceAccountKeyPath { get; set; }

    /// <summary>
    /// Default content type for uploaded blobs.
    /// </summary>
    public string DefaultContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Default bucket name.
    /// </summary>
    public string? DefaultBucket { get; set; }
}
