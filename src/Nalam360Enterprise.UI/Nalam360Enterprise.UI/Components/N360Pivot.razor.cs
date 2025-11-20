using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components;

public partial class N360Pivot<TItem>
{
    // Data
    [Parameter] public List<TItem> DataSource { get; set; } = new();
    [Parameter] public PivotConfiguration Configuration { get; set; } = new();
    [Parameter] public List<PivotFieldListItem>? AvailableFields { get; set; }

    // Display
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowFieldList { get; set; } = false;
    [Parameter] public bool ShowStatistics { get; set; } = true;
    [Parameter] public bool ShowDisplayModeToggle { get; set; } = true;
    [Parameter] public string EmptyText { get; set; } = "No data to display. Configure fields to see pivot table.";
    [Parameter] public bool IsResponsive { get; set; } = true;

    // Features
    [Parameter] public bool AllowDrillDown { get; set; } = true;
    [Parameter] public bool AllowExport { get; set; } = true;
    [Parameter] public bool AllowFieldDragDrop { get; set; } = true;
    [Parameter] public PivotDisplayMode CurrentDisplayMode { get; set; } = PivotDisplayMode.Grid;

    // Events
    [Parameter] public EventCallback<PivotConfiguration> OnConfigurationChanged { get; set; }
    [Parameter] public EventCallback<PivotResult> OnPivotCalculated { get; set; }
    [Parameter] public EventCallback<PivotDrillDownData> OnDrillDown { get; set; }
    [Parameter] public EventCallback<PivotExportSettings> OnExport { get; set; }

