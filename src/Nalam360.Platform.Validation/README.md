# Nalam360.Platform.Validation

Comprehensive validation infrastructure for the Nalam360 Enterprise Platform, combining FluentValidation for complex business rules with attribute-based validation for declarative model validation.

## Features

- ✅ **FluentValidation Integration**: Complex, fluent validation rules
- ✅ **Attribute-Based Validation**: 10 custom validation attributes for declarative validation
- ✅ **Result Pattern**: Returns `Result<T>` instead of throwing exceptions
- ✅ **ModelState Support**: MVC-style model state dictionary for UI integration
- ✅ **Property-Level Validation**: Validate individual properties or entire models
- ✅ **Custom Error Messages**: Fully customizable error messages

## Installation

```bash
dotnet add package Nalam360.Platform.Validation
```

## Configuration

```csharp
using Nalam360.Platform.Validation.DependencyInjection;

// Startup.cs or Program.cs
builder.Services.AddAttributeValidation();
```

## Custom Validation Attributes

### 1. AlphanumericAttribute
Validates that a string contains only alphanumeric characters.

```csharp
public class ProductModel
{
    [Alphanumeric]
    public string Sku { get; set; }  // Only letters and numbers
    
    [Alphanumeric(allowSpaces: true)]
    public string ProductName { get; set; }  // Letters, numbers, and spaces
}
```

### 2. StrongPasswordAttribute
Validates password strength (uppercase, lowercase, digit, special character).

```csharp
public class ChangePasswordCommand
{
    [StrongPassword(MinimumLength = 12)]
    public string NewPassword { get; set; }
}
```

**Requirements:**
- Minimum length (default 8, configurable)
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

### 3. EmailDomainAttribute
Validates email address with optional domain restrictions.

```csharp
public class RegisterUserCommand
{
    // Allow only company email addresses
    [EmailDomain("company.com", "subsidiary.com")]
    public string Email { get; set; }
    
    // Any valid email
    [EmailDomain]
    public string ContactEmail { get; set; }
}
```

### 4. NumericRangeAttribute
Validates numeric values within a specified range.

```csharp
public class OrderModel
{
    [NumericRange(1, 100)]
    public int Quantity { get; set; }
    
    [NumericRange(0.01, 99999.99)]
    public decimal Price { get; set; }
}
```

### 5. FutureDateAttribute
Validates that a date is not in the past.

```csharp
public class ScheduleEventCommand
{
    [FutureDate(allowToday: true)]
    public DateTime StartDate { get; set; }
    
    [FutureDate(allowToday: false)]
    public DateTime EndDate { get; set; }
}
```

### 6. PastDateAttribute
Validates that a date is not in the future.

```csharp
public class EmployeeModel
{
    [PastDate]
    public DateTime DateOfBirth { get; set; }
    
    [PastDate(allowToday: true)]
    public DateTime HireDate { get; set; }
}
```

### 7. MinimumCountAttribute
Validates that a collection contains at least a minimum number of items.

```csharp
public class CreateOrderCommand
{
    [MinimumCount(1)]
    public List<OrderItem> Items { get; set; }
    
    [MinimumCount(3)]
    public string[] Tags { get; set; }
}
```

### 8. MaximumCountAttribute
Validates that a collection contains at most a maximum number of items.

```csharp
public class UserPreferencesModel
{
    [MaximumCount(5)]
    public List<string> FavoriteCategories { get; set; }
}
```

### 9. RegexPatternAttribute
Validates that a string matches a specific regex pattern.

```csharp
public class PhoneNumberModel
{
    [RegexPattern(@"^\+?[1-9]\d{1,14}$")]  // E.164 format
    public string PhoneNumber { get; set; }
    
    [RegexPattern(@"^[A-Z]{2}\d{6}$", RegexOptions.None)]  // Custom format
    public string CustomerId { get; set; }
}
```

### 10. ComparePropertyAttribute
Validates that a property value matches another property (e.g., password confirmation).

```csharp
public class RegisterUserCommand
{
    public string Password { get; set; }
    
    [CompareProperty(nameof(Password))]
    public string ConfirmPassword { get; set; }
}
```

## IAttributeValidationService

