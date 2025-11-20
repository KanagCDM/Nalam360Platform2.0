using System.ComponentModel.DataAnnotations;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Validation;

/// <summary>
/// Service for validating objects using data annotation attributes.
/// </summary>
public interface IAttributeValidationService
{
    /// <summary>
    /// Validates an object using data annotation attributes.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A Result containing validation errors if any.</returns>
    Result Validate(object instance);

    /// <summary>
    /// Validates an object using data annotation attributes.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A Result containing validation errors if any.</returns>
    Result<T> Validate<T>(T instance) where T : notnull;

    /// <summary>
    /// Validates a specific property of an object.
    /// </summary>
    /// <param name="instance">The object containing the property.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns>A Result containing validation errors if any.</returns>
    Result ValidateProperty(object instance, string propertyName);

    /// <summary>
    /// Validates an object and throws a ValidationException if validation fails.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    void ValidateAndThrow(object instance);
}

/// <summary>
/// Default implementation of IAttributeValidationService.
/// </summary>
public sealed class AttributeValidationService : IAttributeValidationService
{
    public Result Validate(object instance)
    {
        if (instance == null)
            return Result.Failure(Error.Validation("The instance to validate cannot be null."));

        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (isValid)
            return Result.Success();

        var errorMessage = string.Join("; ", validationResults.Select(vr =>
            $"{vr.MemberNames.FirstOrDefault() ?? "Unknown"}: {vr.ErrorMessage ?? "Validation failed"}"));

        return Result.Failure(Error.Validation(errorMessage));
    }

    public Result<T> Validate<T>(T instance) where T : notnull
    {
        if (instance == null)
            return Result<T>.Failure(Error.Validation("The instance to validate cannot be null."));

        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (isValid)
            return Result<T>.Success(instance);

        var errorMessage = string.Join("; ", validationResults.Select(vr =>
            $"{vr.MemberNames.FirstOrDefault() ?? "Unknown"}: {vr.ErrorMessage ?? "Validation failed"}"));

        return Result<T>.Failure(Error.Validation(errorMessage));
    }

    public Result ValidateProperty(object instance, string propertyName)
    {
        if (instance == null)
            return Result.Failure(Error.Validation("The instance to validate cannot be null."));

        if (string.IsNullOrWhiteSpace(propertyName))
            return Result.Failure(Error.Validation("The property name cannot be null or empty."));

        var propertyInfo = instance.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
            return Result.Failure(Error.Validation($"Property '{propertyName}' not found on type '{instance.GetType().Name}'."));

        var propertyValue = propertyInfo.GetValue(instance);
        var validationContext = new ValidationContext(instance) { MemberName = propertyName };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(
            propertyValue,
            validationContext,
            validationResults);

        if (isValid)
            return Result.Success();

        var errorMessage = string.Join("; ", validationResults.Select(vr =>
            $"{propertyName}: {vr.ErrorMessage ?? "Validation failed"}"));

        return Result.Failure(Error.Validation(errorMessage));
    }

    public void ValidateAndThrow(object instance)
    {
        if (instance == null)
            throw new ValidationException("The instance to validate cannot be null.");

        var validationContext = new ValidationContext(instance);
        Validator.ValidateObject(instance, validationContext, validateAllProperties: true);
    }
}
