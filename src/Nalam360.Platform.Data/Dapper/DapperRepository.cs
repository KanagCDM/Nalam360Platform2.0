using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.Dapper;

/// <summary>
/// Base implementation of IDapperRepository using Dapper for high-performance data access.
/// </summary>
public class DapperRepository<TEntity> : IDapperRepository<TEntity> where TEntity : class
{
    private readonly string _connectionString;
    private readonly IDbConnection _connection;

    public DapperRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _connection = new SqlConnection(_connectionString);
    }

    public DapperRepository(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _connectionString = connection.ConnectionString ?? string.Empty;
    }

    public virtual async Task<Result<IEnumerable<T>>> QueryAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var results = await _connection.QueryAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            return Result<IEnumerable<T>>.Success(results);
        }
        catch (SqlException ex)
        {
            return Result<IEnumerable<T>>.Failure(Error.Database("SQL_QUERY_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(Error.Unexpected("DAPPER_QUERY_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<T?>> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var result = await _connection.QueryFirstOrDefaultAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            return Result<T?>.Success(result);
        }
        catch (SqlException ex)
        {
            return Result<T?>.Failure(Error.Database("SQL_QUERY_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<T?>.Failure(Error.Unexpected("DAPPER_QUERY_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<T?>> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var result = await _connection.QuerySingleOrDefaultAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            return Result<T?>.Success(result);
        }
        catch (SqlException ex)
        {
            return Result<T?>.Failure(Error.Database("SQL_QUERY_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<T?>.Failure(Error.Unexpected("DAPPER_QUERY_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<int>> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var rowsAffected = await _connection.ExecuteAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            return Result<int>.Success(rowsAffected);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(Error.Database("SQL_EXECUTE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Unexpected("DAPPER_EXECUTE_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<T?>> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var result = await _connection.ExecuteScalarAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            return Result<T?>.Success(result);
        }
        catch (SqlException ex)
        {
            return Result<T?>.Failure(Error.Database("SQL_SCALAR_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<T?>.Failure(Error.Unexpected("DAPPER_SCALAR_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<TResult>> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, TResult> map,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            using var gridReader = await _connection.QueryMultipleAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            var result = map(gridReader);
            return Result<TResult>.Success(result);
        }
        catch (SqlException ex)
        {
            return Result<TResult>.Failure(Error.Database("SQL_MULTIPLE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(Error.Unexpected("DAPPER_MULTIPLE_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<int>> BulkInsertAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!entities.Any())
            return Result<int>.Success(0);

        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var tableName = GetTableName();
            var properties = GetInsertableProperties();
            var columnNames = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames})";

            var rowsAffected = await _connection.ExecuteAsync(
                sql,
                entities,
                transaction,
                commandTimeout);

            return Result<int>.Success(rowsAffected);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(Error.Database("BULK_INSERT_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Unexpected("BULK_INSERT_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<int>> BulkUpdateAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!entities.Any())
            return Result<int>.Success(0);

        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var tableName = GetTableName();
            var properties = GetUpdatableProperties();
            var keyProperty = GetKeyProperty();

            var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));
            var sql = $"UPDATE {tableName} SET {setClause} WHERE [{keyProperty.Name}] = @{keyProperty.Name}";

            var rowsAffected = await _connection.ExecuteAsync(
                sql,
                entities,
                transaction,
                commandTimeout);

            return Result<int>.Success(rowsAffected);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(Error.Database("BULK_UPDATE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Unexpected("BULK_UPDATE_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<int>> BulkDeleteAsync(
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!entities.Any())
            return Result<int>.Success(0);

        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var tableName = GetTableName();
            var keyProperty = GetKeyProperty();
            var keyValues = entities.Select(e => keyProperty.GetValue(e)).ToList();

            var sql = $"DELETE FROM {tableName} WHERE [{keyProperty.Name}] IN @Keys";

            var rowsAffected = await _connection.ExecuteAsync(
                sql,
                new { Keys = keyValues },
                transaction,
                commandTimeout);

            return Result<int>.Success(rowsAffected);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(Error.Database("BULK_DELETE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Unexpected("BULK_DELETE_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<IEnumerable<T>>> ExecuteStoredProcedureAsync<T>(
        string procedureName,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var results = await _connection.QueryAsync<T>(
                procedureName,
                param,
                transaction,
                commandTimeout,
                CommandType.StoredProcedure);

            return Result<IEnumerable<T>>.Success(results);
        }
        catch (SqlException ex)
        {
            return Result<IEnumerable<T>>.Failure(Error.Database("STORED_PROCEDURE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(Error.Unexpected("STORED_PROCEDURE_ERROR", ex.Message));
        }
    }

    public virtual async Task<Result<(IEnumerable<T> Results, DynamicParameters OutputParams)>> ExecuteStoredProcedureWithOutputAsync<T>(
        string procedureName,
        DynamicParameters parameters,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var results = await _connection.QueryAsync<T>(
                procedureName,
                parameters,
                transaction,
                commandTimeout,
                CommandType.StoredProcedure);

            return Result<(IEnumerable<T>, DynamicParameters)>.Success((results, parameters));
        }
        catch (SqlException ex)
        {
            return Result<(IEnumerable<T>, DynamicParameters)>.Failure(Error.Database("STORED_PROCEDURE_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<T>, DynamicParameters)>.Failure(Error.Unexpected("STORED_PROCEDURE_ERROR", ex.Message));
        }
    }

    protected virtual async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                await sqlConnection.OpenAsync(cancellationToken);
            }
            else
            {
                _connection.Open();
            }
        }
    }

    protected virtual string GetTableName()
    {
        var type = typeof(TEntity);
        
        // Check for [Table] attribute
        var tableAttr = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
        if (tableAttr != null)
        {
            var schema = !string.IsNullOrEmpty(tableAttr.Schema) ? $"[{tableAttr.Schema}]." : "";
            return $"{schema}[{tableAttr.Name}]";
        }

        // Default to class name with "dbo" schema
        return $"[dbo].[{type.Name}]";
    }

    protected virtual PropertyInfo GetKeyProperty()
    {
        var type = typeof(TEntity);
        
        // Check for [Key] attribute
        var keyProperty = type.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null);

        if (keyProperty != null)
            return keyProperty;

        // Convention: "Id" or "{TypeName}Id"
        keyProperty = type.GetProperty("Id") ?? type.GetProperty($"{type.Name}Id");

        if (keyProperty == null)
            throw new InvalidOperationException($"No key property found for entity type {type.Name}");

        return keyProperty;
    }

    protected virtual IEnumerable<PropertyInfo> GetInsertableProperties()
    {
        var type = typeof(TEntity);
        var keyProperty = GetKeyProperty();

        return type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() == null)
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption 
                != System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)
            .Where(p => p != keyProperty || keyProperty.PropertyType != typeof(int)); // Skip auto-increment int keys
    }

    protected virtual IEnumerable<PropertyInfo> GetUpdatableProperties()
    {
        var type = typeof(TEntity);
        var keyProperty = GetKeyProperty();

        return type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() == null)
            .Where(p => p != keyProperty); // Don't update the key
    }
}
