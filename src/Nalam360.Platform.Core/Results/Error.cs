namespace Nalam360.Platform.Core.Results;

/// <summary>
/// Represents an error with a code and message
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Message { get; }
    public Dictionary<string, object>? Metadata { get; init; }

    public Error(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public Error WithMetadata(string key, object value)
    {
        var metadata = Metadata ?? new Dictionary<string, object>();
        metadata[key] = value;
        return this with { Metadata = metadata };
    }

    public static Error NotFound(string entity, object id) 
        => new("NOT_FOUND", $"{entity} with id '{id}' was not found");

    public static Error Validation(string message) 
        => new("VALIDATION_ERROR", message);

    public static Error Conflict(string message) 
        => new("CONFLICT", message);

    public static Error Unauthorized(string message = "Unauthorized access") 
        => new("UNAUTHORIZED", message);

    public static Error Forbidden(string message = "Forbidden access") 
        => new("FORBIDDEN", message);

    public static Error Internal(string message = "An internal error occurred") 
        => new("INTERNAL_ERROR", message);

    public static Error Internal(string code, string message) 
        => new(code, message);

    public static Error Database(string code, string message)
        => new(code, message);

    public static Error Unexpected(string code, string message)
        => new(code, message);

    public static Error Unavailable(string code, string message) 
        => new(code, message);

    public static Error TooManyRequests(string code, string message) 
        => new(code, message);
}
