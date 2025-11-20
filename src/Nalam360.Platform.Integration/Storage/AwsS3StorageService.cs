using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// AWS S3 storage implementation.
/// </summary>
public class AwsS3StorageService : IBlobStorageService
{
    private readonly ILogger<AwsS3StorageService> _logger;
    private readonly IAmazonS3 _client;

    public AwsS3StorageService(
        ILogger<AwsS3StorageService> logger,
        IOptions<AwsS3Options> options)
    {
        _logger = logger;
        var opts = options.Value;

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(opts.Region)
        };

        _client = new AmazonS3Client(opts.AccessKey, opts.SecretKey, config);

        _logger.LogInformation("AWS S3 client initialized for region: {Region}", opts.Region);
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
            var request = new PutObjectRequest
            {
                BucketName = containerName,
                Key = blobName,
                InputStream = content,
                ContentType = contentType
            };

            await _client.PutObjectAsync(request, cancellationToken);

            var url = $"https://{containerName}.s3.amazonaws.com/{blobName}";

            _logger.LogInformation(
                "Uploaded object {Key} to bucket {Bucket}",
                blobName,
                containerName);

            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload object {Key}", blobName);
            return Result<string>.Failure(Error.Internal(
                "S3.UploadFailed",
                $"Failed to upload object: {ex.Message}"));
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
            var request = new GetObjectRequest
            {
                BucketName = containerName,
                Key = blobName
            };

            var response = await _client.GetObjectAsync(request, cancellationToken);

            _logger.LogInformation(
                "Downloaded object {Key} from bucket {Bucket}",
                blobName,
                containerName);

            return Result<Stream>.Success(response.ResponseStream);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Object not found: {Key}", blobName);
            return Result<Stream>.Failure(Error.NotFound("Object", blobName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download object {Key}", blobName);
            return Result<Stream>.Failure(Error.Internal(
                "S3.DownloadFailed",
                $"Failed to download object: {ex.Message}"));
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
            var request = new DeleteObjectRequest
            {
                BucketName = containerName,
                Key = blobName
            };

            await _client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation(
                "Deleted object {Key} from bucket {Bucket}",
                blobName,
                containerName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete object {Key}", blobName);
            return Result.Failure(Error.Internal(
                "S3.DeleteFailed",
                $"Failed to delete object: {ex.Message}"));
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
            var request = new GetObjectMetadataRequest
            {
                BucketName = containerName,
                Key = blobName
            };

            await _client.GetObjectMetadataAsync(request, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check object existence {Key}", blobName);
            return Result<bool>.Failure(Error.Internal(
                "S3.ExistsFailed",
                $"Failed to check object existence: {ex.Message}"));
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
            var request = new ListObjectsV2Request
            {
                BucketName = containerName,
                Prefix = prefix
            };

            var response = await _client.ListObjectsV2Async(request, cancellationToken);
            var keys = response.S3Objects.Select(o => o.Key).ToList();

            _logger.LogInformation(
                "Listed {Count} objects in bucket {Bucket}",
                keys.Count,
                containerName);

            return Result<IReadOnlyList<string>>.Success(keys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {Bucket}", containerName);
            return Result<IReadOnlyList<string>>.Failure(Error.Internal(
                "S3.ListFailed",
                $"Failed to list objects: {ex.Message}"));
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
            var request = new GetPreSignedUrlRequest
            {
                BucketName = containerName,
                Key = blobName,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = await Task.Run(() => _client.GetPreSignedURL(request), cancellationToken);

            _logger.LogInformation(
                "Generated pre-signed URL for object {Key}, expires in {Expiration}",
                blobName,
                expiration);

            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate pre-signed URL for object {Key}", blobName);
            return Result<string>.Failure(Error.Internal(
                "S3.UrlGenerationFailed",
                $"Failed to generate pre-signed URL: {ex.Message}"));
        }
    }
}
