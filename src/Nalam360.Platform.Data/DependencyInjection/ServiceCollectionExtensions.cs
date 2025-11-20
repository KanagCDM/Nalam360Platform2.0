using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Data.EntityFramework;
using Nalam360.Platform.Data.Repositories;
using Nalam360.Platform.Data.UnitOfWork;

namespace Nalam360.Platform.Data.DependencyInjection;

/// <summary>
/// Extension methods for registering data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Platform.Data services to the service collection.
    /// </summary>
    public static IServiceCollection AddPlatformData(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        return services;
    }

    /// <summary>
    /// Adds Platform.Data services with a specific DbContext.
    /// </summary>
    public static IServiceCollection AddPlatformData<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddPlatformData();
        services.AddScoped<IUnitOfWork>(sp => new EfUnitOfWork(sp.GetRequiredService<TContext>()));

        return services;
    }
}
