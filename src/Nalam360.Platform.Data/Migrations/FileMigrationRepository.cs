using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.Migrations;

/// <summary>
/// File-based migration repository that stores migrations as SQL files.
/// </summary>
public class FileMigrationRepository : IMigrationRepository
{
    private readonly string _migrationsDirectory;

    public FileMigrationRepository(string migrationsDirectory)
    {
        _migrationsDirectory = migrationsDirectory ?? throw new ArgumentNullException(nameof(migrationsDirectory));
        
        if (!Directory.Exists(_migrationsDirectory))
        {
            Directory.CreateDirectory(_migrationsDirectory);
        }
    }

    public async Task<Result<IReadOnlyList<Migration>>> GetAllMigrationsAsync(
        CancellationToken ct = default)
    {
        try
        {
            var migrations = new List<Migration>();
            var files = Directory.GetFiles(_migrationsDirectory, "*_up.sql");

            foreach (var upFile in files.OrderBy(f => f))
            {
                var migration = await LoadMigrationFromFileAsync(upFile, ct);
                if (migration != null)
                {
                    migrations.Add(migration);
                }
            }

            return Result<IReadOnlyList<Migration>>.Success(migrations);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Migration>>.Failure(
                Error.Unexpected("LOAD_MIGRATIONS_ERROR", ex.Message));
        }
    }

    public async Task<Result<Migration?>> GetMigrationByIdAsync(
        string migrationId,
        CancellationToken ct = default)
    {
        try
        {
            var upFile = Path.Combine(_migrationsDirectory, $"{migrationId}_up.sql");
            
            if (!File.Exists(upFile))
            {
                return Result<Migration?>.Success(null);
            }

            var migration = await LoadMigrationFromFileAsync(upFile, ct);
            return Result<Migration?>.Success(migration);
        }
        catch (Exception ex)
        {
            return Result<Migration?>.Failure(
                Error.Unexpected("LOAD_MIGRATION_ERROR", ex.Message));
        }
    }

    public async Task<Result> SaveMigrationAsync(
        Migration migration,
        CancellationToken ct = default)
    {
        try
        {
            var upFile = Path.Combine(_migrationsDirectory, $"{migration.Id}_up.sql");
            var downFile = Path.Combine(_migrationsDirectory, $"{migration.Id}_down.sql");
            var metaFile = Path.Combine(_migrationsDirectory, $"{migration.Id}_meta.txt");

            // Save up script
            await File.WriteAllTextAsync(upFile, migration.UpScript, ct);

            // Save down script
            await File.WriteAllTextAsync(downFile, migration.DownScript, ct);

            // Save metadata
            var metadata = new List<string>
            {
                $"Name: {migration.Name}",
                $"Description: {migration.Description ?? "N/A"}",
                $"CreatedAt: {migration.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                $"Dependencies: {string.Join(", ", migration.Dependencies)}"
            };
            await File.WriteAllLinesAsync(metaFile, metadata, ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected("SAVE_MIGRATION_ERROR", ex.Message));
        }
    }

    public async Task<Result> DeleteMigrationAsync(
        string migrationId,
        CancellationToken ct = default)
    {
        try
        {
            var upFile = Path.Combine(_migrationsDirectory, $"{migrationId}_up.sql");
            var downFile = Path.Combine(_migrationsDirectory, $"{migrationId}_down.sql");
            var metaFile = Path.Combine(_migrationsDirectory, $"{migrationId}_meta.txt");

            if (File.Exists(upFile))
                File.Delete(upFile);

            if (File.Exists(downFile))
                File.Delete(downFile);

            if (File.Exists(metaFile))
                File.Delete(metaFile);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected("DELETE_MIGRATION_ERROR", ex.Message));
        }
    }

    private async Task<Migration?> LoadMigrationFromFileAsync(
        string upFilePath,
        CancellationToken ct)
    {
        var fileName = Path.GetFileNameWithoutExtension(upFilePath);
        var migrationId = fileName.Replace("_up", "");
        
        var downFile = Path.Combine(_migrationsDirectory, $"{migrationId}_down.sql");
        var metaFile = Path.Combine(_migrationsDirectory, $"{migrationId}_meta.txt");

        if (!File.Exists(downFile))
            return null;

        var upScript = await File.ReadAllTextAsync(upFilePath, ct);
        var downScript = await File.ReadAllTextAsync(downFile, ct);

        // Load metadata if exists
        string name = migrationId;
        string? description = null;
        DateTime createdAt = File.GetCreationTimeUtc(upFilePath);
        var dependencies = new List<string>();

        if (File.Exists(metaFile))
        {
            var metaLines = await File.ReadAllLinesAsync(metaFile, ct);
            foreach (var line in metaLines)
            {
                if (line.StartsWith("Name: "))
                    name = line.Substring(6);
                else if (line.StartsWith("Description: "))
                {
                    var desc = line.Substring(13);
                    description = desc == "N/A" ? null : desc;
                }
                else if (line.StartsWith("CreatedAt: "))
                {
                    if (DateTime.TryParse(line.Substring(11), out var date))
                        createdAt = date;
                }
                else if (line.StartsWith("Dependencies: "))
                {
                    var deps = line.Substring(14);
                    if (deps != "")
                        dependencies = deps.Split(',').Select(d => d.Trim()).ToList();
                }
            }
        }

        return new Migration
        {
            Id = migrationId,
            Name = name,
            UpScript = upScript,
            DownScript = downScript,
            Description = description,
            CreatedAt = createdAt,
            Dependencies = dependencies
        };
    }
}
