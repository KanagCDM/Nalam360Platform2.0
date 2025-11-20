using System.ComponentModel.DataAnnotations;

namespace Nalam360.Platform.Validation.ModelState;

/// <summary>
/// Represents a validation error in model state.
/// </summary>
public sealed record ModelStateError(string PropertyName, string ErrorMessage);

/// <summary>
/// Represents the validation state of a model.
/// </summary>
public sealed class ModelStateDictionary
{
    private readonly Dictionary<string, List<string>> _errors = new();

    /// <summary>
    /// Gets whether the model state is valid (no errors).
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Gets all validation errors.
    /// </summary>
    public IReadOnlyCollection<ModelStateError> Errors =>
        _errors.SelectMany(kvp => kvp.Value.Select(msg => new ModelStateError(kvp.Key, msg)))
            .ToList();

    /// <summary>
    /// Adds an error for a specific property.
    /// </summary>
    public void AddError(string propertyName, string errorMessage)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();

        _errors[propertyName].Add(errorMessage);
    }

    /// <summary>
    /// Adds multiple errors from validation results.
    /// </summary>
    public void AddErrors(IEnumerable<ValidationResult> validationResults)
    {
        foreach (var result in validationResults)
        {
            var propertyName = result.MemberNames.FirstOrDefault() ?? "Unknown";
            AddError(propertyName, result.ErrorMessage ?? "Validation failed");
        }
    }

    /// <summary>
    /// Gets errors for a specific property.
    /// </summary>
    public IReadOnlyCollection<string> GetErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out var errors) ? errors : Array.Empty<string>();
    }

    /// <summary>
    /// Clears all errors.
    /// </summary>
    public void Clear()
    {
        _errors.Clear();
    }

    /// <summary>
    /// Clears errors for a specific property.
    /// </summary>
    public void ClearProperty(string propertyName)
    {
        _errors.Remove(propertyName);
    }
}

/// <summary>
/// Extension methods for ModelStateDictionary.
/// </summary>
public static class ModelStateDictionaryExtensions
{
    /// <summary>
    /// Validates an object and populates the ModelStateDictionary with errors.
    /// </summary>
    public static bool TryValidate(this ModelStateDictionary modelState, object instance)
    {
        if (instance == null)
        {
            modelState.AddError("Instance", "The instance to validate cannot be null.");
            return false;
        }

        modelState.Clear();

        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (!isValid)
            modelState.AddErrors(validationResults);

        return isValid;
    }

    /// <summary>
    /// Validates a specific property and updates the ModelStateDictionary.
    /// </summary>
    public static bool TryValidateProperty(this ModelStateDictionary modelState, object instance, string propertyName)
    {
        if (instance == null)
        {
            modelState.AddError("Instance", "The instance to validate cannot be null.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            modelState.AddError("PropertyName", "The property name cannot be null or empty.");
            return false;
        }

        modelState.ClearProperty(propertyName);

        var propertyInfo = instance.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            modelState.AddError(propertyName, $"Property '{propertyName}' not found.");
            return false;
        }

        var propertyValue = propertyInfo.GetValue(instance);
        var validationContext = new ValidationContext(instance) { MemberName = propertyName };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(
            propertyValue,
            validationContext,
            validationResults);

        if (!isValid)
        {
            foreach (var result in validationResults)
            {
                modelState.AddError(propertyName, result.ErrorMessage ?? "Validation failed");
            }
        }

        return isValid;
    }

    /// <summary>
    /// Converts ModelStateDictionary errors to a dictionary format.
    /// </summary>
    public static Dictionary<string, string[]> ToDictionary(this ModelStateDictionary modelState)
    {
        return modelState.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }

    /// <summary>
    /// Gets a formatted error message containing all validation errors.
    /// </summary>
    public static string GetErrorMessage(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return string.Empty;

        return string.Join("; ", modelState.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
    }
}
