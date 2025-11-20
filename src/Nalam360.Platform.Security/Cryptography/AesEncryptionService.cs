using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Nalam360.Platform.Security.Cryptography;

/// <summary>
/// AES-256 encryption service implementation.
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IOptions<AesEncryptionOptions> options)
    {
        var opts = options.Value;
        
        // Derive key and IV from the provided key
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(opts.EncryptionKey));
        
        using var md5 = MD5.Create();
        _iv = md5.ComputeHash(Encoding.UTF8.GetBytes(opts.EncryptionKey));
    }

    /// <inheritdoc/>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = Encrypt(plainBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <inheritdoc/>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentNullException(nameof(cipherText));

        var encryptedBytes = Convert.FromBase64String(cipherText);
        var decryptedBytes = Decrypt(encryptedBytes);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <inheritdoc/>
    public byte[] Encrypt(byte[] data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    /// <inheritdoc/>
    public byte[] Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            throw new ArgumentNullException(nameof(encryptedData));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }
}
