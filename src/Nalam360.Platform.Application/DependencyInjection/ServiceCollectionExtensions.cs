using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nalam360.Platform.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application layer services including mediator and handlers
    /// </summary>
    public static IServiceCollection AddPlatformApplication(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Register mediator
        services.AddScoped<Messaging.IMediator, Messaging.Mediator>();

        // Register all request handlers
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(Messaging.IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Adds FluentValidation validators from the specified assembly
    /// </summary>
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(FluentValidation.IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
