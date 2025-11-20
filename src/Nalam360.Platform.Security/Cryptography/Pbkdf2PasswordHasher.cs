using System.Security.Cryptography;

namespace Nalam360.Platform.Security.Cryptography;

/// <summary>
/// Password hasher using PBKDF2 with HMAC-SHA256.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        // Format: {iterations}.{salt}.{hash}
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        ArgumentException.ThrowIfNullOrEmpty(hash);

        var parts = hash.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var storedHash = Convert.FromBase64String(parts[2]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
