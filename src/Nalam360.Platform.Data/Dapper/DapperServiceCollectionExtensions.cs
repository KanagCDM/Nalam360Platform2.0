using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nalam360.Platform.Data.Dapper;

/// <summary>
/// Extension methods for registering Dapper services.
/// </summary>
public static class DapperServiceCollectionExtensions
{
    /// <summary>
    /// Add Dapper repository with connection string.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDapperRepository<TEntity>(
        this IServiceCollection services,
        string connectionString)
        where TEntity : class
    {
        services.TryAddTransient<IDapperRepository<TEntity>>(sp => 
            new DapperRepository<TEntity>(connectionString));

        return services;
    }

    /// <summary>
    /// Add Dapper repository with connection factory.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="connectionFactory">Factory to create database connections</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDapperRepository<TEntity>(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnection> connectionFactory)
        where TEntity : class
    {
        services.TryAddTransient<IDapperRepository<TEntity>>(sp =>
        {
            var connection = connectionFactory(sp);
            return new DapperRepository<TEntity>(connection);
        });

        return services;
    }

    /// <summary>
    /// Add custom Dapper repository implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TRepository">Repository implementation type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDapperRepository<TEntity, TRepository>(
        this IServiceCollection services)
        where TEntity : class
        where TRepository : class, IDapperRepository<TEntity>
    {
        services.TryAddTransient<IDapperRepository<TEntity>, TRepository>();
        return services;
    }

    /// <summary>
    /// Add Dapper connection factory for dependency injection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDapperConnection(
        this IServiceCollection services,
        string connectionString)
    {
        services.TryAddTransient<IDbConnection>(sp => new SqlConnection(connectionString));
        return services;
    }

    /// <summary>
    /// Add Dapper connection factory with custom connection type.
    /// </summary>
    /// <typeparam name="TConnection">Connection type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="connectionFactory">Factory to create connections</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDapperConnection<TConnection>(
        this IServiceCollection services,
        Func<IServiceProvider, TConnection> connectionFactory)
        where TConnection : class, IDbConnection
    {
        services.TryAddTransient<IDbConnection>(connectionFactory);
        return services;
    }
}
