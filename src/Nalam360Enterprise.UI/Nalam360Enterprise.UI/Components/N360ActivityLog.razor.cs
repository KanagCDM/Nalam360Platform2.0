using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components;

public partial class N360ActivityLog<TEntry> where TEntry : ActivityLogEntry
{
    // Data
    [Parameter] public List<TEntry> Entries { get; set; } = new();
    [Parameter] public ActivityLogSettings Settings { get; set; } = new();
    
    // Display
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowStatistics { get; set; } = true;
    [Parameter] public bool ShowFilters { get; set; } = false;
    [Parameter] public ActivityLogViewMode CurrentViewMode { get; set; } = ActivityLogViewMode.List;
    [Parameter] public string EmptyText { get; set; } = "No activity logs found";
    [Parameter] public bool IsResponsive { get; set; } = true;
    
    // Features
    [Parameter] public bool AllowExport { get; set; } = true;
    [Parameter] public bool AllowDetails { get; set; } = true;
    [Parameter] public bool EnableRealTime { get; set; } = false;
    [Parameter] public int AutoRefreshSeconds { get; set; } = 30;
    
    // Events
    [Parameter] public EventCallback<ActivityLogFilter> OnFilterChanged { get; set; }
    [Parameter] public EventCallback OnRefresh { get; set; }
    [Parameter] public EventCallback<TEntry> OnEntrySelected { get; set; }
    [Parameter] public EventCallback<ActivityLogExportOptions> OnExport { get; set; }
    
