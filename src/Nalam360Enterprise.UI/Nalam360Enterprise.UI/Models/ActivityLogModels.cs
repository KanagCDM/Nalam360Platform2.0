using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Activity log entry representing a system event or user action
/// </summary>
public class ActivityLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActivityLogType Type { get; set; } = ActivityLogType.Information;
    public ActivityLogSeverity Severity { get; set; } = ActivityLogSeverity.Low;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? SessionId { get; set; }
    public string? RequestId { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? UserAgent { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Activity log type classification
/// </summary>
public enum ActivityLogType
{
    Information,
    Warning,
    Error,
    Critical,
    Success,
    Security,
    Performance,
    Audit,
    Debug
}

/// <summary>
/// Activity severity level
/// </summary>
public enum ActivityLogSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Activity log viewer display mode
/// </summary>
public enum ActivityLogViewMode
{
    List,
    Timeline,
    Grid,
    Chart,
    Map
}

/// <summary>
/// Activity log grouping option
/// </summary>
public enum ActivityLogGroupBy
{
    None,
    Type,
    User,
    Module,
    Resource,
    Date,
    Severity,
    Category
}

/// <summary>
/// Filter criteria for activity logs
/// </summary>
public class ActivityLogFilter
{
    public List<ActivityLogType> Types { get; set; } = new();
    public List<ActivityLogSeverity> Severities { get; set; } = new();
    public List<string> Users { get; set; } = new();
    public List<string> Modules { get; set; } = new();
    public List<string> Resources { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchQuery { get; set; }
    public bool ShowSuccessOnly { get; set; } = false;
    public bool ShowErrorsOnly { get; set; } = false;
    public bool ShowSecurityOnly { get; set; } = false;
    public TimeSpan? MinDuration { get; set; }
    public TimeSpan? MaxDuration { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Statistics for activity log data
/// </summary>
public class ActivityLogStatistics
{
    public int TotalEntries { get; set; }
    public int InformationCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int CriticalCount { get; set; }
    public int SecurityCount { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueModules { get; set; }
    public int UniqueResources { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public DateTime? OldestEntry { get; set; }
    public DateTime? NewestEntry { get; set; }
    public Dictionary<string, int> EntriesByType { get; set; } = new();
    public Dictionary<string, int> EntriesByModule { get; set; } = new();
    public Dictionary<string, int> EntriesByUser { get; set; } = new();
    public Dictionary<string, int> EntriesByHour { get; set; } = new();
}

/// <summary>
/// Activity log export options
/// </summary>
public class ActivityLogExportOptions
{
    public ActivityLogExportFormat Format { get; set; } = ActivityLogExportFormat.Excel;
    public string FileName { get; set; } = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}";
    public bool IncludeMetadata { get; set; } = true;
    public bool IncludeStackTrace { get; set; } = false;
    public bool IncludeHeaders { get; set; } = true;
    public ActivityLogFilter? Filter { get; set; }
    public List<string> Columns { get; set; } = new()
    {
        "Timestamp", "Type", "Action", "User", "Module", "Resource", "Description"
    };
}

/// <summary>
/// Export format options
/// </summary>
public enum ActivityLogExportFormat
{
    Excel,
    CSV,
    JSON,
    PDF,
    HTML
}

/// <summary>
/// Activity log settings
/// </summary>
public class ActivityLogSettings
{
    public bool AutoRefresh { get; set; } = false;
    public int RefreshInterval { get; set; } = 30; // seconds
    public int PageSize { get; set; } = 50;
    public bool ShowStackTrace { get; set; } = false;
    public bool ShowMetadata { get; set; } = true;
    public bool ShowFilters { get; set; } = true;
    public bool ShowStatistics { get; set; } = true;
    public bool EnableExport { get; set; } = true;
    public bool EnableRealTime { get; set; } = false;
    public int MaxEntries { get; set; } = 10000;
    public List<string> VisibleColumns { get; set; } = new()
    {
        "Timestamp", "Type", "Severity", "Action", "User", "Module", "Resource", "Description"
    };
}

/// <summary>
/// Activity log entry details modal data
/// </summary>
public class ActivityLogDetails
{
    public ActivityLogEntry Entry { get; set; } = new();
    public List<ActivityLogEntry> RelatedEntries { get; set; } = new();
    public Dictionary<string, string> ParsedMetadata { get; set; } = new();
}

/// <summary>
/// Chart data for activity visualization
/// </summary>
public class ActivityLogChartData
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "#1976d2";
}

/// <summary>
/// Time-series data point for activity timeline
/// </summary>
public class ActivityTimeSeriesData
{
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
    public ActivityLogType Type { get; set; }
}
