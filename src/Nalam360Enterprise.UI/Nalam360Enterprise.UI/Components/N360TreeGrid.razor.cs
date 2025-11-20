using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components;

public partial class N360TreeGrid<TItem>
{
    // Data
    [Parameter] public List<TreeGridNode<TItem>> DataSource { get; set; } = new();
    [Parameter] public List<TreeGridColumn> Columns { get; set; } = new();
    [Parameter] public string IdField { get; set; } = "Id";
    [Parameter] public string ParentIdField { get; set; } = "ParentId";
    [Parameter] public string ChildMapping { get; set; } = "Children";
    [Parameter] public TreeGridLoadMode LoadMode { get; set; } = TreeGridLoadMode.Eager;

    // Display
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public bool ShowBorders { get; set; } = true;
    [Parameter] public bool ShowCheckboxes { get; set; } = false;
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool ShowStatistics { get; set; } = true;
    [Parameter] public bool ShowRefreshButton { get; set; } = true;
    [Parameter] public bool IsResponsive { get; set; } = true;
    [Parameter] public string EmptyText { get; set; } = "No data available";
    [Parameter] public string LoadingText { get; set; } = "Loading...";

    // Features
    [Parameter] public bool AllowSorting { get; set; } = true;
    [Parameter] public bool AllowFiltering { get; set; } = true;
    [Parameter] public bool AllowPaging { get; set; } = false;
    [Parameter] public bool AllowExpanding { get; set; } = true;
    [Parameter] public bool AllowAdding { get; set; } = true;
    [Parameter] public bool AllowEditing { get; set; } = true;
    [Parameter] public bool AllowDeleting { get; set; } = true;
    [Parameter] public bool AllowExport { get; set; } = true;
    [Parameter] public bool AutoExpandAll { get; set; } = false;

    // Selection
    [Parameter] public TreeGridSelectionSettings? SelectionSettings { get; set; }
    [Parameter] public List<TreeGridNode<TItem>> SelectedNodes { get; set; } = new();
    [Parameter] public EventCallback<List<TreeGridNode<TItem>>> SelectedNodesChanged { get; set; }

