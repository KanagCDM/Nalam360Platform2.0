namespace MedWay.Domain.Primitives;

/// <summary>
/// Represents an error with code and message
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} with ID '{id}' was not found");

    public static Error Validation(string property, string message) =>
        new($"Validation.{property}", message);

    public static Error Conflict(string message) =>
        new("Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized access") =>
        new("Unauthorized", message);

    public static Error Forbidden(string message = "Forbidden") =>
        new("Forbidden", message);

    public static Error InternalError(string message = "An internal error occurred") =>
        new("InternalError", message);
}
