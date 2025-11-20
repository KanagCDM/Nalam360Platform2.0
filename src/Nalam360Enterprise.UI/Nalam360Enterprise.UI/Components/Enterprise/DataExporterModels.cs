namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Export configuration
/// </summary>
public class ExportConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DataExportFormat Format { get; set; } = DataExportFormat.Excel;
    public List<ExportColumn> Columns { get; set; } = new();
    public ExportFilter? Filter { get; set; }
    public DataExportOptions Options { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Export formats
/// </summary>
public enum DataExportFormat
{
    Excel,
    CSV,
    PDF,
    JSON,
    XML,
    HTML
}

/// <summary>
/// Export column configuration
/// </summary>
public class ExportColumn
{
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public bool IsVisible { get; set; } = true;
    public int Width { get; set; } = 100;
    public string? Format { get; set; }
    public ExportAlignment Alignment { get; set; } = ExportAlignment.Left;
    public bool IsBold { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
}

/// <summary>
/// Column alignment
/// </summary>
public enum ExportAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Export filter criteria
/// </summary>
public class ExportFilter
{
    public List<DataFilterCondition> Conditions { get; set; } = new();
    public FilterLogic Logic { get; set; } = FilterLogic.And;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxRecords { get; set; }
}

/// <summary>
/// Filter condition
/// </summary>
public class DataFilterCondition
{
    public string Field { get; set; } = string.Empty;
    public DataFilterOperator Operator { get; set; } = DataFilterOperator.Equals;
    public string? Value { get; set; }
}

/// <summary>
/// Filter operators
/// </summary>
public enum DataFilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Between,
    In,
    IsNull,
    IsNotNull
}

/// <summary>
/// Filter logic
/// </summary>
public enum FilterLogic
{
    And,
    Or
}

/// <summary>
/// Export options
/// </summary>
public class DataExportOptions
{
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeFooters { get; set; } = false;
    public bool AutoFitColumns { get; set; } = true;
    public bool ShowGridLines { get; set; } = true;
    public bool FreezeHeaders { get; set; } = true;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public DataPageOrientation Orientation { get; set; } = DataPageOrientation.Portrait;
    public DataPageSize PageSize { get; set; } = DataPageSize.A4;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string NumberFormat { get; set; } = "#,##0.00";
    public string Delimiter { get; set; } = ",";
    public string Encoding { get; set; } = "UTF-8";
    public bool CompressOutput { get; set; } = false;
    public string? Password { get; set; }
}

/// <summary>
/// Page orientation
/// </summary>
public enum DataPageOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Page size
/// </summary>
public enum DataPageSize
{
    A4,
    Letter,
    Legal,
    A3,
    A5
}

/// <summary>
/// Export template
/// </summary>
public class ExportTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DataExportFormat Format { get; set; } = DataExportFormat.Excel;
    public List<ExportColumn> Columns { get; set; } = new();
    public DataExportOptions Options { get; set; } = new();
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Scheduled export
/// </summary>
public class ScheduledExport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ExportConfiguration Configuration { get; set; } = new();
    public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Daily;
    public TimeSpan ExecutionTime { get; set; } = TimeSpan.FromHours(9);
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    public int? DayOfMonth { get; set; }
    public List<string> Recipients { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
}

/// <summary>
/// Schedule frequency
/// </summary>
public enum ScheduleFrequency
{
    Once,
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

/// <summary>
/// Export result
/// </summary>
public class ExportResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public int RecordCount { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Export history entry
/// </summary>
public class ExportHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConfigurationName { get; set; } = string.Empty;
    public DataExportFormat Format { get; set; }
    public int RecordCount { get; set; }
    public long FileSize { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.Now;
    public string ExportedBy { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Available data fields for export
/// </summary>
public class ExportField
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
}
