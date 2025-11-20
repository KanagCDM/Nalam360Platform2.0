using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Security.KeyVault;

/// <summary>
/// Service for managing secrets in a key vault.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Gets a secret from the key vault.
    /// </summary>
    Task<Result<string>> GetSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a secret in the key vault.
    /// </summary>
    Task<Result> SetSecretAsync(
        string secretName,
        string secretValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret from the key vault.
    /// </summary>
    Task<Result> DeleteSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all secret names in the key vault.
    /// </summary>
    Task<Result<IReadOnlyList<string>>> ListSecretsAsync(
        CancellationToken cancellationToken = default);
}
