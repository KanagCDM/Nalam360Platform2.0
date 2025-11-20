using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nalam360.Platform.Documentation.Parsers;

namespace Nalam360.Platform.Documentation.DependencyInjection;

/// <summary>
/// Extension methods for registering documentation services.
/// </summary>
public static class DocumentationServiceCollectionExtensions
{
    /// <summary>
    /// Adds documentation generation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDocumentationGeneration(this IServiceCollection services)
    {
        services.TryAddSingleton<IDocumentationParser, XmlDocumentationParser>();
        services.TryAddSingleton<IDocumentationService, DocumentationService>();
        return services;
    }
}
