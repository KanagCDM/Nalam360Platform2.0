using System.Linq.Expressions;

namespace Nalam360Enterprise.UI.Core.Forms;

/// <summary>
/// Validation rule severity
/// </summary>
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationError> Warnings { get; set; } = new();
    public List<ValidationError> Infos { get; set; } = new();

    public bool HasErrors => Errors.Any();
    public bool HasWarnings => Warnings.Any();
}

/// <summary>
/// Represents a validation error
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public object? AttemptedValue { get; set; }
}

/// <summary>
/// Base interface for validation rules
/// </summary>
public interface IValidationRule<T>
{
    Task<ValidationResult> ValidateAsync(T value);
    string PropertyName { get; }
    ValidationSeverity Severity { get; }
}

/// <summary>
/// Abstract base class for validation rules
/// </summary>
public abstract class ValidationRule<T> : IValidationRule<T>
{
    public string PropertyName { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    protected string ErrorMessage { get; set; } = "Validation failed";

    public abstract Task<ValidationResult> ValidateAsync(T value);

    protected ValidationResult CreateError(T value)
    {
        var result = new ValidationResult { IsValid = false };
        var error = new ValidationError
        {
            PropertyName = PropertyName,
            Message = ErrorMessage,
            Severity = Severity,
            AttemptedValue = value
        };

        if (Severity == ValidationSeverity.Error)
            result.Errors.Add(error);
        else if (Severity == ValidationSeverity.Warning)
            result.Warnings.Add(error);
        else
            result.Infos.Add(error);

        return result;
    }
}

/// <summary>
/// Required field validation rule
/// </summary>
public class RequiredRule<T> : ValidationRule<T>
{
    public RequiredRule()
    {
        ErrorMessage = "This field is required";
    }

    public override Task<ValidationResult> ValidateAsync(T value)
    {
        var result = new ValidationResult();

        if (value == null || 
            (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            return Task.FromResult(CreateError(value));
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// String length validation rule
/// </summary>
public class StringLengthRule : ValidationRule<string>
{
    public int MinLength { get; set; }
    public int MaxLength { get; set; }

    public StringLengthRule(int minLength, int maxLength)
    {
        MinLength = minLength;
        MaxLength = maxLength;
        ErrorMessage = $"Length must be between {minLength} and {maxLength} characters";
    }

    public override Task<ValidationResult> ValidateAsync(string value)
    {
        var result = new ValidationResult();

        if (value != null && (value.Length < MinLength || value.Length > MaxLength))
        {
            return Task.FromResult(CreateError(value));
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// Regular expression validation rule
/// </summary>
public class RegexRule : ValidationRule<string>
{
    private readonly System.Text.RegularExpressions.Regex _regex;

    public RegexRule(string pattern, string errorMessage)
    {
        _regex = new System.Text.RegularExpressions.Regex(pattern);
        ErrorMessage = errorMessage;
    }

    public override Task<ValidationResult> ValidateAsync(string value)
    {
        var result = new ValidationResult();

        if (value != null && !_regex.IsMatch(value))
        {
            return Task.FromResult(CreateError(value));
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// Email validation rule
/// </summary>
public class EmailRule : RegexRule
{
    public EmailRule() : base(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        "Please enter a valid email address")
    {
    }
}

/// <summary>
/// Numeric range validation rule
/// </summary>
public class RangeRule<T> : ValidationRule<T> where T : IComparable<T>
{
    public T Min { get; set; }
    public T Max { get; set; }

    public RangeRule(T min, T max)
    {
        Min = min;
        Max = max;
        ErrorMessage = $"Value must be between {min} and {max}";
    }

    public override Task<ValidationResult> ValidateAsync(T value)
    {
        var result = new ValidationResult();

        if (value != null && (value.CompareTo(Min) < 0 || value.CompareTo(Max) > 0))
        {
            return Task.FromResult(CreateError(value));
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// Custom validation rule using a predicate function
/// </summary>
public class CustomRule<T> : ValidationRule<T>
{
    private readonly Func<T, Task<bool>> _validationFunc;

    public CustomRule(Func<T, Task<bool>> validationFunc, string errorMessage)
    {
        _validationFunc = validationFunc;
        ErrorMessage = errorMessage;
    }

    public CustomRule(Func<T, bool> validationFunc, string errorMessage)
    {
        _validationFunc = value => Task.FromResult(validationFunc(value));
        ErrorMessage = errorMessage;
    }

    public override async Task<ValidationResult> ValidateAsync(T value)
    {
        var result = new ValidationResult();
        var isValid = await _validationFunc(value);

        if (!isValid)
        {
            return CreateError(value);
        }

        return result;
    }
}
