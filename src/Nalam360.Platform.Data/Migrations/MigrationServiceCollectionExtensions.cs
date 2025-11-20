using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nalam360.Platform.Core.Time;

namespace Nalam360.Platform.Data.Migrations;

/// <summary>
/// Extension methods for registering migration services.
/// </summary>
public static class MigrationServiceCollectionExtensions
{
    /// <summary>
    /// Add database migration services with file-based repository.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="migrationsDirectory">Directory to store migration files</param>
    /// <param name="appliedBy">Name of the user/system applying migrations (default: "System")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseMigrations(
        this IServiceCollection services,
        string migrationsDirectory,
        string appliedBy = "System")
    {
        services.TryAddSingleton<IMigrationRepository>(sp =>
            new FileMigrationRepository(migrationsDirectory));

        services.TryAddScoped<IMigrationGenerator>(sp =>
        {
            var context = sp.GetRequiredService<DbContext>();
            return new EfCoreMigrationGenerator(context);
        });

        services.TryAddScoped<IMigrationService>(sp =>
        {
            var context = sp.GetRequiredService<DbContext>();
            var repository = sp.GetRequiredService<IMigrationRepository>();
            var generator = sp.GetRequiredService<IMigrationGenerator>();
            var timeProvider = sp.GetRequiredService<ITimeProvider>();
            
            return new MigrationService(context, repository, generator, timeProvider, appliedBy);
        });

        return services;
    }

    /// <summary>
    /// Add database migration services with custom repository.
    /// </summary>
    /// <typeparam name="TRepository">Custom migration repository type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="appliedBy">Name of the user/system applying migrations (default: "System")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseMigrations<TRepository>(
        this IServiceCollection services,
        string appliedBy = "System")
        where TRepository : class, IMigrationRepository
    {
        services.TryAddSingleton<IMigrationRepository, TRepository>();

        services.TryAddScoped<IMigrationGenerator>(sp =>
        {
            var context = sp.GetRequiredService<DbContext>();
            return new EfCoreMigrationGenerator(context);
        });

        services.TryAddScoped<IMigrationService>(sp =>
        {
            var context = sp.GetRequiredService<DbContext>();
            var repository = sp.GetRequiredService<IMigrationRepository>();
            var generator = sp.GetRequiredService<IMigrationGenerator>();
            var timeProvider = sp.GetRequiredService<ITimeProvider>();
            
            return new MigrationService(context, repository, generator, timeProvider, appliedBy);
        });

        return services;
    }

    /// <summary>
    /// Add database migration services with custom implementations.
    /// </summary>
    /// <typeparam name="TMigrationService">Migration service implementation</typeparam>
    /// <typeparam name="TMigrationRepository">Migration repository implementation</typeparam>
    /// <typeparam name="TMigrationGenerator">Migration generator implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseMigrations<TMigrationService, TMigrationRepository, TMigrationGenerator>(
        this IServiceCollection services)
        where TMigrationService : class, IMigrationService
        where TMigrationRepository : class, IMigrationRepository
        where TMigrationGenerator : class, IMigrationGenerator
    {
        services.TryAddSingleton<IMigrationRepository, TMigrationRepository>();
        services.TryAddScoped<IMigrationGenerator, TMigrationGenerator>();
        services.TryAddScoped<IMigrationService, TMigrationService>();

        return services;
    }
}
