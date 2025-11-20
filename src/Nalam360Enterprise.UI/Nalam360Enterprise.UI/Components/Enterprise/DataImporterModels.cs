namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a data import session
/// </summary>
public class DataImportSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ImportStep CurrentStep { get; set; } = ImportStep.Upload;
    public ImportStatus Status { get; set; } = ImportStatus.InProgress;
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? FileType { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public List<ColumnMapping> ColumnMappings { get; set; } = new();
    public List<ValidationRule> ValidationRules { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Import wizard steps
/// </summary>
public enum ImportStep
{
    Upload = 1,
    Mapping = 2,
    Validation = 3,
    Preview = 4,
    Import = 5,
    Complete = 6
}

/// <summary>
/// Import session status
/// </summary>
public enum ImportStatus
{
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Column mapping configuration
/// </summary>
public class ColumnMapping
{
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public string? DefaultValue { get; set; }
    public string? TransformFunction { get; set; }
    public int SourceIndex { get; set; }
}

/// <summary>
/// Available target fields for mapping
/// </summary>
public class TargetField
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public string? Description { get; set; }
    public List<string> AllowedValues { get; set; } = new();
}

/// <summary>
/// Validation rule
/// </summary>
public class ValidationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Field { get; set; } = string.Empty;
    public ValidationRuleType Type { get; set; }
    public string? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Validation rule types
/// </summary>
public enum ValidationRuleType
{
    Required,
    MinLength,
    MaxLength,
    Pattern,
    MinValue,
    MaxValue,
    Email,
    Phone,
    Url,
    Date,
    Custom
}

/// <summary>
/// Import error
/// </summary>
public class ImportError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? Value { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Warning,
    Error,
    Critical
}

/// <summary>
/// Import template
/// </summary>
public class ImportTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ColumnMapping> ColumnMappings { get; set; } = new();
    public List<ValidationRule> ValidationRules { get; set; } = new();
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Import options
/// </summary>
public class ImportOptions
{
    public bool SkipFirstRow { get; set; } = true;
    public string Delimiter { get; set; } = ",";
    public string Encoding { get; set; } = "UTF-8";
    public bool AllowDuplicates { get; set; } = false;
    public bool StopOnError { get; set; } = false;
    public int BatchSize { get; set; } = 100;
    public bool CreateBackup { get; set; } = true;
    public bool SendNotification { get; set; } = true;
    public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Skip;
}

/// <summary>
/// Duplicate handling strategies
/// </summary>
public enum DuplicateHandling
{
    Skip,
    Update,
    Error,
    CreateNew
}

/// <summary>
/// Import preview data
/// </summary>
public class ImportPreviewData
{
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public int TotalRows { get; set; }
    public int PreviewRows { get; set; }
}

/// <summary>
/// Import statistics
/// </summary>
public class ImportStatistics
{
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public int SkippedRows { get; set; }
    public int DuplicateRows { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double RowsPerSecond => Duration.TotalSeconds > 0 ? ProcessedRows / Duration.TotalSeconds : 0;
    public double SuccessRate => TotalRows > 0 ? (double)SuccessfulRows / TotalRows * 100 : 0;
}

/// <summary>
/// Import result
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public ImportStatistics Statistics { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
