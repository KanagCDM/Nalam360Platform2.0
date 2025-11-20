namespace Nalam360.Platform.Core.Security;

/// <summary>
/// Abstraction for cryptographically secure random number generation
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Generates a random integer between min (inclusive) and max (exclusive)
    /// </summary>
    int GetInt32(int minValue, int maxValue);

    /// <summary>
    /// Fills the buffer with cryptographically strong random bytes
    /// </summary>
    void GetBytes(byte[] buffer);

    /// <summary>
    /// Fills the span with cryptographically strong random bytes
    /// </summary>
    void GetBytes(Span<byte> buffer);

    /// <summary>
    /// Generates a random string of specified length using the given character set
    /// </summary>
    string GetString(int length, string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
}
