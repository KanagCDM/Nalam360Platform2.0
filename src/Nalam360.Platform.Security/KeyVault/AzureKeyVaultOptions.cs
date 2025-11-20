namespace Nalam360.Platform.Security.KeyVault;

/// <summary>
/// Configuration options for Azure Key Vault.
/// </summary>
public class AzureKeyVaultOptions
{
    /// <summary>
    /// Gets or sets the Key Vault URI.
    /// Format: https://{vault-name}.vault.azure.net/
    /// </summary>
    public string VaultUri { get; set; } = string.Empty;
}
