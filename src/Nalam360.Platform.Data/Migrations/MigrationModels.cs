using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.Migrations;

/// <summary>
/// Represents a database migration with up and down scripts.
/// </summary>
public class Migration
{
    /// <summary>
    /// Unique migration identifier (typically timestamp-based).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable migration name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// SQL script to apply the migration (forward).
    /// </summary>
    public required string UpScript { get; init; }

    /// <summary>
    /// SQL script to rollback the migration (backward).
    /// </summary>
    public required string DownScript { get; init; }

    /// <summary>
    /// Migration description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// When the migration was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Migration dependencies (must be applied before this one).
    /// </summary>
    public List<string> Dependencies { get; init; } = new();
}

/// <summary>
/// Represents the execution result of a migration.
/// </summary>
public class MigrationExecutionResult
{
    public required string MigrationId { get; init; }
    public required string MigrationName { get; init; }
    public required bool Success { get; init; }
    public required DateTime ExecutedAt { get; init; }
    public required TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> ExecutedStatements { get; init; } = new();

    public static MigrationExecutionResult Successful(
        string migrationId,
        string migrationName,
        DateTime executedAt,
        TimeSpan duration,
        List<string> statements) => new()
        {
            MigrationId = migrationId,
            MigrationName = migrationName,
            Success = true,
            ExecutedAt = executedAt,
            Duration = duration,
            ExecutedStatements = statements
        };

    public static MigrationExecutionResult Failed(
        string migrationId,
        string migrationName,
        DateTime executedAt,
        TimeSpan duration,
        string error) => new()
        {
            MigrationId = migrationId,
            MigrationName = migrationName,
            Success = false,
            ExecutedAt = executedAt,
            Duration = duration,
            ErrorMessage = error
        };
}

/// <summary>
/// Tracks applied migrations in the database.
/// </summary>
public class MigrationHistory
{
    public required string MigrationId { get; init; }
    public required string MigrationName { get; init; }
    public required DateTime AppliedAt { get; init; }
    public required string AppliedBy { get; init; }
    public int ExecutionTimeMs { get; init; }
    public string? Checksum { get; init; }
}

/// <summary>
/// Service for managing database migrations.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Get all pending migrations that haven't been applied yet.
    /// </summary>
    Task<Result<IReadOnlyList<Migration>>> GetPendingMigrationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all applied migrations from the database.
    /// </summary>
    Task<Result<IReadOnlyList<MigrationHistory>>> GetAppliedMigrationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Apply a single migration.
    /// </summary>
    Task<Result<MigrationExecutionResult>> ApplyMigrationAsync(Migration migration, CancellationToken ct = default);

    /// <summary>
    /// Apply all pending migrations.
    /// </summary>
    Task<Result<IReadOnlyList<MigrationExecutionResult>>> ApplyAllPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Rollback the last applied migration.
    /// </summary>
    Task<Result<MigrationExecutionResult>> RollbackLastMigrationAsync(CancellationToken ct = default);

    /// <summary>
    /// Rollback to a specific migration version.
    /// </summary>
    Task<Result<IReadOnlyList<MigrationExecutionResult>>> RollbackToAsync(string targetMigrationId, CancellationToken ct = default);

    /// <summary>
    /// Generate a new migration from current model changes.
    /// </summary>
    Task<Result<Migration>> GenerateMigrationAsync(string name, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Validate migration checksums to detect tampering.
    /// </summary>
    Task<Result<bool>> ValidateMigrationIntegrityAsync(CancellationToken ct = default);
}

/// <summary>
/// Repository for storing and retrieving migrations.
/// </summary>
public interface IMigrationRepository
{
    /// <summary>
    /// Get all available migrations ordered by ID.
    /// </summary>
    Task<Result<IReadOnlyList<Migration>>> GetAllMigrationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific migration by ID.
    /// </summary>
    Task<Result<Migration?>> GetMigrationByIdAsync(string migrationId, CancellationToken ct = default);

    /// <summary>
    /// Save a new migration.
    /// </summary>
    Task<Result> SaveMigrationAsync(Migration migration, CancellationToken ct = default);

    /// <summary>
    /// Delete a migration (use with caution).
    /// </summary>
    Task<Result> DeleteMigrationAsync(string migrationId, CancellationToken ct = default);
}

/// <summary>
/// Generates migration scripts from entity model changes.
/// </summary>
public interface IMigrationGenerator
{
    /// <summary>
    /// Generate migration SQL from model changes.
    /// </summary>
    Task<Result<(string UpScript, string DownScript)>> GenerateMigrationScriptsAsync(
        string migrationName,
        CancellationToken ct = default);

    /// <summary>
    /// Compare current model with database schema and detect differences.
    /// </summary>
    Task<Result<IReadOnlyList<SchemaDifference>>> DetectSchemaChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Represents a difference between model and database schema.
/// </summary>
public class SchemaDifference
{
    public required DifferenceType Type { get; init; }
    public required string EntityName { get; init; }
    public string? PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Types of schema differences.
/// </summary>
public enum DifferenceType
{
    TableAdded,
    TableRemoved,
    TableRenamed,
    ColumnAdded,
    ColumnRemoved,
    ColumnRenamed,
    ColumnTypeChanged,
    ColumnNullabilityChanged,
    IndexAdded,
    IndexRemoved,
    ForeignKeyAdded,
    ForeignKeyRemoved,
    UniqueConstraintAdded,
    UniqueConstraintRemoved
}
