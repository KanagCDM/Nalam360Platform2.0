# Form Schema & Validation Guide

## Overview
Comprehensive guide to schema-driven forms in Nalam360 Enterprise UI, inspired by Yup/Zod validation patterns from TypeScript, adapted for C#/Blazor.

## Table of Contents
1. [Quick Start](#quick-start)
2. [ValidationRules API](#validationrules-api)
3. [Schema Examples](#schema-examples)
4. [Migration from DataAnnotations](#migration-from-dataannotations)
5. [Advanced Patterns](#advanced-patterns)
6. [Server-Side Validation](#server-side-validation)
7. [Custom Validators](#custom-validators)

---

## Quick Start

### Basic Validation
```csharp
@code {
    private string email = "";
    
    private ValidationRules emailRules = new ValidationRules()
        .Required("Email is required")
        .Email("Please enter a valid email address");
}
```

```razor
<N360TextBox @bind-Value="@email"
             Label="Email"
             ValidationRules="@emailRules" />
```

### Chained Rules
```csharp
private ValidationRules passwordRules = new ValidationRules()
    .Required("Password is required")
    .MinLength(8, "Password must be at least 8 characters")
    .MaxLength(100, "Password cannot exceed 100 characters")
    .Pattern(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)", 
             "Password must contain uppercase, lowercase, and number");
```

---

## ValidationRules API

### Constructor
```csharp
public class ValidationRules
{
    private List<IValidationRule<object>> _rules = new();
    
    public ValidationRules() { }
}
```

### String Rules

#### Required
```csharp
public ValidationRules Required(string message = "This field is required")
{
    _rules.Add(new RequiredRule<object> 
    { 
        ErrorMessage = message,
        PropertyName = "Field"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .Required("Username is required");
```

#### MinLength
```csharp
public ValidationRules MinLength(int length, string? message = null)
{
    _rules.Add(new MinLengthRule<string>
    {
        MinLength = length,
        ErrorMessage = message ?? $"Minimum length is {length} characters"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MinLength(3, "Name must be at least 3 characters");
```

#### MaxLength
```csharp
public ValidationRules MaxLength(int length, string? message = null)
{
    _rules.Add(new MaxLengthRule<string>
    {
        MaxLength = length,
        ErrorMessage = message ?? $"Maximum length is {length} characters"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MaxLength(50, "Description cannot exceed 50 characters");
```

#### Email
```csharp
public ValidationRules Email(string? message = null)
{
    _rules.Add(new EmailRule<string>
    {
        ErrorMessage = message ?? "Invalid email format"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .Email("Please provide a valid email address");
```

#### URL
```csharp
public ValidationRules Url(string? message = null)
{
    _rules.Add(new UrlRule<string>
    {
        ErrorMessage = message ?? "Invalid URL format"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .Url("Please enter a valid website URL");
```

#### Pattern (Regex)
```csharp
public ValidationRules Pattern(string pattern, string? message = null)
{
    _rules.Add(new PatternRule<string>
    {
        Pattern = pattern,
        ErrorMessage = message ?? "Invalid format"
    });
    return this;
}
```

**Example:**
```csharp
// Phone number
var rules = new ValidationRules()
    .Pattern(@"^\d{3}-\d{3}-\d{4}$", "Phone must be in format 123-456-7890");

// Postal code
var rules = new ValidationRules()
    .Pattern(@"^\d{5}(-\d{4})?$", "Invalid ZIP code");
```

### Numeric Rules

#### MinValue
```csharp
public ValidationRules MinValue(double min, string? message = null)
{
    _rules.Add(new MinValueRule<double>
    {
        MinValue = min,
        ErrorMessage = message ?? $"Value must be at least {min}"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MinValue(0, "Age cannot be negative");
```

#### MaxValue
```csharp
public ValidationRules MaxValue(double max, string? message = null)
{
    _rules.Add(new MaxValueRule<double>
    {
        MaxValue = max,
        ErrorMessage = message ?? $"Value cannot exceed {max}"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MaxValue(120, "Age cannot exceed 120");
```

#### Range
```csharp
public ValidationRules Range(double min, double max, string? message = null)
{
    return MinValue(min).MaxValue(max, message);
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .Range(18, 65, "Age must be between 18 and 65");
```

### Date Rules

#### MinDate
```csharp
public ValidationRules MinDate(DateTime min, string? message = null)
{
    _rules.Add(new MinDateRule<DateTime>
    {
        MinDate = min,
        ErrorMessage = message ?? $"Date must be after {min:yyyy-MM-dd}"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MinDate(DateTime.Today, "Date cannot be in the past");
```

#### MaxDate
```csharp
public ValidationRules MaxDate(DateTime max, string? message = null)
{
    _rules.Add(new MaxDateRule<DateTime>
    {
        MaxDate = max,
        ErrorMessage = message ?? $"Date must be before {max:yyyy-MM-dd}"
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .MaxDate(DateTime.Today.AddYears(1), "Date cannot be more than 1 year in future");
```

### Async Rules

#### Custom Async Validation
```csharp
public ValidationRules CustomAsync(
    Func<object, Task<bool>> validator, 
    string message)
{
    _rules.Add(new AsyncCustomRule<object>
    {
        Validator = validator,
        ErrorMessage = message
    });
    return this;
}
```

**Example:**
```csharp
var rules = new ValidationRules()
    .CustomAsync(async (value) =>
    {
        var username = value?.ToString();
        if (string.IsNullOrEmpty(username)) return true;
        
        var exists = await _userService.UsernameExistsAsync(username);
        return !exists; // Valid if username does NOT exist
    }, "Username is already taken");
```

---

## Schema Examples

### User Registration Form
```csharp
public class RegistrationFormModel
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public bool AgreeToTerms { get; set; }
}

public class RegistrationFormRules
{
    public ValidationRules UsernameRules => new ValidationRules()
        .Required("Username is required")
        .MinLength(3, "Username must be at least 3 characters")
        .MaxLength(20, "Username cannot exceed 20 characters")
        .Pattern(@"^[a-zA-Z0-9_]+$", "Username can only contain letters, numbers, and underscores")
        .CustomAsync(async (value) =>
        {
            var username = value?.ToString();
            return !await _userService.UsernameExistsAsync(username);
        }, "Username is already taken");
    
    public ValidationRules EmailRules => new ValidationRules()
        .Required("Email is required")
        .Email("Invalid email format")
        .CustomAsync(async (value) =>
        {
            var email = value?.ToString();
            return !await _userService.EmailExistsAsync(email);
        }, "Email is already registered");
    
    public ValidationRules PasswordRules => new ValidationRules()
        .Required("Password is required")
        .MinLength(8, "Password must be at least 8 characters")
        .Pattern(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])",
                "Password must contain uppercase, lowercase, number, and special character");
    
    public ValidationRules ConfirmPasswordRules => new ValidationRules()
        .Required("Please confirm your password")
        .Custom((value) =>
        {
            // Assumes access to Password field
            return value?.ToString() == Password;
        }, "Passwords do not match");
    
    public ValidationRules DateOfBirthRules => new ValidationRules()
        .Required("Date of birth is required")
        .MaxDate(DateTime.Today.AddYears(-18), "You must be at least 18 years old")
        .MinDate(DateTime.Today.AddYears(-120), "Invalid date of birth");
    
    public ValidationRules AgreeToTermsRules => new ValidationRules()
        .Custom((value) => value is bool b && b, "You must agree to the terms and conditions");
}
```

### Invoice Form
```csharp
public class InvoiceFormRules
{
    public ValidationRules InvoiceNumberRules => new ValidationRules()
        .Required("Invoice number is required")
        .Pattern(@"^INV-\d{6}$", "Invoice number must be in format INV-123456");
    
    public ValidationRules CustomerNameRules => new ValidationRules()
        .Required("Customer name is required")
        .MinLength(2, "Customer name must be at least 2 characters");
    
    public ValidationRules AmountRules => new ValidationRules()
        .Required("Amount is required")
        .MinValue(0.01, "Amount must be greater than zero")
        .MaxValue(999999.99, "Amount cannot exceed $999,999.99");
    
    public ValidationRules DueDateRules => new ValidationRules()
        .Required("Due date is required")
        .MinDate(DateTime.Today, "Due date cannot be in the past");
    
    public ValidationRules TaxIdRules => new ValidationRules()
        .Pattern(@"^\d{2}-\d{7}$", "Tax ID must be in format 12-3456789");
}
```

### Employee Profile Form
```csharp
public class EmployeeProfileRules
{
    public ValidationRules FirstNameRules => new ValidationRules()
        .Required("First name is required")
        .MinLength(2)
        .MaxLength(50)
        .Pattern(@"^[a-zA-Z\s'-]+$", "Name can only contain letters, spaces, hyphens, and apostrophes");
    
    public ValidationRules LastNameRules => new ValidationRules()
        .Required("Last name is required")
        .MinLength(2)
        .MaxLength(50)
        .Pattern(@"^[a-zA-Z\s'-]+$", "Name can only contain letters, spaces, hyphens, and apostrophes");
    
    public ValidationRules EmployeeIdRules => new ValidationRules()
        .Required("Employee ID is required")
        .Pattern(@"^EMP\d{6}$", "Employee ID must be in format EMP123456");
    
    public ValidationRules DepartmentRules => new ValidationRules()
        .Required("Department is required");
    
    public ValidationRules HireDateRules => new ValidationRules()
        .Required("Hire date is required")
        .MaxDate(DateTime.Today, "Hire date cannot be in the future")
        .MinDate(DateTime.Today.AddYears(-50), "Invalid hire date");
    
    public ValidationRules SalaryRules => new ValidationRules()
        .Required("Salary is required")
        .MinValue(0, "Salary cannot be negative")
        .MaxValue(10000000, "Salary exceeds maximum allowed");
    
    public ValidationRules PhoneRules => new ValidationRules()
        .Pattern(@"^\+?1?\d{10}$", "Phone number must be 10 digits")
        .When(() => !string.IsNullOrEmpty(Phone)); // Optional but validated if provided
    
    public ValidationRules EmailRules => new ValidationRules()
        .Required("Email is required")
        .Email()
        .Pattern(@"^[a-zA-Z0-9._%+-]+@company\.com$", "Must use company email domain");
}
```

---

## Migration from DataAnnotations

### Before (DataAnnotations)
```csharp
public class UserModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$")]
    public string Username { get; set; } = "";
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = "";
    
    [Required]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }
}

// Usage in Blazor
<DataAnnotationsValidator />
<ValidationSummary />
```

### After (ValidationRules)
```csharp
@code {
    private string username = "";
    private string email = "";
    private int age = 0;
    
    private ValidationRules usernameRules = new ValidationRules()
        .Required("Username is required")
        .MinLength(3)
        .MaxLength(20)
        .Pattern(@"^[a-zA-Z0-9_]+$");
    
    private ValidationRules emailRules = new ValidationRules()
        .Required()
        .Email("Invalid email");
    
    private ValidationRules ageRules = new ValidationRules()
        .Required()
        .Range(18, 120, "Age must be between 18 and 120");
}

// Usage in Razor
<N360TextBox @bind-Value="@username" ValidationRules="@usernameRules" />
<N360TextBox @bind-Value="@email" ValidationRules="@emailRules" />
<N360NumericTextBox @bind-Value="@age" ValidationRules="@ageRules" />
```

### Migration Guide

| DataAnnotations | ValidationRules | Notes |
|-----------------|-----------------|-------|
| `[Required]` | `.Required()` | Same behavior |
| `[StringLength(max, MinimumLength = min)]` | `.MinLength(min).MaxLength(max)` | Separate methods |
| `[EmailAddress]` | `.Email()` | Built-in email validation |
| `[Phone]` | `.Pattern(@"^\d{10}$")` | Custom regex |
| `[Url]` | `.Url()` | Built-in URL validation |
| `[RegularExpression]` | `.Pattern()` | Same regex support |
| `[Range(min, max)]` | `.Range(min, max)` | Same behavior |
| `[Compare]` | `.Custom()` | Custom comparison logic |
| `[CreditCard]` | `.Pattern()` | Custom Luhn validation |
| Custom attribute | `.CustomAsync()` | Async support |

---

## Advanced Patterns

### Conditional Validation
```csharp
public ValidationRules ShippingAddressRules => new ValidationRules()
    .When(() => RequiresShipping, rules => rules
        .Required("Shipping address is required when shipping is needed")
        .MinLength(10, "Please provide a complete address"));
```

### Dependent Field Validation
```csharp
public class PasswordChangeRules
{
    private string _newPassword = "";
    
    public ValidationRules NewPasswordRules => new ValidationRules()
        .Required("New password is required")
        .MinLength(8)
        .Pattern(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)", "Must contain upper, lower, and digit");
    
    public ValidationRules ConfirmPasswordRules => new ValidationRules()
        .Required("Please confirm password")
        .Custom((value) => value?.ToString() == _newPassword, "Passwords must match");
}
```

### Array/Collection Validation
```csharp
public ValidationRules TagsRules => new ValidationRules()
    .Custom((value) =>
    {
        if (value is List<string> tags)
        {
            return tags.Count >= 1 && tags.Count <= 5;
        }
        return false;
    }, "Please provide 1-5 tags")
    .Custom((value) =>
    {
        if (value is List<string> tags)
        {
            return tags.All(t => t.Length <= 20);
        }
        return true;
    }, "Each tag must be 20 characters or less");
```

### Multi-Field Cross-Validation
```csharp
public class DateRangeRules
{
    private DateTime _startDate;
    
    public ValidationRules StartDateRules => new ValidationRules()
        .Required("Start date is required");
    
    public ValidationRules EndDateRules => new ValidationRules()
        .Required("End date is required")
        .Custom((value) =>
        {
            if (value is DateTime endDate)
            {
                return endDate >= _startDate;
            }
            return true;
        }, "End date must be after start date");
}
```

### Severity Levels
```csharp
public ValidationRules UsernameRules => new ValidationRules()
    .Required("Username is required") // Error
    .MinLength(3, "Username should be at least 3 characters") // Error
    .MaxLength(20, "Username should not exceed 20 characters") // Error
    .Warning((value) =>
    {
        var username = value?.ToString();
        return !username?.Contains("admin", StringComparison.OrdinalIgnoreCase) ?? true;
    }, "Using 'admin' in username is not recommended") // Warning
    .Info((value) =>
    {
        var username = value?.ToString();
        return username?.Length >= 8;
    }, "Longer usernames are more secure"); // Info
```

---

## Server-Side Validation

### API Integration
```csharp
public class ServerValidationRules
{
    private readonly HttpClient _http;
    
    public ValidationRules EmailRules => new ValidationRules()
        .Required()
        .Email()
        .CustomAsync(async (value) =>
        {
            var email = value?.ToString();
            if (string.IsNullOrEmpty(email)) return true;
            
            var response = await _http.GetAsync($"/api/users/check-email?email={email}");
            var result = await response.Content.ReadFromJsonAsync<CheckEmailResult>();
            return result?.IsAvailable ?? false;
        }, "Email is already registered");
}
```

### Debounced Validation
```csharp
public class DebouncedValidation
{
    private Timer? _debounceTimer;
    
    public ValidationRules UsernameRules => new ValidationRules()
        .Required()
        .CustomAsync(async (value) =>
        {
            // Debounce for 500ms
            await Task.Delay(500);
            
            var username = value?.ToString();
            return await _userService.IsUsernameAvailableAsync(username);
        }, "Username is not available");
}
```

### Form-Level Validation
```csharp
public class FormValidator
{
    public async Task<ValidationResult> ValidateFormAsync(FormModel model)
    {
        var results = new List<ValidationResult>();
        
        // Field-level validation
        results.Add(await usernameRules.ValidateAsync(model.Username));
        results.Add(await emailRules.ValidateAsync(model.Email));
        results.Add(await passwordRules.ValidateAsync(model.Password));
        
        // Form-level cross-field validation
        if (model.Password != model.ConfirmPassword)
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Errors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        PropertyName = "ConfirmPassword",
                        Message = "Passwords do not match",
                        Severity = ValidationSeverity.Error
                    }
                }
            });
        }
        
        return CombineResults(results);
    }
}
```

---

## Custom Validators

### Create Custom Rule
```csharp
public class CreditCardRule : ValidationRule<string>
{
    public CreditCardRule()
    {
        ErrorMessage = "Invalid credit card number";
    }
    
    public override Task<ValidationResult> ValidateAsync(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Task.FromResult(new ValidationResult { IsValid = true });
        
        // Luhn algorithm
        var isValid = ValidateLuhn(value.Replace(" ", "").Replace("-", ""));
        
        return Task.FromResult(isValid 
            ? new ValidationResult { IsValid = true }
            : CreateError(value));
    }
    
    private bool ValidateLuhn(string cardNumber)
    {
        if (!long.TryParse(cardNumber, out _))
            return false;
        
        int sum = 0;
        bool alternate = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';
            
            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        return sum % 10 == 0;
    }
}

// Usage
public ValidationRules CreditCardRules => new ValidationRules()
    .Required("Credit card number is required")
    .AddRule(new CreditCardRule());
```

### Custom Async Rule
```csharp
public class UniqueValueRule<T> : ValidationRule<T>
{
    private readonly Func<T, Task<bool>> _checkUnique;
    
    public UniqueValueRule(Func<T, Task<bool>> checkUnique, string message)
    {
        _checkUnique = checkUnique;
        ErrorMessage = message;
    }
    
    public override async Task<ValidationResult> ValidateAsync(T value)
    {
        var isUnique = await _checkUnique(value);
        
        return isUnique
            ? new ValidationResult { IsValid = true }
            : CreateError(value);
    }
}

// Usage
public ValidationRules OrderIdRules => new ValidationRules()
    .Required()
    .AddRule(new UniqueValueRule<string>(
        async (orderId) => !await _orderService.ExistsAsync(orderId),
        "Order ID already exists"));
```

---

## JSON Schema (Future Enhancement)

### JSON Schema Definition
```json
{
  "type": "object",
  "properties": {
    "username": {
      "type": "string",
      "minLength": 3,
      "maxLength": 20,
      "pattern": "^[a-zA-Z0-9_]+$",
      "errorMessage": {
        "required": "Username is required",
        "minLength": "Username must be at least 3 characters",
        "pattern": "Username can only contain letters, numbers, and underscores"
      }
    },
    "email": {
      "type": "string",
      "format": "email",
      "errorMessage": "Invalid email address"
    },
    "age": {
      "type": "integer",
      "minimum": 18,
      "maximum": 120,
      "errorMessage": "Age must be between 18 and 120"
    }
  },
  "required": ["username", "email"]
}
```

### Load from JSON
```csharp
public class JsonSchemaLoader
{
    public ValidationRules LoadFromJson(string jsonSchema, string propertyName)
    {
        var schema = JsonSerializer.Deserialize<JsonSchema>(jsonSchema);
        var property = schema.Properties[propertyName];
        
        var rules = new ValidationRules();
        
        if (schema.Required.Contains(propertyName))
        {
            rules.Required(property.ErrorMessage?.Required ?? "This field is required");
        }
        
        if (property.MinLength.HasValue)
        {
            rules.MinLength(property.MinLength.Value, property.ErrorMessage?.MinLength);
        }
        
        if (property.MaxLength.HasValue)
        {
            rules.MaxLength(property.MaxLength.Value, property.ErrorMessage?.MaxLength);
        }
        
        if (!string.IsNullOrEmpty(property.Pattern))
        {
            rules.Pattern(property.Pattern, property.ErrorMessage?.Pattern);
        }
        
        return rules;
    }
}
```

---

## Best Practices

1. **Reusable Rules**: Define rules in a centralized location
2. **Meaningful Messages**: Provide clear, actionable error messages
3. **Client + Server**: Validate on both client and server
4. **Async Sparingly**: Use async validation only when necessary (network calls)
5. **Debounce**: Debounce expensive async validations
6. **Consistent Format**: Use consistent error message formatting
7. **Localization**: Support i18n for error messages
8. **Performance**: Cache validation results when possible
9. **Testing**: Unit test complex validation logic
10. **Documentation**: Document custom validation rules

## References
- [Yup (JavaScript)](https://github.com/jquense/yup)
- [Zod (TypeScript)](https://github.com/colinhacks/zod)
- [FluentValidation (.NET)](https://github.com/FluentValidation/FluentValidation)
- [DataAnnotations (Microsoft)](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
