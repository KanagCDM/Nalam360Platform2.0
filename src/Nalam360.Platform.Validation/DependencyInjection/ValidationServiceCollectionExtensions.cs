using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nalam360.Platform.Validation.DependencyInjection;

/// <summary>
/// Extension methods for registering validation services.
/// </summary>
public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Adds attribute-based validation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAttributeValidation(this IServiceCollection services)
    {
        services.TryAddSingleton<IAttributeValidationService, AttributeValidationService>();
        return services;
    }
}