### Validate Entire Object
```csharp
public class UserRegistrationService
{
    private readonly IAttributeValidationService _validationService;
    
    public UserRegistrationService(IAttributeValidationService validationService)
    {
        _validationService = validationService;
    }
    
    public async Task<Result<User>> RegisterUserAsync(RegisterUserCommand command)
    {
        // Validate using Result pattern
        var validationResult = _validationService.Validate(command);
        if (validationResult.IsFailure)
            return Result<User>.Failure(validationResult.Error);
        
        // Or validate and get validated object
        var result = _validationService.Validate(command);
        if (result.IsFailure)
            return Result<User>.Failure(result.Error);
        
        // Proceed with registration...
        var user = new User { Email = command.Email };
        return Result<User>.Success(user);
    }
}
```

### Validate Single Property
```csharp
public async Task<Result> ValidateEmailAsync(string userId, string newEmail)
{
    var user = new User { Email = newEmail };
    
    // Validate only the Email property
    var result = _validationService.ValidateProperty(user, nameof(User.Email));
    
    if (result.IsFailure)
        return Result.Failure(result.Error);
    
    // Update email...
    return Result.Success();
}
```

### Validate and Throw
```csharp
public void ProcessPayment(PaymentCommand command)
{
    // Throws ValidationException if validation fails
    _validationService.ValidateAndThrow(command);
    
    // Process payment...
}
```

## ModelState Integration

The `ModelStateDictionary` provides MVC-style validation state management.

### Basic Usage
```csharp
using Nalam360.Platform.Validation.ModelState;

public class UserController
{
    public IActionResult Register(RegisterUserCommand command)
    {
        var modelState = new ModelStateDictionary();
        
        if (!modelState.TryValidate(command))
        {
            // Validation failed
            var errors = modelState.Errors;  // List of ModelStateError
            return BadRequest(errors);
        }
        
        // Validation succeeded...
        return Ok();
    }
}
```

### Property-Level Validation
```csharp
public IActionResult UpdateEmail(string newEmail)
{
    var user = new User { Email = newEmail };
    var modelState = new ModelStateDictionary();
    
    if (!modelState.TryValidateProperty(user, nameof(User.Email)))
    {
        var emailErrors = modelState.GetErrors(nameof(User.Email));
        return BadRequest(new { Email = emailErrors });
    }
    
    return Ok();
}
```

### Error Handling
```csharp
var modelState = new ModelStateDictionary();
modelState.TryValidate(command);

if (!modelState.IsValid)
{
    // Get all errors
    var allErrors = modelState.Errors;
    
    // Get errors for specific property
    var emailErrors = modelState.GetErrors("Email");
    
    // Convert to dictionary
    var errorDict = modelState.ToDictionary();
    // { "Email": ["Invalid email address"], "Password": ["Password too weak"] }
    
    // Get formatted message
    var message = modelState.GetErrorMessage();
    // "Email: Invalid email address; Password: Password too weak"
    
    // Clear all errors
    modelState.Clear();
    
    // Clear specific property errors
    modelState.ClearProperty("Email");
}
```

## Complete Example

```csharp
using System.ComponentModel.DataAnnotations;
using Nalam360.Platform.Validation.Attributes;

public class CreateUserCommand
{
    [Required]
    [Alphanumeric(allowSpaces: true)]
    public string FullName { get; set; }
    
    [Required]
    [EmailDomain("company.com")]
    public string Email { get; set; }
    
    [Required]
    [StrongPassword(MinimumLength = 12)]
    public string Password { get; set; }
    
    [Required]
    [CompareProperty(nameof(Password))]
    public string ConfirmPassword { get; set; }
    
    [FutureDate(allowToday: false)]
    public DateTime? SubscriptionEndDate { get; set; }
    
    [MinimumCount(1)]
    [MaximumCount(5)]
    public List<string> Roles { get; set; }
    
    [RegexPattern(@"^\+[1-9]\d{1,14}$")]  // E.164 phone format
    public string? PhoneNumber { get; set; }
}

public class UserService
{
    private readonly IAttributeValidationService _validationService;
    
    public UserService(IAttributeValidationService validationService)
    {
        _validationService = validationService;
    }
    
    public async Task<Result<Guid>> CreateUserAsync(CreateUserCommand command)
    {
        // Validate command
        var validationResult = _validationService.Validate(command);
        if (validationResult.IsFailure)
        {
            // Returns Result with validation error
            // Error.Code = "VALIDATION_ERROR"
            // Error.Message = "FullName: Must contain only alphanumeric characters and spaces; ..."
            return Result<Guid>.Failure(validationResult.Error);
        }
        
        // All validations passed, create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = command.FullName,
            Email = command.Email,
            PasswordHash = HashPassword(command.Password),
            Roles = command.Roles,
            PhoneNumber = command.PhoneNumber
        };
        
        // Save to database...
        return Result<Guid>.Success(user.Id);
    }
}
```

