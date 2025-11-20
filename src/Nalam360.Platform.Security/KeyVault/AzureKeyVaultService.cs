using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Security.KeyVault;

/// <summary>
/// Azure Key Vault implementation of the key vault service.
/// </summary>
public class AzureKeyVaultService : IKeyVaultService
{
    private readonly ILogger<AzureKeyVaultService> _logger;
    private readonly SecretClient _client;

    public AzureKeyVaultService(
        ILogger<AzureKeyVaultService> logger,
        IOptions<AzureKeyVaultOptions> options)
    {
        _logger = logger;
        var opts = options.Value;

        var credential = new DefaultAzureCredential();
        _client = new SecretClient(new Uri(opts.VaultUri), credential);

        _logger.LogInformation(
            "Azure Key Vault client initialized for vault: {VaultUri}",
            opts.VaultUri);
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GetSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            _logger.LogInformation("Retrieved secret: {SecretName}", secretName);

            return Result<string>.Success(secret.Value.Value);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret not found: {SecretName}", secretName);
            return Result<string>.Failure(Error.NotFound("Secret", secretName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
            return Result<string>.Failure(Error.Internal(
                "KeyVault.GetSecretFailed",
                $"Failed to retrieve secret: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> SetSecretAsync(
        string secretName,
        string secretValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SetSecretAsync(secretName, secretValue, cancellationToken);

            _logger.LogInformation("Set secret: {SecretName}", secretName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
            return Result.Failure(Error.Internal(
                "KeyVault.SetSecretFailed",
                $"Failed to set secret: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var operation = await _client.StartDeleteSecretAsync(secretName, cancellationToken);
            await operation.WaitForCompletionAsync(cancellationToken);

            _logger.LogInformation("Deleted secret: {SecretName}", secretName);

            return Result.Success();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret not found for deletion: {SecretName}", secretName);
            return Result.Failure(Error.NotFound("Secret", secretName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret: {SecretName}", secretName);
            return Result.Failure(Error.Internal(
                "KeyVault.DeleteSecretFailed",
                $"Failed to delete secret: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<string>>> ListSecretsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var secretNames = new List<string>();

            await foreach (var secretProperties in _client.GetPropertiesOfSecretsAsync(cancellationToken))
            {
                secretNames.Add(secretProperties.Name);
            }

            _logger.LogInformation("Listed {Count} secrets", secretNames.Count);

            return Result<IReadOnlyList<string>>.Success(secretNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list secrets");
            return Result<IReadOnlyList<string>>.Failure(Error.Internal(
                "KeyVault.ListSecretsFailed",
                $"Failed to list secrets: {ex.Message}"));
        }
    }
}
