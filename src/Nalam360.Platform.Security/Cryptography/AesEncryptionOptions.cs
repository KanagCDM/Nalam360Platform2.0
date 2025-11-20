namespace Nalam360.Platform.Security.Cryptography;

/// <summary>
/// Configuration options for AES encryption.
/// </summary>
public class AesEncryptionOptions
{
    /// <summary>
    /// Gets or sets the encryption key.
    /// Must be at least 32 characters for AES-256.
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;
}
