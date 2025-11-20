namespace Nalam360.Platform.Security.Cryptography;

/// <summary>
/// Service for encryption and decryption operations.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string.
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string.
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encrypts binary data.
    /// </summary>
    byte[] Encrypt(byte[] data);

    /// <summary>
    /// Decrypts binary data.
    /// </summary>
    byte[] Decrypt(byte[] encryptedData);
}
