namespace Nalam360.Platform.Security.Tokens;

/// <summary>
/// JWT token service interface.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token.
    /// </summary>
    string GenerateToken(TokenRequest request);

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    TokenValidationResult ValidateToken(string token);
}

/// <summary>
/// Token generation request.
/// </summary>
public record TokenRequest(
    string Subject,
    string? Audience = null,
    TimeSpan? ExpiresIn = null,
    Dictionary<string, object>? Claims = null);

/// <summary>
/// Token validation result.
/// </summary>
public record TokenValidationResult(
    bool IsValid,
    string? Subject = null,
    Dictionary<string, object>? Claims = null,
    string? Error = null);
