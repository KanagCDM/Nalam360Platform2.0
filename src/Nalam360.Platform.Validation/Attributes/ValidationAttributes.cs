using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Nalam360.Platform.Validation.Attributes;

/// <summary>
/// Validates that a string contains only alphanumeric characters.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class AlphanumericAttribute : ValidationAttribute
{
    private readonly bool _allowSpaces;

    public AlphanumericAttribute(bool allowSpaces = false)
    {
        _allowSpaces = allowSpaces;
        ErrorMessage = allowSpaces
            ? "The field {0} must contain only alphanumeric characters and spaces."
            : "The field {0} must contain only alphanumeric characters.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var input = value.ToString()!;
        var pattern = _allowSpaces ? @"^[a-zA-Z0-9\s]+$" : @"^[a-zA-Z0-9]+$";

        if (!Regex.IsMatch(input, pattern))
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a string contains at least one uppercase, lowercase, digit, and special character.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    public int MinimumLength { get; set; } = 8;

    public StrongPasswordAttribute()
    {
        ErrorMessage = "The field {0} must be at least {1} characters and contain uppercase, lowercase, digit, and special character.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var password = value.ToString()!;

        if (password.Length < MinimumLength)
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, MinimumLength),
                new[] { validationContext.MemberName! });
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, MinimumLength),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a string is a valid email address with custom domain restrictions.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class EmailDomainAttribute : ValidationAttribute
{
    private readonly string[] _allowedDomains;

    public EmailDomainAttribute(params string[] allowedDomains)
    {
        _allowedDomains = allowedDomains;
        ErrorMessage = "The field {0} must be an email address from allowed domains: {1}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var email = value.ToString()!;
        var emailAttribute = new EmailAddressAttribute();

        if (!emailAttribute.IsValid(email))
        {
            return new ValidationResult(
                "The field {0} is not a valid email address.",
                new[] { validationContext.MemberName! });
        }

        if (_allowedDomains.Length > 0)
        {
            var domain = email.Split('@').LastOrDefault();
            if (domain == null || !_allowedDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult(
                    string.Format(ErrorMessage!, validationContext.DisplayName, string.Join(", ", _allowedDomains)),
                    new[] { validationContext.MemberName! });
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a numeric value falls within a specified range.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NumericRangeAttribute : ValidationAttribute
{
    private readonly double _minimum;
    private readonly double _maximum;

    public NumericRangeAttribute(double minimum, double maximum)
    {
        _minimum = minimum;
        _maximum = maximum;
        ErrorMessage = "The field {0} must be between {1} and {2}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (!double.TryParse(value.ToString(), out var numericValue))
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a numeric value.",
                new[] { validationContext.MemberName! });
        }

        if (numericValue < _minimum || numericValue > _maximum)
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, _minimum, _maximum),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a date is not in the past.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    private readonly bool _allowToday;

    public FutureDateAttribute(bool allowToday = true)
    {
        _allowToday = allowToday;
        ErrorMessage = allowToday
            ? "The field {0} must be today or a future date."
            : "The field {0} must be a future date.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not DateTime date)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a DateTime value.",
                new[] { validationContext.MemberName! });
        }

        var comparison = _allowToday ? date.Date >= DateTime.UtcNow.Date : date.Date > DateTime.UtcNow.Date;

        if (!comparison)
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a date is not in the future.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class PastDateAttribute : ValidationAttribute
{
    private readonly bool _allowToday;

    public PastDateAttribute(bool allowToday = true)
    {
        _allowToday = allowToday;
        ErrorMessage = allowToday
            ? "The field {0} must be today or a past date."
            : "The field {0} must be a past date.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not DateTime date)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a DateTime value.",
                new[] { validationContext.MemberName! });
        }

        var comparison = _allowToday ? date.Date <= DateTime.UtcNow.Date : date.Date < DateTime.UtcNow.Date;

        if (!comparison)
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a collection contains a minimum number of items.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class MinimumCountAttribute : ValidationAttribute
{
    private readonly int _minimumCount;

    public MinimumCountAttribute(int minimumCount)
    {
        _minimumCount = minimumCount;
        ErrorMessage = "The field {0} must contain at least {1} items.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not System.Collections.IEnumerable enumerable)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a collection.",
                new[] { validationContext.MemberName! });
        }

        var count = enumerable.Cast<object>().Count();

        if (count < _minimumCount)
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, _minimumCount),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a collection contains a maximum number of items.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class MaximumCountAttribute : ValidationAttribute
{
    private readonly int _maximumCount;

    public MaximumCountAttribute(int maximumCount)
    {
        _maximumCount = maximumCount;
        ErrorMessage = "The field {0} must contain at most {1} items.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not System.Collections.IEnumerable enumerable)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a collection.",
                new[] { validationContext.MemberName! });
        }

        var count = enumerable.Cast<object>().Count();

        if (count > _maximumCount)
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, _maximumCount),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a string matches a specific regex pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class RegexPatternAttribute : ValidationAttribute
{
    private readonly string _pattern;
    private readonly RegexOptions _options;

    public RegexPatternAttribute(string pattern, RegexOptions options = RegexOptions.None)
    {
        _pattern = pattern;
        _options = options;
        ErrorMessage = "The field {0} does not match the required pattern.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var input = value.ToString()!;

        if (!Regex.IsMatch(input, _pattern, _options))
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a property value matches another property value (e.g., password confirmation).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class ComparePropertyAttribute : ValidationAttribute
{
    private readonly string _otherPropertyName;

    public ComparePropertyAttribute(string otherPropertyName)
    {
        _otherPropertyName = otherPropertyName;
        ErrorMessage = "The field {0} must match {1}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);

        if (otherProperty == null)
        {
            return new ValidationResult(
                $"Unknown property: {_otherPropertyName}",
                new[] { validationContext.MemberName! });
        }

        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if (!Equals(value, otherValue))
        {
            return new ValidationResult(
                string.Format(ErrorMessage!, validationContext.DisplayName, _otherPropertyName),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}
