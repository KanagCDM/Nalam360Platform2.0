using MedWay.Domain.Primitives;
using System.Text.RegularExpressions;

namespace MedWay.Domain.ValueObjects;

/// <summary>
/// Email value object
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(Error.Validation(nameof(Email), "Email is required"));

        if (value.Length > 255)
            return Result.Failure<Email>(Error.Validation(nameof(Email), "Email is too long"));

        if (!EmailRegex.IsMatch(value))
            return Result.Failure<Email>(Error.Validation(nameof(Email), "Invalid email format"));

        return new Email(value.ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