## Integration with FluentValidation

Combine attribute-based validation with FluentValidation for complex scenarios:

```csharp
// Attributes for simple, declarative rules
public class CreateOrderCommand
{
    [Required, EmailDomain("company.com")]
    public string CustomerEmail { get; set; }
    
    [NumericRange(1, int.MaxValue)]
    public int CustomerId { get; set; }
    
    [MinimumCount(1)]
    public List<OrderItem> Items { get; set; }
}

// FluentValidation for complex business rules
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    private readonly ICustomerRepository _customerRepository;
    
    public CreateOrderValidator(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
        
        RuleFor(x => x.CustomerId)
            .MustAsync(CustomerExists)
            .WithMessage("Customer does not exist");
        
        RuleFor(x => x.Items)
            .Must(items => items.Sum(i => i.Quantity) <= 1000)
            .WithMessage("Total quantity cannot exceed 1000");
    }
    
    private async Task<bool> CustomerExists(int customerId, CancellationToken ct)
    {
        return await _customerRepository.ExistsAsync(customerId, ct);
    }
}
```

## Best Practices

1. **Use Attributes for Simple Rules**: Data format, ranges, required fields
2. **Use FluentValidation for Complex Rules**: Database lookups, cross-property validation
3. **Return Result Pattern**: Avoid exceptions for validation failures
4. **Property-Level Validation**: For real-time UI feedback
5. **Custom Error Messages**: Override default messages for better UX
6. **ModelState for UI**: Use `ModelStateDictionary` in web applications

## Error Handling

```csharp
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    var result = await _userService.CreateUserAsync(command);
    
    return result.Match(
        userId => Ok(new { UserId = userId }),
        error => 
        {
            if (error.Code == "VALIDATION_ERROR")
            {
                // Parse validation message and return structured errors
                var errors = ParseValidationMessage(error.Message);
                return BadRequest(new { Errors = errors });
            }
            return StatusCode(500, error.Message);
        });
}
```

## Testing

```csharp
public class ValidationAttributeTests
{
    private readonly IAttributeValidationService _validationService;
    
    public ValidationAttributeTests()
    {
        _validationService = new AttributeValidationService();
    }
    
    [Fact]
    public void StrongPassword_WithWeakPassword_ShouldFail()
    {
        var model = new TestModel { Password = "weak" };
        
        var result = _validationService.Validate(model);
        
        Assert.False(result.IsSuccess);
        Assert.Contains("Password", result.Error.Message);
    }
    
    [Fact]
    public void EmailDomain_WithInvalidDomain_ShouldFail()
    {
        var model = new TestModel { Email = "user@wrongdomain.com" };
        
        var result = _validationService.ValidateProperty(model, nameof(TestModel.Email));
        
        Assert.False(result.IsSuccess);
    }
}

public class TestModel
{
    [StrongPassword(MinimumLength = 8)]
    public string Password { get; set; }
    
    [EmailDomain("company.com")]
    public string Email { get; set; }
}
```

## Performance Considerations

- **Attribute validation is synchronous**: Use for fast, local rules
- **FluentValidation supports async**: Use for database lookups
- **Cache compiled validators**: FluentValidation caches internally
- **Property-level validation**: More efficient for real-time UI feedback

## Related Documentation

- [Platform.Core Results](../Nalam360.Platform.Core/README.md#result-pattern)
- [Platform.Application Validation Behavior](../Nalam360.Platform.Application/README.md#validation)
- [FluentValidation Official Docs](https://docs.fluentvalidation.net/)

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/nalam360/platform).
