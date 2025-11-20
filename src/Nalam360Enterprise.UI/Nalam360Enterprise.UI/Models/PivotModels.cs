using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Pivot field configuration
/// </summary>
public class PivotField
{
    public string Name { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public PivotFieldType Type { get; set; } = PivotFieldType.Row;
    public PivotDataType DataType { get; set; } = PivotDataType.String;
    public PivotAggregateFunction AggregateFunction { get; set; } = PivotAggregateFunction.Sum;
    public string? Format { get; set; }
    public bool ShowSubTotals { get; set; } = true;
    public bool ShowGrandTotals { get; set; } = true;
    public PivotSortOrder SortOrder { get; set; } = PivotSortOrder.Ascending;
    public bool Expanded { get; set; } = true;
    public List<string> FilterValues { get; set; } = new();
    public Func<object, string>? ValueFormatter { get; set; }
}

/// <summary>
/// Pivot table configuration
/// </summary>
public class PivotConfiguration
{
    public List<PivotField> RowFields { get; set; } = new();
    public List<PivotField> ColumnFields { get; set; } = new();
    public List<PivotField> ValueFields { get; set; } = new();
    public List<PivotField> FilterFields { get; set; } = new();
    public bool ShowRowHeaders { get; set; } = true;
    public bool ShowColumnHeaders { get; set; } = true;
    public bool ShowGrandTotals { get; set; } = true;
    public bool ShowSubTotals { get; set; } = true;
    public bool AllowDrillDown { get; set; } = true;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public PivotDisplayMode DisplayMode { get; set; } = PivotDisplayMode.Grid;
}

/// <summary>
/// Pivot cell data
/// </summary>
public class PivotCell
{
    public object? Value { get; set; }
    public string FormattedValue { get; set; } = string.Empty;
    public PivotCellType CellType { get; set; } = PivotCellType.Value;
    public bool IsGrandTotal { get; set; } = false;
    public bool IsSubTotal { get; set; } = false;
    public int RowSpan { get; set; } = 1;
    public int ColSpan { get; set; } = 1;
    public string? CssClass { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Pivot row data
/// </summary>
public class PivotRow
{
    public List<PivotCell> Cells { get; set; } = new();
    public int Level { get; set; } = 0;
    public bool IsExpanded { get; set; } = true;
    public bool IsTotal { get; set; } = false;
    public string? GroupKey { get; set; }
    public Dictionary<string, object> RowData { get; set; } = new();
}

/// <summary>
/// Pivot table result
/// </summary>
public class PivotResult
{
    public List<PivotRow> Rows { get; set; } = new();
    public List<string> ColumnHeaders { get; set; } = new();
    public List<List<PivotCell>> HeaderRows { get; set; } = new();
    public PivotStatistics Statistics { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Pivot statistics
/// </summary>
public class PivotStatistics
{
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public int DataRows { get; set; }
    public int GroupedRows { get; set; }
    public double GrandTotal { get; set; }
    public Dictionary<string, double> SubTotals { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Pivot filter criteria
/// </summary>
public class PivotFilter
{
    public string FieldName { get; set; } = string.Empty;
    public PivotFilterOperator Operator { get; set; } = PivotFilterOperator.Equal;
    public object? Value { get; set; }
    public List<object> Values { get; set; } = new();
}

/// <summary>
/// Pivot drill-down data
/// </summary>
public class PivotDrillDownData
{
    public string RowField { get; set; } = string.Empty;
    public string ColumnField { get; set; } = string.Empty;
    public object? RowValue { get; set; }
    public object? ColumnValue { get; set; }
    public List<Dictionary<string, object>> DetailRecords { get; set; } = new();
}

/// <summary>
/// Pivot export settings
/// </summary>
public class PivotExportSettings
{
    public PivotExportFormat Format { get; set; } = PivotExportFormat.Excel;
    public string FileName { get; set; } = "PivotTable";
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeFilters { get; set; } = true;
    public bool IncludeFormatting { get; set; } = true;
}

/// <summary>
/// Pivot chart configuration
/// </summary>
public class PivotChartConfiguration
{
    public PivotChartType ChartType { get; set; } = PivotChartType.Column;
    public string Title { get; set; } = string.Empty;
    public bool ShowLegend { get; set; } = true;
    public bool ShowDataLabels { get; set; } = false;
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 400;
}

/// <summary>
/// Pivot field list item
/// </summary>
public class PivotFieldListItem
{
    public string Name { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public PivotDataType DataType { get; set; } = PivotDataType.String;
    public bool IsSelected { get; set; } = false;
    public PivotFieldType? AssignedType { get; set; }
}

// Enums
public enum PivotFieldType
{
    Row,
    Column,
    Value,
    Filter
}

public enum PivotDataType
{
    String,
    Number,
    Date,
    Boolean,
    Currency,
    Percentage
}

public enum PivotAggregateFunction
{
    Sum,
    Average,
    Count,
    Min,
    Max,
    DistinctCount,
    Product,
    StdDev,
    Variance
}

public enum PivotSortOrder
{
    None,
    Ascending,
    Descending
}

public enum PivotCellType
{
    Value,
    RowHeader,
    ColumnHeader,
    GrandTotal,
    SubTotal,
    Empty
}

public enum PivotDisplayMode
{
    Grid,
    Chart,
    GridAndChart
}

public enum PivotFilterOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    Between,
    In
}

public enum PivotExportFormat
{
    Excel,
    Csv,
    Pdf,
    Json
}

public enum PivotChartType
{
    Column,
    Bar,
    Line,
    Area,
    Pie,
    Doughnut,
    Scatter
}
