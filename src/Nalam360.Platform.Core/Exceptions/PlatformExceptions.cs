namespace Nalam360.Platform.Core.Exceptions;

/// <summary>
/// Base exception for all platform exceptions
/// </summary>
public abstract class PlatformException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object>? Metadata { get; }

    protected PlatformException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    protected PlatformException(string errorCode, string message, Dictionary<string, object>? metadata, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Metadata = metadata;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public sealed class NotFoundException : PlatformException
{
    public NotFoundException(string resourceType, object resourceId)
        : base("NOT_FOUND", $"{resourceType} with id '{resourceId}' was not found")
    {
    }
}

/// <summary>
/// Exception thrown when a validation error occurs
/// </summary>
public sealed class ValidationException : PlatformException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = new[] { error } })
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public sealed class BusinessRuleException : PlatformException
{
    public BusinessRuleException(string message)
        : base("BUSINESS_RULE_VIOLATION", message)
    {
    }

    public BusinessRuleException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource)
/// </summary>
public sealed class ConflictException : PlatformException
{
    public ConflictException(string message)
        : base("CONFLICT", message)
    {
    }
}

/// <summary>
/// Exception thrown when unauthorized access is attempted
/// </summary>
public sealed class UnauthorizedException : PlatformException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base("UNAUTHORIZED", message)
    {
    }
}

/// <summary>
/// Exception thrown when forbidden access is attempted
/// </summary>
public sealed class ForbiddenException : PlatformException
{
    public ForbiddenException(string message = "Forbidden access")
        : base("FORBIDDEN", message)
    {
    }
}