    // RBAC
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; } = false;
    
    // Styling
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool IsRTL { get; set; } = false;
    [Parameter] public bool IsLoading { get; set; } = false;
    
    // Private state
    private ActivityLogFilter _filter = new();
    private ActivityLogSettings _settings = new();
    private ActivityLogStatistics? _statistics;
    private ActivityLogExportOptions _exportOptions = new();
    private List<TEntry> _filteredEntries = new();
    private TEntry? _selectedEntry;
    private bool _showFilters = false;
    private bool _showDetails = false;
    private bool _showExportDialog = false;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _activeFilterCount = 0;
    private System.Threading.Timer? _autoRefreshTimer;
    
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(RequiredPermission) && HideIfNoPermission)
        {
            if (PermissionService != null && !await PermissionService.HasPermissionAsync(RequiredPermission))
            {
                return;
            }
        }
        
        _settings = Settings ?? new ActivityLogSettings();
        _showFilters = ShowFilters;
        
        InitializeData();
        
        if (EnableRealTime && AutoRefreshSeconds > 0)
        {
            StartAutoRefresh();
        }
        
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        InitializeData();
        await base.OnParametersSetAsync();
    }
    
    private void InitializeData()
    {
        ApplyFiltersInternal();
        CalculateStatistics();
        UpdatePagination();
    }
    
    private void ApplyFiltersInternal()
    {
        IEnumerable<TEntry> filtered = Entries;
        
        // Type filter
        if (_filter.Types.Any())
        {
            filtered = filtered.Where(e => _filter.Types.Contains(e.Type));
        }
        
        // Severity filter
        if (_filter.Severities.Any())
        {
            filtered = filtered.Where(e => _filter.Severities.Contains(e.Severity));
        }
        
        // Date range filter
        if (_filter.StartDate.HasValue)
        {
            filtered = filtered.Where(e => e.Timestamp >= _filter.StartDate.Value);
        }
        
        if (_filter.EndDate.HasValue)
        {
            filtered = filtered.Where(e => e.Timestamp <= _filter.EndDate.Value);
        }
        
        // Search query
        if (!string.IsNullOrWhiteSpace(_filter.SearchQuery))
        {
            var query = _filter.SearchQuery.ToLower();
            filtered = filtered.Where(e =>
                e.Action.ToLower().Contains(query) ||
                e.Description.ToLower().Contains(query) ||
                e.UserName.ToLower().Contains(query) ||
                e.Module.ToLower().Contains(query) ||
                e.Resource.ToLower().Contains(query)
            );
        }
        
        // Success/Error filters
        if (_filter.ShowSuccessOnly)
        {
            filtered = filtered.Where(e => e.IsSuccess);
        }
        
        if (_filter.ShowErrorsOnly)
        {
            filtered = filtered.Where(e => !e.IsSuccess);
        }
        
        // Duration filter
        if (_filter.MinDuration.HasValue)
        {
            filtered = filtered.Where(e => e.Duration >= _filter.MinDuration.Value);
        }
        
        if (_filter.MaxDuration.HasValue)
        {
            filtered = filtered.Where(e => e.Duration <= _filter.MaxDuration.Value);
        }
        
        // Tags filter
        if (_filter.Tags.Any())
        {
            filtered = filtered.Where(e => e.Tags.Intersect(_filter.Tags).Any());
        }
        
        _filteredEntries = filtered.OrderByDescending(e => e.Timestamp).ToList();
        
        // Calculate active filter count
        _activeFilterCount = 0;
        if (_filter.Types.Any()) _activeFilterCount++;
        if (_filter.Severities.Any()) _activeFilterCount++;
        if (_filter.StartDate.HasValue || _filter.EndDate.HasValue) _activeFilterCount++;
        if (!string.IsNullOrWhiteSpace(_filter.SearchQuery)) _activeFilterCount++;
        if (_filter.ShowSuccessOnly || _filter.ShowErrorsOnly) _activeFilterCount++;
        if (_filter.MinDuration.HasValue || _filter.MaxDuration.HasValue) _activeFilterCount++;
        if (_filter.Tags.Any()) _activeFilterCount++;
    }
    
    private void CalculateStatistics()
    {
        _statistics = new ActivityLogStatistics
        {
            TotalEntries = _filteredEntries.Count,
            InformationCount = _filteredEntries.Count(e => e.Type == ActivityLogType.Information),
            WarningCount = _filteredEntries.Count(e => e.Type == ActivityLogType.Warning),
            ErrorCount = _filteredEntries.Count(e => e.Type == ActivityLogType.Error),
            CriticalCount = _filteredEntries.Count(e => e.Type == ActivityLogType.Critical),
            SecurityCount = _filteredEntries.Count(e => e.Type == ActivityLogType.Security),
            UniqueUsers = _filteredEntries.Select(e => e.UserId).Distinct().Count(),
            UniqueModules = _filteredEntries.Select(e => e.Module).Distinct().Count(),
            UniqueResources = _filteredEntries.Select(e => e.Resource).Distinct().Count(),
            AverageDuration = _filteredEntries.Any(e => e.Duration.HasValue) 
                ? TimeSpan.FromMilliseconds(_filteredEntries.Where(e => e.Duration.HasValue)
                    .Average(e => e.Duration!.Value.TotalMilliseconds))
                : TimeSpan.Zero,
            OldestEntry = _filteredEntries.Any() ? _filteredEntries.Min(e => e.Timestamp) : null,
            NewestEntry = _filteredEntries.Any() ? _filteredEntries.Max(e => e.Timestamp) : null
        };
        
        // Group by type
        _statistics.EntriesByType = _filteredEntries
            .GroupBy(e => e.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Group by module
        _statistics.EntriesByModule = _filteredEntries
            .Where(e => !string.IsNullOrEmpty(e.Module))
            .GroupBy(e => e.Module)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Group by user
        _statistics.EntriesByUser = _filteredEntries
            .Where(e => !string.IsNullOrEmpty(e.UserName))
            .GroupBy(e => e.UserName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
    }
    
    private void UpdatePagination()
    {
        _totalPages = (int)Math.Ceiling(_filteredEntries.Count / (double)_settings.PageSize);
        if (_currentPage > _totalPages && _totalPages > 0)
        {
            _currentPage = _totalPages;
        }
        else if (_currentPage < 1)
        {
            _currentPage = 1;
        }
    }
    
    private async Task ChangeViewMode(ActivityLogViewMode mode)
    {
        CurrentViewMode = mode;
        await Task.CompletedTask;
        StateHasChanged();
    }
    
    private void ToggleFilters()
    {
        _showFilters = !_showFilters;
        StateHasChanged();
    }
    
    private void ToggleTypeFilter(ActivityLogType type, bool isChecked)
    {
        if (isChecked)
        {
            if (!_filter.Types.Contains(type))
            {
                _filter.Types.Add(type);
            }
        }
        else
        {
            _filter.Types.Remove(type);
        }
    }
    
    private void ToggleSeverityFilter(ActivityLogSeverity severity, bool isChecked)
    {
        if (isChecked)
        {
            if (!_filter.Severities.Contains(severity))
            {
                _filter.Severities.Add(severity);
            }
        }
        else
        {
            _filter.Severities.Remove(severity);
        }
    }
    
    private async Task ApplyFilters()
    {
        _currentPage = 1;
        ApplyFiltersInternal();
        CalculateStatistics();
        UpdatePagination();
        
        await OnFilterChanged.InvokeAsync(_filter);
        StateHasChanged();
    }
    
    private async Task ClearFilters()
    {
        _filter = new ActivityLogFilter();
        await ApplyFilters();
    }
    
    private async Task RefreshLogs()
    {
        IsLoading = true;
        StateHasChanged();
        
        await OnRefresh.InvokeAsync();
        
        await Task.Delay(500); // Simulate loading
        
        InitializeData();
        IsLoading = false;
        StateHasChanged();
    }
    
    private void FirstPage()
    {
        _currentPage = 1;
        StateHasChanged();
    }
    
    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            StateHasChanged();
        }
    }
    
    private void NextPage()
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            StateHasChanged();
        }
    }
    
    private void LastPage()
    {
        _currentPage = _totalPages;
        StateHasChanged();
    }
    
    private async Task ShowEntryDetails(TEntry entry)
    {
        if (!AllowDetails) return;
        
        _selectedEntry = entry;
        _showDetails = true;
        await OnEntrySelected.InvokeAsync(entry);
        StateHasChanged();
    }
    
    private void CloseDetails()
    {
        _showDetails = false;
        _selectedEntry = null;
        StateHasChanged();
    }
    
    private void ShowExportDialog()
    {
        _exportOptions = new ActivityLogExportOptions
        {
            Filter = _filter,
            FileName = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}"
        };
        _showExportDialog = true;
        StateHasChanged();
    }
    
    private void CloseExportDialog()
    {
        _showExportDialog = false;
        StateHasChanged();
    }
    
    private async Task ConfirmExport()
    {
        await OnExport.InvokeAsync(_exportOptions);
        CloseExportDialog();
    }
    
    private string GetTypeIcon(ActivityLogType type)
    {
        return type switch
        {
            ActivityLogType.Information => "ℹ️",
            ActivityLogType.Warning => "⚠️",
            ActivityLogType.Error => "❌",
            ActivityLogType.Critical => "🔥",
            ActivityLogType.Success => "✅",
            ActivityLogType.Security => "🔒",
            ActivityLogType.Performance => "⚡",
            ActivityLogType.Audit => "📋",
            ActivityLogType.Debug => "🐛",
            _ => "📄"
        };
    }
    
    private void StartAutoRefresh()
    {
        _autoRefreshTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await RefreshLogs();
            });
        }, null, TimeSpan.FromSeconds(AutoRefreshSeconds), TimeSpan.FromSeconds(AutoRefreshSeconds));
    }
    
    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            { "role", "region" },
            { "aria-label", "Activity Log" }
        };
        
        if (IsRTL)
        {
            attributes.Add("dir", "rtl");
        }
        
        return attributes;
    }
    
    public void Dispose()
    {
        _autoRefreshTimer?.Dispose();
    }
}
