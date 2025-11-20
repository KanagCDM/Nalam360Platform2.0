using System.Data;
using Dapper;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.Dapper;

/// <summary>
/// Dapper-based repository for high-performance data access.
/// Use for read-heavy scenarios, reporting, and bulk operations where EF Core overhead is unnecessary.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IDapperRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Execute a query and return multiple results.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="sql">SQL query</param>
    /// <param name="param">Query parameters</param>
    /// <param name="transaction">Database transaction (optional)</param>
    /// <param name="commandTimeout">Command timeout in seconds</param>
    /// <param name="commandType">Command type (Text, StoredProcedure)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query results</returns>
    Task<Result<IEnumerable<T>>> QueryAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a query and return the first result or default.
    /// </summary>
    Task<Result<T?>> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a query and return a single result or default.
    /// </summary>
    Task<Result<T?>> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a command (INSERT, UPDATE, DELETE) and return rows affected.
    /// </summary>
    Task<Result<int>> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a command and return a scalar value.
    /// </summary>
    Task<Result<T?>> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute multiple queries and return multiple result sets.
    /// </summary>
    Task<Result<TResult>> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, TResult> map,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk insert entities using efficient batch processing.
    /// </summary>
    Task<Result<int>> BulkInsertAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update entities using efficient batch processing.
    /// </summary>
    Task<Result<int>> BulkUpdateAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk delete entities using efficient batch processing.
    /// </summary>
    Task<Result<int>> BulkDeleteAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure.
    /// </summary>
    Task<Result<IEnumerable<T>>> ExecuteStoredProcedureAsync<T>(
        string procedureName,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure with output parameters.
    /// </summary>
    Task<Result<(IEnumerable<T> Results, DynamicParameters OutputParams)>> ExecuteStoredProcedureWithOutputAsync<T>(
        string procedureName,
        DynamicParameters parameters,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);
}
