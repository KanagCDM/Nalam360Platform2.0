using System.Security.Cryptography;

namespace Nalam360.Platform.Core.Security;

/// <summary>
/// Cryptographically secure random number generator
/// </summary>
public sealed class CryptoRandomNumberGenerator : IRandomNumberGenerator
{
    public int GetInt32(int minValue, int maxValue)
    {
        return RandomNumberGenerator.GetInt32(minValue, maxValue);
    }

    public void GetBytes(byte[] buffer)
    {
        RandomNumberGenerator.Fill(buffer);
    }

    public void GetBytes(Span<byte> buffer)
    {
        RandomNumberGenerator.Fill(buffer);
    }

    public string GetString(int length, string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
    {
        ArgumentNullException.ThrowIfNull(characterSet);
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (characterSet.Length == 0) throw new ArgumentException("Character set cannot be empty", nameof(characterSet));

        var result = new char[length];
        var bytes = new byte[length * 4]; // Allocate more to reduce bias
        RandomNumberGenerator.Fill(bytes);

        for (int i = 0; i < length; i++)
        {
            var randomValue = BitConverter.ToUInt32(bytes, i * 4);
            result[i] = characterSet[(int)(randomValue % (uint)characterSet.Length)];
        }

        return new string(result);
    }
}