    // RBAC
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; } = false;

    // Styling
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool IsRTL { get; set; } = false;
    [Parameter] public bool IsLoading { get; set; } = false;

    // Private state
    private PivotResult? _pivotResult;
    private bool _showFieldList = false;
    private bool _showDrillDown = false;
    private PivotDrillDownData? _drillDownData;
    private PivotFieldListItem? _draggingField;
    private PivotFieldType? _targetFieldType;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(RequiredPermission) && HideIfNoPermission)
        {
            if (PermissionService != null && !await PermissionService.HasPermissionAsync(RequiredPermission))
            {
                return;
            }
        }

        _showFieldList = ShowFieldList;
        
        if (Configuration.RowFields.Any() || Configuration.ColumnFields.Any())
        {
            await CalculatePivot();
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (DataSource?.Any() == true && (Configuration.RowFields.Any() || Configuration.ColumnFields.Any()))
        {
            await CalculatePivot();
        }
        await base.OnParametersSetAsync();
    }

    private async Task CalculatePivot()
    {
        IsLoading = true;
        StateHasChanged();

        await Task.Delay(100); // Simulate calculation delay

        try
        {
            _pivotResult = BuildPivotTable();
            await OnPivotCalculated.InvokeAsync(_pivotResult);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private PivotResult BuildPivotTable()
    {
        var result = new PivotResult();

        if (!DataSource.Any() || (!Configuration.RowFields.Any() && !Configuration.ColumnFields.Any()))
        {
            return result;
        }

        // Build column headers
        result.ColumnHeaders = BuildColumnHeaders();
        result.HeaderRows = BuildHeaderRows();

        // Build data rows
        result.Rows = BuildDataRows();

        // Calculate statistics
        result.Statistics = CalculateStatistics(result);

        return result;
    }

    private List<string> BuildColumnHeaders()
    {
        var headers = new List<string>();

        // Add row field headers
        foreach (var field in Configuration.RowFields)
        {
            headers.Add(field.Caption);
        }

        // Add value column headers (simplified - single level)
        if (Configuration.ColumnFields.Any())
        {
            var columnValues = GetDistinctValues(Configuration.ColumnFields.First().Name);
            foreach (var colValue in columnValues)
            {
                foreach (var valueField in Configuration.ValueFields)
                {
                    headers.Add($"{colValue} - {valueField.Caption}");
                }
            }
        }
        else
        {
            foreach (var valueField in Configuration.ValueFields)
            {
                headers.Add(valueField.Caption);
            }
        }

        // Add grand total column
        if (Configuration.ShowGrandTotals)
        {
            headers.Add("Grand Total");
        }

        return headers;
    }

    private List<List<PivotCell>> BuildHeaderRows()
    {
        var headerRows = new List<List<PivotCell>>();
        var headerRow = new List<PivotCell>();

        // Row field headers
        foreach (var field in Configuration.RowFields)
        {
            headerRow.Add(new PivotCell
            {
                FormattedValue = field.Caption,
                CellType = PivotCellType.ColumnHeader,
                CssClass = "row-field-header"
            });
        }

        // Column headers
        if (Configuration.ColumnFields.Any())
        {
            var columnValues = GetDistinctValues(Configuration.ColumnFields.First().Name);
            foreach (var colValue in columnValues)
            {
                foreach (var valueField in Configuration.ValueFields)
                {
                    headerRow.Add(new PivotCell
                    {
                        FormattedValue = $"{colValue}",
                        CellType = PivotCellType.ColumnHeader,
                        CssClass = "value-header"
                    });
                }
            }
        }
        else
        {
            foreach (var valueField in Configuration.ValueFields)
            {
                headerRow.Add(new PivotCell
                {
                    FormattedValue = valueField.Caption,
                    CellType = PivotCellType.ColumnHeader,
                    CssClass = "value-header"
                });
            }
        }

        // Grand total header
        if (Configuration.ShowGrandTotals)
        {
            headerRow.Add(new PivotCell
            {
                FormattedValue = "Grand Total",
                CellType = PivotCellType.ColumnHeader,
                CssClass = "grand-total-header"
            });
        }

        headerRows.Add(headerRow);
        return headerRows;
    }

    private List<PivotRow> BuildDataRows()
    {
        var rows = new List<PivotRow>();

        if (!Configuration.RowFields.Any())
        {
            // No row grouping - single summary row
            rows.Add(BuildSummaryRow(DataSource.ToList()));
            return rows;
        }

        // Group by first row field
        var firstRowField = Configuration.RowFields.First();
        var groups = GroupByField(DataSource.ToList(), firstRowField.Name);

        foreach (var group in groups.OrderBy(g => g.Key))
        {
            var row = BuildDataRow(group.Key?.ToString() ?? "", group.ToList(), 0);
            rows.Add(row);

            // Add subtotal if needed
            if (Configuration.ShowSubTotals && Configuration.ValueFields.Any())
            {
                var subtotalRow = BuildSubTotalRow(group.ToList(), 0);
                subtotalRow.IsTotal = true;
                rows.Add(subtotalRow);
            }
        }

        // Add grand total row
        if (Configuration.ShowGrandTotals)
        {
            var grandTotalRow = BuildGrandTotalRow();
            rows.Add(grandTotalRow);
        }

        return rows;
    }

    private PivotRow BuildDataRow(string rowValue, List<TItem> groupData, int level)
    {
        var row = new PivotRow
        {
            Level = level,
            GroupKey = rowValue,
            IsExpanded = true
        };

        // Row header cell
        row.Cells.Add(new PivotCell
        {
            FormattedValue = rowValue,
            CellType = PivotCellType.RowHeader,
            CssClass = "row-header"
        });

        // Value cells
        if (Configuration.ColumnFields.Any())
        {
            var columnValues = GetDistinctValues(Configuration.ColumnFields.First().Name);
            foreach (var colValue in columnValues)
            {
                var cellData = groupData.Where(item => GetFieldValue(item, Configuration.ColumnFields.First().Name)?.ToString() == colValue?.ToString()).ToList();
                
                foreach (var valueField in Configuration.ValueFields)
                {
                    var cellValue = CalculateAggregate(cellData, valueField);
                    row.Cells.Add(new PivotCell
                    {
                        Value = cellValue,
                        FormattedValue = FormatValue(cellValue, valueField),
                        CellType = PivotCellType.Value
                    });
                }
            }
        }
        else
        {
            foreach (var valueField in Configuration.ValueFields)
            {
                var cellValue = CalculateAggregate(groupData, valueField);
                row.Cells.Add(new PivotCell
                {
                    Value = cellValue,
                    FormattedValue = FormatValue(cellValue, valueField),
                    CellType = PivotCellType.Value
                });
            }
        }

        // Grand total cell
        if (Configuration.ShowGrandTotals)
        {
            var grandTotal = CalculateRowTotal(groupData);
            row.Cells.Add(new PivotCell
            {
                Value = grandTotal,
                FormattedValue = grandTotal.ToString("N2"),
                CellType = PivotCellType.Value,
                IsGrandTotal = true
            });
        }

        return row;
    }

    private PivotRow BuildSummaryRow(List<TItem> data)
    {
        return BuildDataRow("Total", data, 0);
    }

    private PivotRow BuildSubTotalRow(List<TItem> groupData, int level)
    {
        var row = new PivotRow
        {
            Level = level,
            IsTotal = true
        };

        row.Cells.Add(new PivotCell
        {
            FormattedValue = "Subtotal",
            CellType = PivotCellType.RowHeader,
            CssClass = "subtotal-header"
        });

        if (Configuration.ColumnFields.Any())
        {
            var columnValues = GetDistinctValues(Configuration.ColumnFields.First().Name);
            foreach (var colValue in columnValues)
            {
                var cellData = groupData.Where(item => GetFieldValue(item, Configuration.ColumnFields.First().Name)?.ToString() == colValue?.ToString()).ToList();
                
                foreach (var valueField in Configuration.ValueFields)
                {
                    var cellValue = CalculateAggregate(cellData, valueField);
                    row.Cells.Add(new PivotCell
                    {
                        Value = cellValue,
                        FormattedValue = FormatValue(cellValue, valueField),
                        IsSubTotal = true
                    });
                }
            }
        }
        else
        {
            foreach (var valueField in Configuration.ValueFields)
            {
                var cellValue = CalculateAggregate(groupData, valueField);
                row.Cells.Add(new PivotCell
                {
                    Value = cellValue,
                    FormattedValue = FormatValue(cellValue, valueField),
                    IsSubTotal = true
                });
            }
        }

        if (Configuration.ShowGrandTotals)
        {
            var total = CalculateRowTotal(groupData);
            row.Cells.Add(new PivotCell
            {
                Value = total,
                FormattedValue = total.ToString("N2"),
                IsSubTotal = true
            });
        }

        return row;
    }

    private PivotRow BuildGrandTotalRow()
    {
        var row = new PivotRow
        {
            Level = 0,
            IsTotal = true
        };

        row.Cells.Add(new PivotCell
        {
            FormattedValue = "Grand Total",
            CellType = PivotCellType.RowHeader,
            CssClass = "grand-total-header"
        });

        if (Configuration.ColumnFields.Any())
        {
            var columnValues = GetDistinctValues(Configuration.ColumnFields.First().Name);
            foreach (var colValue in columnValues)
            {
                var cellData = DataSource.Where(item => GetFieldValue(item, Configuration.ColumnFields.First().Name)?.ToString() == colValue?.ToString()).ToList();
                
                foreach (var valueField in Configuration.ValueFields)
                {
                    var cellValue = CalculateAggregate(cellData, valueField);
                    row.Cells.Add(new PivotCell
                    {
                        Value = cellValue,
                        FormattedValue = FormatValue(cellValue, valueField),
                        IsGrandTotal = true
                    });
                }
            }
        }
        else
        {
            foreach (var valueField in Configuration.ValueFields)
            {
                var cellValue = CalculateAggregate(DataSource.ToList(), valueField);
                row.Cells.Add(new PivotCell
                {
                    Value = cellValue,
                    FormattedValue = FormatValue(cellValue, valueField),
                    IsGrandTotal = true
                });
            }
        }

        if (Configuration.ShowGrandTotals)
        {
            var total = CalculateRowTotal(DataSource.ToList());
            row.Cells.Add(new PivotCell
            {
                Value = total,
                FormattedValue = total.ToString("N2"),
                IsGrandTotal = true
            });
        }

        return row;
    }

    private IEnumerable<IGrouping<object?, TItem>> GroupByField(List<TItem> data, string fieldName)
    {
        return data.GroupBy(item => GetFieldValue(item, fieldName));
    }

    private object? GetFieldValue(TItem item, string fieldName)
    {
        if (item == null) return null;
        
        var property = typeof(TItem).GetProperty(fieldName);
        return property?.GetValue(item);
    }

    private List<object?> GetDistinctValues(string fieldName)
    {
        return DataSource.Select(item => GetFieldValue(item, fieldName)).Distinct().OrderBy(v => v).ToList();
    }

    private double CalculateAggregate(List<TItem> data, PivotField field)
    {
        if (!data.Any()) return 0;

        var values = data.Select(item => GetFieldValue(item, field.Name))
                        .Where(v => v != null)
                        .Select(v => Convert.ToDouble(v))
                        .ToList();

        if (!values.Any()) return 0;

        return field.AggregateFunction switch
        {
            PivotAggregateFunction.Sum => values.Sum(),
            PivotAggregateFunction.Average => values.Average(),
            PivotAggregateFunction.Count => values.Count,
            PivotAggregateFunction.Min => values.Min(),
            PivotAggregateFunction.Max => values.Max(),
            PivotAggregateFunction.DistinctCount => values.Distinct().Count(),
            _ => values.Sum()
        };
    }

    private double CalculateRowTotal(List<TItem> data)
    {
        if (!Configuration.ValueFields.Any()) return 0;
        
        var firstValueField = Configuration.ValueFields.First();
        return CalculateAggregate(data, firstValueField);
    }

    private string FormatValue(double value, PivotField field)
    {
        if (!string.IsNullOrEmpty(field.Format))
        {
            return value.ToString(field.Format);
        }

        return field.DataType switch
        {
            PivotDataType.Currency => value.ToString("C2"),
            PivotDataType.Percentage => value.ToString("P2"),
            _ => value.ToString("N2")
        };
    }

    private PivotStatistics CalculateStatistics(PivotResult result)
    {
        return new PivotStatistics
        {
            TotalRows = result.Rows.Count,
            TotalColumns = result.ColumnHeaders.Count,
            DataRows = result.Rows.Count(r => !r.IsTotal),
            GroupedRows = result.Rows.Count(r => r.Level > 0),
            GrandTotal = result.Rows.LastOrDefault()?.Cells.LastOrDefault()?.Value as double? ?? 0
        };
    }

    private void ToggleFieldList()
    {
        _showFieldList = !_showFieldList;
        StateHasChanged();
    }

    private async Task RefreshPivot()
    {
        await CalculatePivot();
    }

    private async Task ChangeDisplayMode(PivotDisplayMode mode)
    {
        CurrentDisplayMode = mode;
        await Task.CompletedTask;
        StateHasChanged();
    }

    private void OnFieldDragStart(PivotFieldListItem field)
    {
        _draggingField = field;
    }

    private async Task OnFieldDrop(PivotFieldType targetType)
    {
        if (_draggingField == null) return;

        var newField = new PivotField
        {
            Name = _draggingField.Name,
            Caption = _draggingField.Caption,
            DataType = _draggingField.DataType,
            Type = targetType,
            AggregateFunction = _draggingField.DataType == PivotDataType.Number ? PivotAggregateFunction.Sum : PivotAggregateFunction.Count
        };

        switch (targetType)
        {
            case PivotFieldType.Row:
                Configuration.RowFields.Add(newField);
                break;
            case PivotFieldType.Column:
                Configuration.ColumnFields.Add(newField);
                break;
            case PivotFieldType.Value:
                Configuration.ValueFields.Add(newField);
                break;
            case PivotFieldType.Filter:
                Configuration.FilterFields.Add(newField);
                break;
        }

        _draggingField.IsSelected = true;
        _draggingField = null;

        await CalculatePivot();
        await OnConfigurationChanged.InvokeAsync(Configuration);
    }

    private async Task RemoveField(PivotField field, PivotFieldType type)
    {
        switch (type)
        {
            case PivotFieldType.Row:
                Configuration.RowFields.Remove(field);
                break;
            case PivotFieldType.Column:
                Configuration.ColumnFields.Remove(field);
                break;
            case PivotFieldType.Value:
                Configuration.ValueFields.Remove(field);
                break;
            case PivotFieldType.Filter:
                Configuration.FilterFields.Remove(field);
                break;
        }

        if (AvailableFields != null)
        {
            var availableField = AvailableFields.FirstOrDefault(f => f.Name == field.Name);
            if (availableField != null)
            {
                availableField.IsSelected = false;
            }
        }

        await CalculatePivot();
        await OnConfigurationChanged.InvokeAsync(Configuration);
    }

    private async Task ClearAllFields()
    {
        Configuration.RowFields.Clear();
        Configuration.ColumnFields.Clear();
        Configuration.ValueFields.Clear();
        Configuration.FilterFields.Clear();

        if (AvailableFields != null)
        {
            foreach (var field in AvailableFields)
            {
                field.IsSelected = false;
            }
        }

        _pivotResult = null;
        await OnConfigurationChanged.InvokeAsync(Configuration);
        StateHasChanged();
    }

    private async Task ApplyConfiguration()
    {
        await CalculatePivot();
        await OnConfigurationChanged.InvokeAsync(Configuration);
    }

    private async Task OnCellClick(PivotRow row, PivotCell cell)
    {
        if (!AllowDrillDown || row.IsTotal) return;

        // Simulate drill-down data
        _drillDownData = new PivotDrillDownData
        {
            RowField = Configuration.RowFields.FirstOrDefault()?.Caption ?? "",
            ColumnField = Configuration.ColumnFields.FirstOrDefault()?.Caption ?? "",
            RowValue = row.GroupKey,
            ColumnValue = cell.Value,
            DetailRecords = new List<Dictionary<string, object>>()
        };

        _showDrillDown = true;
        await OnDrillDown.InvokeAsync(_drillDownData);
        StateHasChanged();
    }

    private void CloseDrillDown()
    {
        _showDrillDown = false;
        _drillDownData = null;
        StateHasChanged();
    }

    private void ToggleRowExpand(PivotRow row)
    {
        row.IsExpanded = !row.IsExpanded;
        StateHasChanged();
    }

    private async Task ExportData()
    {
        var settings = new PivotExportSettings
        {
            Format = PivotExportFormat.Excel,
            FileName = $"PivotTable_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        await OnExport.InvokeAsync(settings);
    }

    private async Task ExportDrillDown()
    {
        var settings = new PivotExportSettings
        {
            Format = PivotExportFormat.Excel,
            FileName = $"PivotDrillDown_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        await OnExport.InvokeAsync(settings);
    }

    private string GetFieldIcon(PivotDataType dataType)
    {
        return dataType switch
        {
            PivotDataType.Number => "🔢",
            PivotDataType.String => "📝",
            PivotDataType.Date => "📅",
            PivotDataType.Boolean => "☑️",
            PivotDataType.Currency => "💰",
            PivotDataType.Percentage => "📊",
            _ => "📄"
        };
    }

    private string GetAggregateLabel(PivotField field)
    {
        return $"{field.AggregateFunction} of {field.Caption}";
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            { "role", "grid" },
            { "aria-label", "Pivot Table" }
        };

        if (IsRTL)
        {
            attributes.Add("dir", "rtl");
        }

        return attributes;
    }
}
