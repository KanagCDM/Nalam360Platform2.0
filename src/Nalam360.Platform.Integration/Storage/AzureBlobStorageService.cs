using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// Azure Blob Storage implementation.
/// </summary>
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly BlobServiceClient _client;

    public AzureBlobStorageService(
        ILogger<AzureBlobStorageService> logger,
        IOptions<AzureBlobStorageOptions> options)
    {
        _logger = logger;
        var opts = options.Value;

        _client = new BlobServiceClient(opts.ConnectionString);

        _logger.LogInformation("Azure Blob Storage client initialized");
    }

    /// <inheritdoc/>
    public async Task<Result<string>> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);
            
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

            _logger.LogInformation(
                "Uploaded blob {BlobName} to container {Container}",
                blobName,
                containerName);

            return Result<string>.Success(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob {BlobName}", blobName);
            return Result<string>.Failure(Error.Internal(
                "BlobStorage.UploadFailed",
                $"Failed to upload blob: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Stream>> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Downloaded blob {BlobName} from container {Container}",
                blobName,
                containerName);

            return Result<Stream>.Success(response.Value.Content);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob not found: {BlobName}", blobName);
            return Result<Stream>.Failure(Error.NotFound("Blob", blobName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob {BlobName}", blobName);
            return Result<Stream>.Failure(Error.Internal(
                "BlobStorage.DownloadFailed",
                $"Failed to download blob: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Deleted blob {BlobName} from container {Container}",
                blobName,
                containerName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete blob {BlobName}", blobName);
            return Result.Failure(Error.Internal(
                "BlobStorage.DeleteFailed",
                $"Failed to delete blob: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync(cancellationToken);

            return Result<bool>.Success(exists.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check blob existence {BlobName}", blobName);
            return Result<bool>.Failure(Error.Internal(
                "BlobStorage.ExistsFailed",
                $"Failed to check blob existence: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<string>>> ListBlobsAsync(
        string containerName,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            var blobNames = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(
                prefix: prefix,
                cancellationToken: cancellationToken))
            {
                blobNames.Add(blobItem.Name);
            }

            _logger.LogInformation(
                "Listed {Count} blobs in container {Container}",
                blobNames.Count,
                containerName);

            return Result<IReadOnlyList<string>>.Success(blobNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs in container {Container}", containerName);
            return Result<IReadOnlyList<string>>.Failure(Error.Internal(
                "BlobStorage.ListFailed",
                $"Failed to list blobs: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GetSharedAccessUrlAsync(
        string containerName,
        string blobName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _client.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation(
                "Generated SAS URL for blob {BlobName}, expires in {Expiration}",
                blobName,
                expiration);

            return await Task.FromResult(Result<string>.Success(sasUri.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SAS URL for blob {BlobName}", blobName);
            return Result<string>.Failure(Error.Internal(
                "BlobStorage.SasGenerationFailed",
                $"Failed to generate SAS URL: {ex.Message}"));
        }
    }
}
