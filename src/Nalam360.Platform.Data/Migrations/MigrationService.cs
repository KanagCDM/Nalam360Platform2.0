using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nalam360.Platform.Core.Results;
using Nalam360.Platform.Core.Time;

namespace Nalam360.Platform.Data.Migrations;

/// <summary>
/// Executes database migrations with transaction support and version tracking.
/// </summary>
public class MigrationService : IMigrationService
{
    private readonly DbContext _context;
    private readonly IMigrationRepository _repository;
    private readonly IMigrationGenerator _generator;
    private readonly ITimeProvider _timeProvider;
    private readonly string _appliedBy;

    private const string MigrationTableName = "__MigrationHistory";

    public MigrationService(
        DbContext context,
        IMigrationRepository repository,
        IMigrationGenerator generator,
        ITimeProvider timeProvider,
        string appliedBy = "System")
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _appliedBy = appliedBy;
    }

    public async Task<Result<IReadOnlyList<Migration>>> GetPendingMigrationsAsync(
        CancellationToken ct = default)
    {
        try
        {
            await EnsureMigrationTableExistsAsync(ct);

            var allMigrations = await _repository.GetAllMigrationsAsync(ct);
            if (allMigrations.IsFailure)
                return Result<IReadOnlyList<Migration>>.Failure(allMigrations.Error!);

            var appliedMigrations = await GetAppliedMigrationsAsync(ct);
            if (appliedMigrations.IsFailure)
                return Result<IReadOnlyList<Migration>>.Failure(appliedMigrations.Error!);

            var appliedIds = appliedMigrations.Value!.Select(h => h.MigrationId).ToHashSet();
            var pending = allMigrations.Value!
                .Where(m => !appliedIds.Contains(m.Id))
                .OrderBy(m => m.Id)
                .ToList();

            return Result<IReadOnlyList<Migration>>.Success(pending);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Migration>>.Failure(
                Error.Database("GET_PENDING_MIGRATIONS_ERROR", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<MigrationHistory>>> GetAppliedMigrationsAsync(
        CancellationToken ct = default)
    {
        try
        {
            await EnsureMigrationTableExistsAsync(ct);

            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;

            if (!wasOpen)
                await connection.OpenAsync(ct);

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $@"
                    SELECT MigrationId, MigrationName, AppliedAt, AppliedBy, ExecutionTimeMs, Checksum
                    FROM {MigrationTableName}
                    ORDER BY AppliedAt";

                var history = new List<MigrationHistory>();
                using var reader = await command.ExecuteReaderAsync(ct);
                
                while (await reader.ReadAsync(ct))
                {
                    history.Add(new MigrationHistory
                    {
                        MigrationId = reader.GetString(0),
                        MigrationName = reader.GetString(1),
                        AppliedAt = reader.GetDateTime(2),
                        AppliedBy = reader.GetString(3),
                        ExecutionTimeMs = reader.GetInt32(4),
                        Checksum = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }

                return Result<IReadOnlyList<MigrationHistory>>.Success(history);
            }
            finally
            {
                if (!wasOpen)
                    await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MigrationHistory>>.Failure(
                Error.Database("GET_APPLIED_MIGRATIONS_ERROR", ex.Message));
        }
    }

    public async Task<Result<MigrationExecutionResult>> ApplyMigrationAsync(
        Migration migration,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var startTime = _timeProvider.UtcNow;

        try
        {
            await EnsureMigrationTableExistsAsync(ct);

            // Check if already applied
            var applied = await GetAppliedMigrationsAsync(ct);
            if (applied.IsSuccess && applied.Value!.Any(h => h.MigrationId == migration.Id))
            {
                return Result<MigrationExecutionResult>.Failure(
                    Error.Conflict($"Migration '{migration.Id}' has already been applied"));
            }

            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;

            if (!wasOpen)
                await connection.OpenAsync(ct);

            IDbTransaction? transaction = null;
            try
            {
                transaction = connection.BeginTransaction();

                // Execute migration script
                var statements = SplitIntoStatements(migration.UpScript);
                foreach (var statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        using var command = connection.CreateCommand();
                        command.Transaction = transaction as System.Data.Common.DbTransaction;
                        command.CommandText = statement;
                        
                        if (command is System.Data.Common.DbCommand dbCommand)
                            await dbCommand.ExecuteNonQueryAsync(ct);
                        else
                            command.ExecuteNonQuery();
                    }
                }

                // Record migration history
                var checksum = ComputeChecksum(migration.UpScript);
                await RecordMigrationAsync(
                    connection,
                    transaction,
                    migration.Id,
                    migration.Name,
                    (int)sw.ElapsedMilliseconds,
                    checksum,
                    ct);

                transaction.Commit();
                sw.Stop();

                return Result<MigrationExecutionResult>.Success(
                    MigrationExecutionResult.Successful(
                        migration.Id,
                        migration.Name,
                        startTime,
                        sw.Elapsed,
                        statements));
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                sw.Stop();

                return Result<MigrationExecutionResult>.Success(
                    MigrationExecutionResult.Failed(
                        migration.Id,
                        migration.Name,
                        startTime,
                        sw.Elapsed,
                        ex.Message));
            }
            finally
            {
                transaction?.Dispose();
                if (!wasOpen)
                    await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Result<MigrationExecutionResult>.Failure(
                Error.Database("APPLY_MIGRATION_ERROR", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<MigrationExecutionResult>>> ApplyAllPendingAsync(
        CancellationToken ct = default)
    {
        var pending = await GetPendingMigrationsAsync(ct);
        if (pending.IsFailure)
            return Result<IReadOnlyList<MigrationExecutionResult>>.Failure(pending.Error!);

        var results = new List<MigrationExecutionResult>();

        foreach (var migration in pending.Value!)
        {
            var result = await ApplyMigrationAsync(migration, ct);
            if (result.IsFailure)
                return Result<IReadOnlyList<MigrationExecutionResult>>.Failure(result.Error!);

            results.Add(result.Value!);

            // Stop if any migration fails
            if (!result.Value!.Success)
                break;
        }

        return Result<IReadOnlyList<MigrationExecutionResult>>.Success(results);
    }

    public async Task<Result<MigrationExecutionResult>> RollbackLastMigrationAsync(
        CancellationToken ct = default)
    {
        var applied = await GetAppliedMigrationsAsync(ct);
        if (applied.IsFailure)
            return Result<MigrationExecutionResult>.Failure(applied.Error!);

        var lastMigration = applied.Value!.OrderByDescending(h => h.AppliedAt).FirstOrDefault();
        if (lastMigration == null)
        {
            return Result<MigrationExecutionResult>.Failure(
                Error.NotFound("Migration", "No migrations to rollback"));
        }

        var migration = await _repository.GetMigrationByIdAsync(lastMigration.MigrationId, ct);
        if (migration.IsFailure || migration.Value == null)
        {
            return Result<MigrationExecutionResult>.Failure(
                Error.NotFound("Migration", lastMigration.MigrationId));
        }

        return await ExecuteRollbackAsync(migration.Value, ct);
    }

    public async Task<Result<IReadOnlyList<MigrationExecutionResult>>> RollbackToAsync(
        string targetMigrationId,
        CancellationToken ct = default)
    {
        var applied = await GetAppliedMigrationsAsync(ct);
        if (applied.IsFailure)
            return Result<IReadOnlyList<MigrationExecutionResult>>.Failure(applied.Error!);

        var migrationsToRollback = applied.Value!
            .Where(h => string.Compare(h.MigrationId, targetMigrationId, StringComparison.Ordinal) > 0)
            .OrderByDescending(h => h.AppliedAt)
            .ToList();

        var results = new List<MigrationExecutionResult>();

        foreach (var history in migrationsToRollback)
        {
            var migration = await _repository.GetMigrationByIdAsync(history.MigrationId, ct);
            if (migration.IsFailure || migration.Value == null)
                continue;

            var result = await ExecuteRollbackAsync(migration.Value, ct);
            if (result.IsFailure)
                return Result<IReadOnlyList<MigrationExecutionResult>>.Failure(result.Error!);

            results.Add(result.Value!);

            if (!result.Value!.Success)
                break;
        }

        return Result<IReadOnlyList<MigrationExecutionResult>>.Success(results);
    }

    public async Task<Result<Migration>> GenerateMigrationAsync(
        string name,
        string? description = null,
        CancellationToken ct = default)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var migrationId = $"{timestamp}_{name.Replace(" ", "_")}";

            var scripts = await _generator.GenerateMigrationScriptsAsync(name, ct);
            if (scripts.IsFailure)
                return Result<Migration>.Failure(scripts.Error!);

            var migration = new Migration
            {
                Id = migrationId,
                Name = name,
                UpScript = scripts.Value.UpScript,
                DownScript = scripts.Value.DownScript,
                Description = description,
                CreatedAt = _timeProvider.UtcNow
            };

            var saveResult = await _repository.SaveMigrationAsync(migration, ct);
            if (saveResult.IsFailure)
                return Result<Migration>.Failure(saveResult.Error!);

            return Result<Migration>.Success(migration);
        }
        catch (Exception ex)
        {
            return Result<Migration>.Failure(
                Error.Unexpected("GENERATE_MIGRATION_ERROR", ex.Message));
        }
    }

    public async Task<Result<bool>> ValidateMigrationIntegrityAsync(CancellationToken ct = default)
    {
        try
        {
            var applied = await GetAppliedMigrationsAsync(ct);
            if (applied.IsFailure)
                return Result<bool>.Failure(applied.Error!);

            foreach (var history in applied.Value!)
            {
                var migration = await _repository.GetMigrationByIdAsync(history.MigrationId, ct);
                if (migration.IsFailure || migration.Value == null)
                    continue;

                var expectedChecksum = ComputeChecksum(migration.Value.UpScript);
                if (history.Checksum != null && history.Checksum != expectedChecksum)
                {
                    return Result<bool>.Failure(
                        Error.Validation($"Migration '{history.MigrationId}' checksum mismatch - migration may have been tampered with"));
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                Error.Database("VALIDATE_INTEGRITY_ERROR", ex.Message));
        }
    }

    private async Task<Result<MigrationExecutionResult>> ExecuteRollbackAsync(
        Migration migration,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var startTime = _timeProvider.UtcNow;

        try
        {
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;

            if (!wasOpen)
                await connection.OpenAsync(ct);

            IDbTransaction? transaction = null;
            try
            {
                transaction = connection.BeginTransaction();

                // Execute rollback script
                var statements = SplitIntoStatements(migration.DownScript);
                foreach (var statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        using var command = connection.CreateCommand();
                        command.Transaction = transaction as System.Data.Common.DbTransaction;
                        command.CommandText = statement;
                        
                        if (command is System.Data.Common.DbCommand dbCommand)
                            await dbCommand.ExecuteNonQueryAsync(ct);
                        else
                            command.ExecuteNonQuery();
                    }
                }

                // Remove from migration history
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction as System.Data.Common.DbTransaction;
                    command.CommandText = $@"
                        DELETE FROM {MigrationTableName}
                        WHERE MigrationId = @MigrationId";

                    var param = new SqlParameter("@MigrationId", migration.Id);
                    command.Parameters.Add(param);
                    
                    if (command is System.Data.Common.DbCommand dbCommand)
                        await dbCommand.ExecuteNonQueryAsync(ct);
                    else
                        command.ExecuteNonQuery();
                }

                transaction.Commit();
                sw.Stop();

                return Result<MigrationExecutionResult>.Success(
                    MigrationExecutionResult.Successful(
                        migration.Id,
                        migration.Name,
                        startTime,
                        sw.Elapsed,
                        statements));
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                sw.Stop();

                return Result<MigrationExecutionResult>.Success(
                    MigrationExecutionResult.Failed(
                        migration.Id,
                        migration.Name,
                        startTime,
                        sw.Elapsed,
                        ex.Message));
            }
            finally
            {
                transaction?.Dispose();
                if (!wasOpen)
                    await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Result<MigrationExecutionResult>.Failure(
                Error.Database("ROLLBACK_ERROR", ex.Message));
        }
    }

    private async Task EnsureMigrationTableExistsAsync(CancellationToken ct)
    {
        var connection = _context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;

        if (!wasOpen)
            await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{MigrationTableName}')
                BEGIN
                    CREATE TABLE {MigrationTableName} (
                        MigrationId NVARCHAR(200) NOT NULL PRIMARY KEY,
                        MigrationName NVARCHAR(500) NOT NULL,
                        AppliedAt DATETIME2 NOT NULL,
                        AppliedBy NVARCHAR(100) NOT NULL,
                        ExecutionTimeMs INT NOT NULL,
                        Checksum NVARCHAR(64) NULL
                    )
                END";

            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private async Task RecordMigrationAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        string migrationId,
        string migrationName,
        int executionTimeMs,
        string checksum,
        CancellationToken ct)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction as System.Data.Common.DbTransaction;
        command.CommandText = $@"
            INSERT INTO {MigrationTableName} (MigrationId, MigrationName, AppliedAt, AppliedBy, ExecutionTimeMs, Checksum)
            VALUES (@MigrationId, @MigrationName, @AppliedAt, @AppliedBy, @ExecutionTimeMs, @Checksum)";

        command.Parameters.Add(new SqlParameter("@MigrationId", migrationId));
        command.Parameters.Add(new SqlParameter("@MigrationName", migrationName));
        command.Parameters.Add(new SqlParameter("@AppliedAt", _timeProvider.UtcNow));
        command.Parameters.Add(new SqlParameter("@AppliedBy", _appliedBy));
        command.Parameters.Add(new SqlParameter("@ExecutionTimeMs", executionTimeMs));
        command.Parameters.Add(new SqlParameter("@Checksum", checksum));

        if (command is System.Data.Common.DbCommand dbCommand)
            await dbCommand.ExecuteNonQueryAsync(ct);
        else
            command.ExecuteNonQuery();
    }

    private List<string> SplitIntoStatements(string script)
    {
        // Split by GO statements (SQL Server batch separator)
        return script
            .Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO", "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s) && !s.Trim().StartsWith("--"))
            .Select(s => s.Trim())
            .ToList();
    }

    private string ComputeChecksum(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
