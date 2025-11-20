using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a dashboard widget/card that can be placed on the dashboard
/// </summary>
public class DashboardWidget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "card"; // card, chart, table, metric, calendar, list, etc.
    public int Row { get; set; }
    public int Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Icon { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public object? Data { get; set; }
    public DateTime? LastUpdated { get; set; }
    public int RefreshInterval { get; set; } // seconds, 0 = no auto-refresh
    public string? DataSource { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Dashboard layout configuration
/// </summary>
public class DashboardLayout
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Default Dashboard";
    public string? Description { get; set; }
    public int Rows { get; set; } = 12;
    public int Columns { get; set; } = 12;
    public int RowHeight { get; set; } = 60; // pixels
    public int Gap { get; set; } = 16; // pixels
    public List<DashboardWidget> Widgets { get; set; } = new();
    public bool IsDefault { get; set; }
    public bool AllowEditing { get; set; } = true;
    public string? OwnerId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Widget template for creating new widgets
/// </summary>
public class WidgetTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // General, Analytics, Healthcare, Enterprise, etc.
    public string Type { get; set; } = "card";
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public int DefaultRowSpan { get; set; } = 1;
    public int DefaultColumnSpan { get; set; } = 1;
    public Dictionary<string, object> DefaultSettings { get; set; } = new();
    public bool RequiresDataSource { get; set; }
    public List<string> AvailableDataSources { get; set; } = new();
}

/// <summary>
/// Widget data refresh event args
/// </summary>
public class WidgetRefreshEventArgs
{
    public string WidgetId { get; set; } = string.Empty;
    public object? NewData { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dashboard metric widget data
/// </summary>
public class MetricData
{
    public string Label { get; set; } = string.Empty;
    public object Value { get; set; } = 0;
    public string? Unit { get; set; }
    public string? Format { get; set; } // number, currency, percentage, etc.
    public decimal? Change { get; set; } // percentage change
    public string? ChangeLabel { get; set; } // "vs last month", "vs last week"
    public string? TrendDirection { get; set; } // up, down, neutral
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Dashboard chart widget data
/// </summary>
public class ChartData
{
    public string ChartType { get; set; } = "line"; // line, bar, pie, doughnut, area, etc.
    public List<string> Labels { get; set; } = new();
    public List<ChartDataset> Datasets { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Chart dataset
/// </summary>
public class ChartDataset
{
    public string Label { get; set; } = string.Empty;
    public List<object> Data { get; set; } = new();
    public string? BackgroundColor { get; set; }
    public string? BorderColor { get; set; }
    public int BorderWidth { get; set; } = 1;
    public bool Fill { get; set; }
}

/// <summary>
/// Dashboard table widget data
/// </summary>
public class TableData
{
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public int TotalRows { get; set; }
    public int PageSize { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
    public bool ShowPaging { get; set; } = true;
    public bool ShowSearch { get; set; } = true;
}

/// <summary>
/// Dashboard list widget data
/// </summary>
public class ListData
{
    public List<ListItem> Items { get; set; } = new();
    public bool ShowIcons { get; set; } = true;
    public bool ShowTimestamps { get; set; } = true;
    public int MaxItems { get; set; } = 10;
}

/// <summary>
/// List item
/// </summary>
public class ListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string? IconColor { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Link { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Dashboard calendar widget data
/// </summary>
public class CalendarData
{
    public List<CalendarEvent> Events { get; set; } = new();
    public DateTime CurrentDate { get; set; } = DateTime.Today;
    public string ViewMode { get; set; } = "month"; // month, week, day
}

/// <summary>
/// Calendar event
/// </summary>
public class CalendarEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Location { get; set; }
}