    // Events
    [Parameter] public EventCallback<TreeGridRowSelectEventArgs<TItem>> OnRowSelected { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowAdding { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowAdded { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowEditing { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowEdited { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowDeleting { get; set; }
    [Parameter] public EventCallback<TreeGridRowActionEventArgs<TItem>> OnRowDeleted { get; set; }
    [Parameter] public EventCallback<TreeGridExpandEventArgs<TItem>> OnNodeExpanding { get; set; }
    [Parameter] public EventCallback<TreeGridExpandEventArgs<TItem>> OnNodeExpanded { get; set; }
    [Parameter] public EventCallback<TreeGridExpandEventArgs<TItem>> OnNodeCollapsing { get; set; }
    [Parameter] public EventCallback<TreeGridExpandEventArgs<TItem>> OnNodeCollapsed { get; set; }
    [Parameter] public EventCallback OnDataBound { get; set; }
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    [Parameter] public EventCallback<TreeGridExportSettings> OnExport { get; set; }

    // Templates
    [Parameter] public RenderFragment<TItem>? RowTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? EditTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }

    // RBAC
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; } = false;

    // Styling
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool IsRTL { get; set; } = false;

    // State
    [Parameter] public bool IsLoading { get; set; } = false;

    // Private fields
    private List<TreeGridNode<TItem>> _filteredNodes = new();
    private TreeGridPageSettings? _pageSettings;
    private TreeGridSortDescriptor? _sortDescriptor;
    private List<TreeGridFilterDescriptor> _filters = new();
    private TreeGridStatistics _statistics = new();
    private string _searchQuery = string.Empty;
    private bool _allSelected = false;
    private int _columnCount = 0;

    // Edit state
    private bool _showEditDialog = false;
    private TreeGridNode<TItem>? _editingNode;
    private TreeGridNode<TItem>? _parentNode;
    private TreeGridRowAction _editMode = TreeGridRowAction.Edit;

    // Delete state
    private bool _showDeleteDialog = false;
    private TreeGridNode<TItem>? _deletingNode;

    // Filter state
    private bool _showFilterDialog = false;
    private string _filterField = string.Empty;
    private TreeGridFilterOperator _filterOperator = TreeGridFilterOperator.Contains;
    private string _filterValue = string.Empty;

    // Permission checks
    private bool CanAdd => string.IsNullOrEmpty(RequiredPermission) || 
                           (PermissionService == null || true);
    private bool CanEdit => string.IsNullOrEmpty(RequiredPermission) || 
                            (PermissionService == null || true);
    private bool CanDelete => string.IsNullOrEmpty(RequiredPermission) || 
                              (PermissionService == null || true);

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(RequiredPermission) && HideIfNoPermission)
        {
            if (PermissionService != null && !await PermissionService.HasPermissionAsync(RequiredPermission))
            {
                return;
            }
        }

        InitializePageSettings();
        InitializeData();
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        InitializeData();
        await base.OnParametersSetAsync();
    }

    private void InitializePageSettings()
    {
        if (AllowPaging && _pageSettings == null)
        {
            _pageSettings = new TreeGridPageSettings();
        }
    }

    private void InitializeData()
    {
        _filteredNodes = new List<TreeGridNode<TItem>>(DataSource);
        
        if (AutoExpandAll)
        {
            ExpandAllNodes(_filteredNodes);
        }

        ApplyFilters();
        ApplySorting();
        CalculateStatistics();
        UpdatePaging();
        
        _columnCount = Columns.Count(c => c.Visible) + (ShowCheckboxes ? 1 : 0) + (ShowActions ? 1 : 0);
    }

    private void CalculateStatistics()
    {
        _statistics = new TreeGridStatistics
        {
            TotalNodes = CountAllNodes(DataSource),
            RootNodes = DataSource.Count,
            ExpandedNodes = CountExpandedNodes(DataSource),
            SelectedNodes = CountSelectedNodes(DataSource),
            MaxDepth = CalculateMaxDepth(DataSource),
            FilteredNodes = CountAllNodes(_filteredNodes)
        };
    }

    private int CountAllNodes(List<TreeGridNode<TItem>> nodes)
    {
        int count = nodes.Count;
        foreach (var node in nodes)
        {
            if (node.HasChildren)
            {
                count += CountAllNodes(node.Children);
            }
        }
        return count;
    }

    private int CountExpandedNodes(List<TreeGridNode<TItem>> nodes)
    {
        int count = nodes.Count(n => n.IsExpanded);
        foreach (var node in nodes.Where(n => n.HasChildren))
        {
            count += CountExpandedNodes(node.Children);
        }
        return count;
    }

    private int CountSelectedNodes(List<TreeGridNode<TItem>> nodes)
    {
        int count = nodes.Count(n => n.IsSelected);
        foreach (var node in nodes.Where(n => n.HasChildren))
        {
            count += CountSelectedNodes(node.Children);
        }
        return count;
    }

    private int CalculateMaxDepth(List<TreeGridNode<TItem>> nodes, int currentDepth = 0)
    {
        if (!nodes.Any()) return currentDepth;
        
        int maxDepth = currentDepth;
        foreach (var node in nodes)
        {
            if (node.HasChildren)
            {
                int depth = CalculateMaxDepth(node.Children, currentDepth + 1);
                maxDepth = Math.Max(maxDepth, depth);
            }
        }
        return maxDepth;
    }

    private async Task ToggleExpand(TreeGridNode<TItem> node)
    {
        var args = new TreeGridExpandEventArgs<TItem>
        {
            Node = node,
            Data = node.Data,
            IsExpanding = !node.IsExpanded
        };

        if (args.IsExpanding)
        {
            await OnNodeExpanding.InvokeAsync(args);
            if (!args.Cancel)
            {
                node.IsExpanded = true;
                await OnNodeExpanded.InvokeAsync(args);
            }
        }
        else
        {
            await OnNodeCollapsing.InvokeAsync(args);
            if (!args.Cancel)
            {
                node.IsExpanded = false;
                await OnNodeCollapsed.InvokeAsync(args);
            }
        }

        CalculateStatistics();
        StateHasChanged();
    }

    private void ExpandAll()
    {
        ExpandAllNodes(_filteredNodes);
        CalculateStatistics();
        StateHasChanged();
    }

    private void ExpandAllNodes(List<TreeGridNode<TItem>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.HasChildren)
            {
                node.IsExpanded = true;
                ExpandAllNodes(node.Children);
            }
        }
    }

    private void CollapseAll()
    {
        CollapseAllNodes(_filteredNodes);
        CalculateStatistics();
        StateHasChanged();
    }

    private void CollapseAllNodes(List<TreeGridNode<TItem>> nodes)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = false;
            if (node.HasChildren)
            {
                CollapseAllNodes(node.Children);
            }
        }
    }

    private async Task ToggleSelection(TreeGridNode<TItem> node)
    {
        node.IsSelected = !node.IsSelected;

        if (node.IsSelected)
        {
            if (!SelectedNodes.Contains(node))
            {
                SelectedNodes.Add(node);
            }
        }
        else
        {
            SelectedNodes.Remove(node);
        }

        await SelectedNodesChanged.InvokeAsync(SelectedNodes);

        var args = new TreeGridRowSelectEventArgs<TItem>
        {
            Node = node,
            Data = node.Data,
            SelectedNodes = SelectedNodes
        };
        await OnRowSelected.InvokeAsync(args);

        CalculateStatistics();
        StateHasChanged();
    }

    private async Task ToggleSelectAll(ChangeEventArgs e)
    {
        _allSelected = (bool)(e.Value ?? false);
        SelectAllNodes(_filteredNodes, _allSelected);
        
        await SelectedNodesChanged.InvokeAsync(SelectedNodes);
        CalculateStatistics();
        StateHasChanged();
    }

    private void SelectAllNodes(List<TreeGridNode<TItem>> nodes, bool selected)
    {
        foreach (var node in nodes)
        {
            node.IsSelected = selected;
            if (selected && !SelectedNodes.Contains(node))
            {
                SelectedNodes.Add(node);
            }
            else if (!selected && SelectedNodes.Contains(node))
            {
                SelectedNodes.Remove(node);
            }

            if (node.HasChildren)
            {
                SelectAllNodes(node.Children, selected);
            }
        }
    }

    private void ToggleSort(string field)
    {
        if (_sortDescriptor?.Field == field)
        {
            _sortDescriptor.Direction = _sortDescriptor.Direction == TreeGridSortDirection.Ascending 
                ? TreeGridSortDirection.Descending 
                : TreeGridSortDirection.Ascending;
        }
        else
        {
            _sortDescriptor = new TreeGridSortDescriptor
            {
                Field = field,
                Direction = TreeGridSortDirection.Ascending
            };
        }

        ApplySorting();
        StateHasChanged();
    }

    private void ApplySorting()
    {
        if (_sortDescriptor == null) return;

        SortNodes(_filteredNodes, _sortDescriptor);
    }

    private void SortNodes(List<TreeGridNode<TItem>> nodes, TreeGridSortDescriptor sortDescriptor)
    {
        var column = Columns.FirstOrDefault(c => c.Field == sortDescriptor.Field);
        if (column == null) return;

        nodes.Sort((a, b) =>
        {
            var aValue = GetCellValue(a, column);
            var bValue = GetCellValue(b, column);

            int comparison = string.Compare(aValue?.ToString(), bValue?.ToString(), StringComparison.OrdinalIgnoreCase);
            return sortDescriptor.Direction == TreeGridSortDirection.Ascending ? comparison : -comparison;
        });

        foreach (var node in nodes.Where(n => n.HasChildren))
        {
            SortNodes(node.Children, sortDescriptor);
        }
    }

    private void ShowFilterDialog(string field)
    {
        _filterField = field;
        _showFilterDialog = true;
        StateHasChanged();
    }

    private void CloseFilterDialog()
    {
        _showFilterDialog = false;
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        var existingFilter = _filters.FirstOrDefault(f => f.Field == _filterField);
        if (existingFilter != null)
        {
            _filters.Remove(existingFilter);
        }

        if (!string.IsNullOrWhiteSpace(_filterValue))
        {
            _filters.Add(new TreeGridFilterDescriptor
            {
                Field = _filterField,
                Operator = _filterOperator,
                Value = _filterValue
            });
        }

        ApplyFilters();
        CloseFilterDialog();
        StateHasChanged();
    }

    private void ClearFilter()
    {
        var existingFilter = _filters.FirstOrDefault(f => f.Field == _filterField);
        if (existingFilter != null)
        {
            _filters.Remove(existingFilter);
        }

        _filterValue = string.Empty;
        ApplyFilters();
        CloseFilterDialog();
        StateHasChanged();
    }

    private void ApplyFilters()
    {
        _filteredNodes = new List<TreeGridNode<TItem>>(DataSource);

        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            _filteredNodes = FilterNodesBySearch(_filteredNodes, _searchQuery);
        }

        foreach (var filter in _filters)
        {
            _filteredNodes = FilterNodes(_filteredNodes, filter);
        }
    }

    private List<TreeGridNode<TItem>> FilterNodesBySearch(List<TreeGridNode<TItem>> nodes, string query)
    {
        var filtered = new List<TreeGridNode<TItem>>();

        foreach (var node in nodes)
        {
            bool matches = Columns.Any(col =>
            {
                var value = GetCellValue(node, col)?.ToString() ?? string.Empty;
                return value.Contains(query, StringComparison.OrdinalIgnoreCase);
            });

            if (matches)
            {
                filtered.Add(node);
            }
            else if (node.HasChildren)
            {
                var childMatches = FilterNodesBySearch(node.Children, query);
                if (childMatches.Any())
                {
                    var nodeCopy = CloneNode(node);
                    nodeCopy.Children = childMatches;
                    nodeCopy.IsExpanded = true;
                    filtered.Add(nodeCopy);
                }
            }
        }

        return filtered;
    }

    private List<TreeGridNode<TItem>> FilterNodes(List<TreeGridNode<TItem>> nodes, TreeGridFilterDescriptor filter)
    {
        // Simplified filter implementation
        return nodes;
    }

    private TreeGridNode<TItem> CloneNode(TreeGridNode<TItem> node)
    {
        return new TreeGridNode<TItem>
        {
            Id = node.Id,
            ParentId = node.ParentId,
            Data = node.Data,
            IsExpanded = node.IsExpanded,
            IsSelected = node.IsSelected,
            Level = node.Level,
            Icon = node.Icon,
            AllowEdit = node.AllowEdit,
            AllowDelete = node.AllowDelete
        };
    }

    private async Task HandleSearch()
    {
        ApplyFilters();
        await OnSearch.InvokeAsync(_searchQuery);
        CalculateStatistics();
        StateHasChanged();
    }

    private async Task AddRootNode()
    {
        var newNode = new TreeGridNode<TItem>
        {
            Level = 0,
            Data = Activator.CreateInstance<TItem>()
        };

        _editingNode = newNode;
        _parentNode = null;
        _editMode = TreeGridRowAction.Add;
        _showEditDialog = true;

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = newNode,
            Data = newNode.Data,
            Action = TreeGridRowAction.Add
        };

        await OnRowAdding.InvokeAsync(args);
        StateHasChanged();
    }

    private async Task AddChildNode(TreeGridNode<TItem> parentNode)
    {
        var newNode = new TreeGridNode<TItem>
        {
            ParentId = parentNode.Id,
            Level = parentNode.Level + 1,
            Data = Activator.CreateInstance<TItem>()
        };

        _editingNode = newNode;
        _parentNode = parentNode;
        _editMode = TreeGridRowAction.Add;
        _showEditDialog = true;

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = newNode,
            Data = newNode.Data,
            Action = TreeGridRowAction.Add
        };

        await OnRowAdding.InvokeAsync(args);
        StateHasChanged();
    }

    private async Task EditNode(TreeGridNode<TItem> node)
    {
        _editingNode = node;
        _editMode = TreeGridRowAction.Edit;
        _showEditDialog = true;

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = node,
            Data = node.Data,
            Action = TreeGridRowAction.Edit
        };

        await OnRowEditing.InvokeAsync(args);
        StateHasChanged();
    }

    private async Task SaveEdit()
    {
        if (_editingNode == null) return;

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = _editingNode,
            Data = _editingNode.Data,
            Action = _editMode
        };

        if (_editMode == TreeGridRowAction.Add)
        {
            if (_parentNode != null)
            {
                _parentNode.Children.Add(_editingNode);
                _parentNode.IsExpanded = true;
            }
            else
            {
                DataSource.Add(_editingNode);
            }
            await OnRowAdded.InvokeAsync(args);
        }
        else
        {
            await OnRowEdited.InvokeAsync(args);
        }

        _showEditDialog = false;
        _editingNode = null;
        _parentNode = null;

        InitializeData();
        StateHasChanged();
    }

    private void CancelEdit()
    {
        _showEditDialog = false;
        _editingNode = null;
        _parentNode = null;
        StateHasChanged();
    }

    private async Task DeleteNode(TreeGridNode<TItem> node)
    {
        _deletingNode = node;
        _showDeleteDialog = true;

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = node,
            Data = node.Data,
            Action = TreeGridRowAction.Delete
        };

        await OnRowDeleting.InvokeAsync(args);
        StateHasChanged();
    }

    private async Task ConfirmDelete()
    {
        if (_deletingNode == null) return;

        RemoveNode(DataSource, _deletingNode);

        var args = new TreeGridRowActionEventArgs<TItem>
        {
            Node = _deletingNode,
            Data = _deletingNode.Data,
            Action = TreeGridRowAction.Delete
        };

        await OnRowDeleted.InvokeAsync(args);

        _showDeleteDialog = false;
        _deletingNode = null;

        InitializeData();
        StateHasChanged();
    }

    private void CancelDelete()
    {
        _showDeleteDialog = false;
        _deletingNode = null;
        StateHasChanged();
    }

    private bool RemoveNode(List<TreeGridNode<TItem>> nodes, TreeGridNode<TItem> nodeToRemove)
    {
        if (nodes.Remove(nodeToRemove))
        {
            return true;
        }

        foreach (var node in nodes)
        {
            if (node.HasChildren && RemoveNode(node.Children, nodeToRemove))
            {
                return true;
            }
        }

        return false;
    }

    private object? GetCellValue(TreeGridNode<TItem> node, TreeGridColumn column)
    {
        if (column.ValueAccessor != null)
        {
            return column.ValueAccessor(node.Data!);
        }

        if (node.Data == null) return null;

        var property = typeof(TItem).GetProperty(column.Field);
        if (property == null) return null;

        var value = property.GetValue(node.Data);

        if (column.DisplayFormatter != null && value != null)
        {
            return column.DisplayFormatter(value);
        }

        if (!string.IsNullOrEmpty(column.Format) && value != null)
        {
            if (value is DateTime dateValue)
            {
                return dateValue.ToString(column.Format);
            }
            else if (value is decimal || value is double || value is float)
            {
                return string.Format(column.Format, value);
            }
        }

        return value;
    }

    private async Task RefreshData()
    {
        IsLoading = true;
        StateHasChanged();

        await Task.Delay(500); // Simulate refresh

        InitializeData();

        IsLoading = false;
        await OnDataBound.InvokeAsync();
        StateHasChanged();
    }

    private async Task ExportData()
    {
        var settings = new TreeGridExportSettings
        {
            Format = TreeGridExportFormat.Excel,
            FileName = $"TreeGrid_Export_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        await OnExport.InvokeAsync(settings);
    }

    private void UpdatePaging()
    {
        if (!AllowPaging || _pageSettings == null) return;

        _pageSettings.TotalRecords = CountAllNodes(_filteredNodes);
        _pageSettings.TotalPages = (int)Math.Ceiling((double)_pageSettings.TotalRecords / _pageSettings.PageSize);
    }

    private void FirstPage()
    {
        if (_pageSettings != null)
        {
            _pageSettings.CurrentPage = 1;
            StateHasChanged();
        }
    }

    private void PreviousPage()
    {
        if (_pageSettings != null && _pageSettings.CurrentPage > 1)
        {
            _pageSettings.CurrentPage--;
            StateHasChanged();
        }
    }

    private void NextPage()
    {
        if (_pageSettings != null && _pageSettings.CurrentPage < _pageSettings.TotalPages)
        {
            _pageSettings.CurrentPage++;
            StateHasChanged();
        }
    }

    private void LastPage()
    {
        if (_pageSettings != null)
        {
            _pageSettings.CurrentPage = _pageSettings.TotalPages;
            StateHasChanged();
        }
    }

    private void OnPageSizeChanged()
    {
        UpdatePaging();
        if (_pageSettings != null)
        {
            _pageSettings.CurrentPage = 1;
        }
        StateHasChanged();
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            { "role", "grid" },
            { "aria-label", "Tree Grid" },
            { "aria-readonly", (!AllowEditing).ToString().ToLower() }
        };

        if (IsRTL)
        {
            attributes.Add("dir", "rtl");
        }

        return attributes;
    }
}
