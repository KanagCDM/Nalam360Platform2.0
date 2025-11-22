using MedWay.Domain.Primitives;
using System.Text.RegularExpressions;

namespace MedWay.Domain.ValueObjects;

/// <summary>
/// Phone number value object
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; private set; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PhoneNumber>(Error.Validation(nameof(PhoneNumber), "Phone number is required"));

        var cleaned = Regex.Replace(value, @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
            return Result.Failure<PhoneNumber>(Error.Validation(nameof(PhoneNumber), "Invalid phone number format"));

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
