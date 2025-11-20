using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.Migrations;

/// <summary>
/// Generates migration scripts by comparing EF Core model with database schema.
/// </summary>
public class EfCoreMigrationGenerator : IMigrationGenerator
{
    private readonly DbContext _context;

    public EfCoreMigrationGenerator(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<(string UpScript, string DownScript)>> GenerateMigrationScriptsAsync(
        string migrationName,
        CancellationToken ct = default)
    {
        try
        {
            var differences = await DetectSchemaChangesAsync(ct);
            if (differences.IsFailure)
                return Result<(string, string)>.Failure(differences.Error!);

            var upScript = GenerateUpScript(differences.Value!);
            var downScript = GenerateDownScript(differences.Value!);

            return Result<(string, string)>.Success((upScript, downScript));
        }
        catch (Exception ex)
        {
            return Result<(string, string)>.Failure(
                Error.Unexpected("MIGRATION_GENERATION_ERROR", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<SchemaDifference>>> DetectSchemaChangesAsync(
        CancellationToken ct = default)
    {
        try
        {
            var differences = new List<SchemaDifference>();
            var model = _context.Model;

            // Get current database schema
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            if (!wasOpen)
                await connection.OpenAsync(ct);

            try
            {
                var existingTables = await GetExistingTablesAsync(connection, ct);
                var modelTables = GetModelTables(model);

                // Detect table additions
                foreach (var modelTable in modelTables)
                {
                    if (!existingTables.Contains(modelTable, StringComparer.OrdinalIgnoreCase))
                    {
                        differences.Add(new SchemaDifference
                        {
                            Type = DifferenceType.TableAdded,
                            EntityName = modelTable,
                            Description = $"Table '{modelTable}' needs to be created"
                        });

                        // Add column differences for new table
                        var entityType = model.GetEntityTypes()
                            .FirstOrDefault(e => e.GetTableName() == modelTable);
                        
                        if (entityType != null)
                        {
                            foreach (var property in entityType.GetProperties())
                            {
                                differences.Add(new SchemaDifference
                                {
                                    Type = DifferenceType.ColumnAdded,
                                    EntityName = modelTable,
                                    PropertyName = property.GetColumnName(),
                                    NewValue = GetColumnDefinition(property),
                                    Description = $"Column '{property.GetColumnName()}' in new table '{modelTable}'"
                                });
                            }
                        }
                    }
                }

                // Detect table removals
                foreach (var existingTable in existingTables)
                {
                    if (!modelTables.Contains(existingTable, StringComparer.OrdinalIgnoreCase))
                    {
                        differences.Add(new SchemaDifference
                        {
                            Type = DifferenceType.TableRemoved,
                            EntityName = existingTable,
                            Description = $"Table '{existingTable}' should be dropped"
                        });
                    }
                }

                // Detect column changes for existing tables
                foreach (var modelTable in modelTables)
                {
                    if (existingTables.Contains(modelTable, StringComparer.OrdinalIgnoreCase))
                    {
                        var entityType = model.GetEntityTypes()
                            .FirstOrDefault(e => e.GetTableName() == modelTable);

                        if (entityType != null)
                        {
                            var existingColumns = await GetExistingColumnsAsync(connection, modelTable, ct);
                            var modelColumns = entityType.GetProperties()
                                .Select(p => p.GetColumnName())
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Detect column additions
                            foreach (var property in entityType.GetProperties())
                            {
                                var columnName = property.GetColumnName();
                                if (!existingColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                                {
                                    differences.Add(new SchemaDifference
                                    {
                                        Type = DifferenceType.ColumnAdded,
                                        EntityName = modelTable,
                                        PropertyName = columnName,
                                        NewValue = GetColumnDefinition(property),
                                        Description = $"Column '{columnName}' needs to be added to '{modelTable}'"
                                    });
                                }
                            }

                            // Detect column removals
                            foreach (var existingColumn in existingColumns)
                            {
                                if (!modelColumns.Contains(existingColumn))
                                {
                                    differences.Add(new SchemaDifference
                                    {
                                        Type = DifferenceType.ColumnRemoved,
                                        EntityName = modelTable,
                                        PropertyName = existingColumn,
                                        Description = $"Column '{existingColumn}' should be removed from '{modelTable}'"
                                    });
                                }
                            }
                        }
                    }
                }

                return Result<IReadOnlyList<SchemaDifference>>.Success(differences);
            }
            finally
            {
                if (!wasOpen)
                    await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<SchemaDifference>>.Failure(
                Error.Database("SCHEMA_DETECTION_ERROR", ex.Message));
        }
    }

    private string GenerateUpScript(IReadOnlyList<SchemaDifference> differences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- Auto-generated migration script");
        sb.AppendLine($"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Group by table for cleaner script
        var tableGroups = differences.GroupBy(d => d.EntityName).OrderBy(g => g.Key);

        foreach (var group in tableGroups)
        {
            sb.AppendLine($"-- Table: {group.Key}");
            
            foreach (var diff in group.OrderBy(d => d.Type))
            {
                switch (diff.Type)
                {
                    case DifferenceType.TableAdded:
                        sb.AppendLine(GenerateCreateTableScript(group.Key, group.Where(d => d.Type == DifferenceType.ColumnAdded)));
                        break;

                    case DifferenceType.TableRemoved:
                        sb.AppendLine($"DROP TABLE IF EXISTS [{group.Key}];");
                        break;

                    case DifferenceType.ColumnAdded when !group.Any(d => d.Type == DifferenceType.TableAdded):
                        sb.AppendLine($"ALTER TABLE [{group.Key}] ADD [{diff.PropertyName}] {diff.NewValue};");
                        break;

                    case DifferenceType.ColumnRemoved:
                        sb.AppendLine($"ALTER TABLE [{group.Key}] DROP COLUMN [{diff.PropertyName}];");
                        break;
                }
            }
            
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateDownScript(IReadOnlyList<SchemaDifference> differences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- Auto-generated rollback script");
        sb.AppendLine($"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Reverse the operations
        var reversed = differences.Reverse().ToList();
        var tableGroups = reversed.GroupBy(d => d.EntityName).OrderBy(g => g.Key);

        foreach (var group in tableGroups)
        {
            sb.AppendLine($"-- Table: {group.Key}");

            foreach (var diff in group)
            {
                switch (diff.Type)
                {
                    case DifferenceType.TableAdded:
                        sb.AppendLine($"DROP TABLE IF EXISTS [{group.Key}];");
                        break;

                    case DifferenceType.TableRemoved:
                        sb.AppendLine($"-- Recreate table [{group.Key}] - manual intervention required");
                        sb.AppendLine($"-- CREATE TABLE [{group.Key}] (...);");
                        break;

                    case DifferenceType.ColumnAdded:
                        sb.AppendLine($"ALTER TABLE [{group.Key}] DROP COLUMN IF EXISTS [{diff.PropertyName}];");
                        break;

                    case DifferenceType.ColumnRemoved:
                        sb.AppendLine($"-- Restore column [{diff.PropertyName}] - manual intervention required");
                        sb.AppendLine($"-- ALTER TABLE [{group.Key}] ADD [{diff.PropertyName}] {diff.OldValue};");
                        break;
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateCreateTableScript(string tableName, IEnumerable<SchemaDifference> columns)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE [{tableName}] (");

        var columnDefinitions = columns
            .Where(c => c.PropertyName != null)
            .Select(c => $"    [{c.PropertyName}] {c.NewValue}")
            .ToList();

        sb.AppendLine(string.Join("," + Environment.NewLine, columnDefinitions));
        sb.AppendLine(");");

        return sb.ToString();
    }

    private async Task<HashSet<string>> GetExistingTablesAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken ct)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE' 
            AND TABLE_SCHEMA = 'dbo'";

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private async Task<HashSet<string>> GetExistingColumnsAsync(
        System.Data.Common.DbConnection connection,
        string tableName,
        CancellationToken ct)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName 
            AND TABLE_SCHEMA = 'dbo'";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@TableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private HashSet<string> GetModelTables(IModel model)
    {
        return model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(t => t != null)
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private string GetColumnDefinition(IProperty property)
    {
        var columnType = property.GetColumnType();
        var isNullable = property.IsNullable;
        var defaultValue = property.GetDefaultValue();

        var definition = columnType;
        
        if (!isNullable)
            definition += " NOT NULL";

        if (defaultValue != null)
            definition += $" DEFAULT {FormatDefaultValue(defaultValue)}";

        if (property.IsPrimaryKey())
            definition += " PRIMARY KEY";

        if (property.ValueGenerated == ValueGenerated.OnAdd && property.ClrType == typeof(int))
            definition = definition.Replace(columnType, $"{columnType} IDENTITY(1,1)");

        return definition;
    }

    private string FormatDefaultValue(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            bool b => b ? "1" : "0",
            _ => value.ToString() ?? "NULL"
        };
    }
}
