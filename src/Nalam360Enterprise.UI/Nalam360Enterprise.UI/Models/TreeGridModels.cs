using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Represents a node in the tree grid
/// </summary>
/// <typeparam name="T">Type of data stored in the node</typeparam>
public class TreeGridNode<T>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentId { get; set; }
    public T Data { get; set; } = default!;
    public List<TreeGridNode<T>> Children { get; set; } = new();
    public bool IsExpanded { get; set; } = false;
    public bool IsSelected { get; set; } = false;
    public int Level { get; set; } = 0;
    public bool HasChildren => Children?.Count > 0;
    public bool IsLeaf => !HasChildren;
    public string? Icon { get; set; }
    public bool IsLoading { get; set; } = false;
    public bool AllowEdit { get; set; } = true;
    public bool AllowDelete { get; set; } = true;
    public Dictionary<string, object> CustomData { get; set; } = new();
}

/// <summary>
/// Column definition for tree grid
/// </summary>
public class TreeGridColumn
{
    public string Field { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public string? Format { get; set; }
    public string? Type { get; set; } = "string"; // string, number, date, boolean
    public int Width { get; set; } = 150;
    public bool Visible { get; set; } = true;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool AllowEditing { get; set; } = true;
    public bool IsTreeColumn { get; set; } = false; // The column that shows the tree structure
    public string? TextAlign { get; set; } = "Left"; // Left, Right, Center
    public string? Template { get; set; }
    public string? EditTemplate { get; set; }
    public bool IsPrimaryKey { get; set; } = false;
    public Func<object, string>? ValueAccessor { get; set; }
    public Func<object, string>? DisplayFormatter { get; set; }
}

/// <summary>
/// Sort descriptor for tree grid
/// </summary>
public class TreeGridSortDescriptor
{
    public string Field { get; set; } = string.Empty;
    public TreeGridSortDirection Direction { get; set; } = TreeGridSortDirection.Ascending;
}

/// <summary>
/// Filter descriptor for tree grid
/// </summary>
public class TreeGridFilterDescriptor
{
    public string Field { get; set; } = string.Empty;
    public TreeGridFilterOperator Operator { get; set; } = TreeGridFilterOperator.Contains;
    public object? Value { get; set; }
    public TreeGridFilterLogic Logic { get; set; } = TreeGridFilterLogic.And;
}

/// <summary>
/// Edit settings for tree grid
/// </summary>
public class TreeGridEditSettings
{
    public TreeGridEditMode Mode { get; set; } = TreeGridEditMode.Row;
    public bool AllowAdding { get; set; } = true;
    public bool AllowEditing { get; set; } = true;
    public bool AllowDeleting { get; set; } = true;
    public bool ShowDeleteConfirm { get; set; } = true;
    public string NewRowPosition { get; set; } = "Below"; // Above, Below, Child, Top, Bottom
}

/// <summary>
/// Selection settings for tree grid
/// </summary>
public class TreeGridSelectionSettings
{
    public TreeGridSelectionMode Mode { get; set; } = TreeGridSelectionMode.Row;
    public TreeGridSelectionType Type { get; set; } = TreeGridSelectionType.Single;
    public bool EnableToggle { get; set; } = true;
    public bool PersistSelection { get; set; } = true;
    public bool CheckboxMode { get; set; } = false;
    public string? CheckboxField { get; set; }
}

/// <summary>
/// Paging settings for tree grid
/// </summary>
public class TreeGridPageSettings
{
    public int PageSize { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalRecords { get; set; } = 0;
    public int[] PageSizes { get; set; } = new[] { 10, 20, 50, 100 };
    public bool ShowPageSizeSelector { get; set; } = true;
}

/// <summary>
/// Context menu item for tree grid
/// </summary>
public class TreeGridContextMenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool Visible { get; set; } = true;
    public bool Disabled { get; set; } = false;
    public string? Target { get; set; } = "Content"; // Header, Content, Both
    public Func<TreeGridContextMenuEventArgs, bool>? VisibleCondition { get; set; }
}

/// <summary>
/// Context menu event arguments
/// </summary>
public class TreeGridContextMenuEventArgs
{
    public object? RowData { get; set; }
    public string? Column { get; set; }
    public TreeGridContextMenuItem? MenuItem { get; set; }
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Row selection event arguments
/// </summary>
/// <typeparam name="T">Type of row data</typeparam>
public class TreeGridRowSelectEventArgs<T>
{
    public TreeGridNode<T>? Node { get; set; }
    public T? Data { get; set; }
    public List<TreeGridNode<T>> SelectedNodes { get; set; } = new();
    public bool IsParentSelection { get; set; } = false;
}

/// <summary>
/// Row action event arguments
/// </summary>
/// <typeparam name="T">Type of row data</typeparam>
public class TreeGridRowActionEventArgs<T>
{
    public TreeGridNode<T>? Node { get; set; }
    public T? Data { get; set; }
    public TreeGridRowAction Action { get; set; }
    public bool Cancel { get; set; } = false;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Cell edit event arguments
/// </summary>
/// <typeparam name="T">Type of row data</typeparam>
public class TreeGridCellEditEventArgs<T>
{
    public TreeGridNode<T>? Node { get; set; }
    public T? Data { get; set; }
    public string Field { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Expand/collapse event arguments
/// </summary>
/// <typeparam name="T">Type of row data</typeparam>
public class TreeGridExpandEventArgs<T>
{
    public TreeGridNode<T>? Node { get; set; }
    public T? Data { get; set; }
    public bool IsExpanding { get; set; }
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Tree grid statistics
/// </summary>
public class TreeGridStatistics
{
    public int TotalNodes { get; set; }
    public int RootNodes { get; set; }
    public int ExpandedNodes { get; set; }
    public int SelectedNodes { get; set; }
    public int MaxDepth { get; set; }
    public int FilteredNodes { get; set; }
}

/// <summary>
/// Export settings for tree grid
/// </summary>
public class TreeGridExportSettings
{
    public TreeGridExportFormat Format { get; set; } = TreeGridExportFormat.Excel;
    public string FileName { get; set; } = "TreeGridExport";
    public bool IncludeHiddenColumns { get; set; } = false;
    public bool ExportCollapsedNodes { get; set; } = false;
    public bool IncludeHeaders { get; set; } = true;
}

// Enums
public enum TreeGridSortDirection
{
    Ascending,
    Descending
}

public enum TreeGridFilterOperator
{
    Equal,
    NotEqual,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    IsNull,
    IsNotNull
}

public enum TreeGridFilterLogic
{
    And,
    Or
}

public enum TreeGridEditMode
{
    Row,
    Cell,
    Dialog,
    Batch
}

public enum TreeGridSelectionMode
{
    Row,
    Cell,
    Both
}

public enum TreeGridSelectionType
{
    Single,
    Multiple
}

public enum TreeGridRowAction
{
    Add,
    Edit,
    Delete,
    Save,
    Cancel
}

public enum TreeGridExportFormat
{
    Excel,
    Csv,
    Pdf
}

public enum TreeGridLoadMode
{
    Lazy,
    Eager
}
