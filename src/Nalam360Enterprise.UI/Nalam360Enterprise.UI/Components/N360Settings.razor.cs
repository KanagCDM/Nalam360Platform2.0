using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Nalam360Enterprise.UI.Models;
using Nalam360Enterprise.UI.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Nalam360Enterprise.UI.Components
{
    public partial class N360Settings<TConfiguration> : ComponentBase where TConfiguration : SettingsConfiguration
    {
        [Parameter] public TConfiguration? Configuration { get; set; }
        [Parameter] public SettingsDisplayOptions? DisplayOptions { get; set; }
        [Parameter] public SettingsValidationRules? ValidationRules { get; set; }
        [Parameter] public bool ShowToolbar { get; set; } = true;
        [Parameter] public bool ShowTabs { get; set; } = true;
        [Parameter] public bool ShowSaveIndicator { get; set; } = true;
        [Parameter] public bool IsResponsive { get; set; } = true;
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public string EmptyText { get; set; } = "No settings available";
        
        [Parameter] public EventCallback<Dictionary<string, object?>> OnSettingsChanged { get; set; }
        [Parameter] public EventCallback<Dictionary<string, object?>> OnSave { get; set; }
        [Parameter] public EventCallback OnReset { get; set; }
        [Parameter] public EventCallback<SettingsExportData> OnExport { get; set; }
        [Parameter] public EventCallback<SettingsExportData> OnImport { get; set; }
        
        [Parameter] public string? RequiredPermission { get; set; }
        [Parameter] public bool HideIfNoPermission { get; set; }
        [Parameter] public string? CssClass { get; set; }
        [Parameter] public bool IsRTL { get; set; }

        private string? _searchQuery;
        private string? _activeSection;
        private Dictionary<string, object?> _currentValues = new();
        private Dictionary<string, object?> _originalValues = new();
        private HashSet<string> _modifiedSettings = new();
        private Dictionary<string, string> _validationErrors = new();
        private List<SettingChange> _changeHistory = new();
        private List<SettingsSection> _filteredSections = new();
        
        private bool _showResetDialog;
        private bool _showExportDialog;
        private bool _showImportDialog;
        private bool _exportAllSections = true;
        private HashSet<string> _selectedExportSections = new();
        private bool _mergeOnImport = true;
        private ElementReference _fileInput;

        private System.Threading.Timer? _autoSaveTimer;

        public bool HasUnsavedChanges => _modifiedSettings.Any();

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(RequiredPermission) && HideIfNoPermission)
            {
                if (PermissionService != null && !await PermissionService.HasPermissionAsync(RequiredPermission))
                {
                    return;
                }
            }

            DisplayOptions ??= new SettingsDisplayOptions();
            InitializeSettings();

            if (DisplayOptions.EnableAutoSave)
            {
                StartAutoSave();
            }
        }

        protected override void OnParametersSet()
        {
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            if (Configuration?.Sections == null) return;

            _filteredSections = Configuration.Sections
                .OrderBy(s => s.Order)
                .ThenBy(s => s.Name)
                .ToList();

            if (string.IsNullOrEmpty(_activeSection) && _filteredSections.Any())
            {
                _activeSection = _filteredSections.First().Id;
            }

            // Initialize current values from configuration
            foreach (var section in Configuration.Sections)
            {
                foreach (var item in section.Items)
                {
                    if (!_currentValues.ContainsKey(item.Key))
                    {
                        _currentValues[item.Key] = item.Value;
                        _originalValues[item.Key] = item.Value;
                    }
                }
            }

            ApplySearchFilter();
        }

        private void SelectSection(string sectionId)
        {
            _activeSection = sectionId;
            StateHasChanged();
        }

        private void ToggleSection(string sectionId)
        {
            var section = Configuration?.Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section != null)
            {
                section.IsExpanded = !section.IsExpanded;
                StateHasChanged();
            }
        }

        private async Task HandleSearch()
        {
            ApplySearchFilter();
            await Task.CompletedTask;
        }

        private void ApplySearchFilter()
        {
            if (Configuration?.Sections == null) return;

            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                _filteredSections = Configuration.Sections
                    .OrderBy(s => s.Order)
                    .ThenBy(s => s.Name)
                    .ToList();
                return;
            }

            var query = _searchQuery.ToLower();
            _filteredSections = Configuration.Sections
                .Select(section => new SettingsSection
                {
                    Id = section.Id,
                    Name = section.Name,
                    Description = section.Description,
                    Icon = section.Icon,
                    IsCollapsible = section.IsCollapsible,
                    IsExpanded = true, // Expand sections when searching
                    Category = section.Category,
                    Order = section.Order,
                    Items = section.Items
                        .Where(item =>
                            item.Label.ToLower().Contains(query) ||
                            (item.Description?.ToLower().Contains(query) ?? false) ||
                            item.Key.ToLower().Contains(query))
                        .ToList()
                })
                .Where(section => section.Items.Any())
                .OrderBy(s => s.Order)
                .ThenBy(s => s.Name)
                .ToList();
        }

        private void ClearSearch()
        {
            _searchQuery = null;
            ApplySearchFilter();
            StateHasChanged();
        }

        private async Task UpdateSettingValue(string key, object? value)
        {
            var oldValue = _currentValues.ContainsKey(key) ? _currentValues[key] : null;
            _currentValues[key] = value;

            // Track modification
            if (value?.ToString() != _originalValues[key]?.ToString())
            {
                _modifiedSettings.Add(key);
            }
            else
            {
                _modifiedSettings.Remove(key);
            }

            // Validate
            if (ValidationRules?.ValidateOnChange == true)
            {
                await ValidateSetting(key, value);
            }

            // Track change
            _changeHistory.Insert(0, new SettingChange
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value,
                Timestamp = DateTime.Now
            });

            // Keep only last 50 changes
            if (_changeHistory.Count > 50)
            {
                _changeHistory = _changeHistory.Take(50).ToList();
            }

            await OnSettingsChanged.InvokeAsync(_currentValues);
            StateHasChanged();
        }

        private async Task<bool> ValidateSetting(string key, object? value)
        {
            _validationErrors.Remove(key);

            var item = Configuration?.Sections
                .SelectMany(s => s.Items)
                .FirstOrDefault(i => i.Key == key);

            if (item == null) return true;

            // Required validation
            if (item.IsRequired && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                _validationErrors[key] = "This field is required";
                return false;
            }

            // Regex validation
            if (!string.IsNullOrEmpty(item.ValidationRegex) && value != null)
            {
                if (!Regex.IsMatch(value.ToString()!, item.ValidationRegex))
                {
                    _validationErrors[key] = item.ValidationMessage ?? "Invalid format";
                    return false;
                }
            }

            // Min/Max validation for numbers
            if (item.Type == SettingType.Number && value != null)
            {
                if (int.TryParse(value.ToString(), out int numValue))
                {
                    if (item.MinValue.HasValue && numValue < item.MinValue.Value)
                    {
                        _validationErrors[key] = $"Value must be at least {item.MinValue.Value}";
                        return false;
                    }
                    if (item.MaxValue.HasValue && numValue > item.MaxValue.Value)
                    {
                        _validationErrors[key] = $"Value must not exceed {item.MaxValue.Value}";
                        return false;
                    }
                }
            }

            // Custom validation rules
            if (ValidationRules?.Rules.ContainsKey(key) == true)
            {
                var result = ValidationRules.Rules[key](value);
                if (!result.IsValid)
                {
                    _validationErrors[key] = result.ErrorMessage ?? "Validation failed";
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateAllSettings()
        {
            _validationErrors.Clear();
            bool isValid = true;

            foreach (var kvp in _currentValues)
            {
                if (!await ValidateSetting(kvp.Key, kvp.Value))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private async Task SaveSettings()
        {
            if (ValidationRules?.PreventInvalidSave == true)
            {
                if (!await ValidateAllSettings())
                {
                    StateHasChanged();
                    return;
                }
            }

            IsLoading = true;
            StateHasChanged();

            try
            {
                // Update original values
                foreach (var key in _modifiedSettings.ToList())
                {
                    _originalValues[key] = _currentValues[key];
                }

                await OnSave.InvokeAsync(_currentValues);
                _modifiedSettings.Clear();
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        private void ShowResetDialog()
        {
            if (DisplayOptions?.ConfirmBeforeReset == true)
            {
                _showResetDialog = true;
                StateHasChanged();
            }
            else
            {
                _ = ConfirmReset();
            }
        }

        private void CloseResetDialog()
        {
            _showResetDialog = false;
            StateHasChanged();
        }

        private async Task ConfirmReset()
        {
            _showResetDialog = false;
            IsLoading = true;
            StateHasChanged();

            try
            {
                // Reset all values to defaults
                if (Configuration?.Sections != null)
                {
                    foreach (var section in Configuration.Sections)
                    {
                        foreach (var item in section.Items)
                        {
                            _currentValues[item.Key] = item.DefaultValue;
                            _originalValues[item.Key] = item.DefaultValue;
                        }
                    }
                }

                _modifiedSettings.Clear();
                _validationErrors.Clear();
                _changeHistory.Clear();

                await OnReset.InvokeAsync();
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        private void ResetSingleSetting(string key)
        {
            var item = Configuration?.Sections
                .SelectMany(s => s.Items)
                .FirstOrDefault(i => i.Key == key);

            if (item != null)
            {
                _currentValues[key] = item.DefaultValue;
                _originalValues[key] = item.DefaultValue;
                _modifiedSettings.Remove(key);
                _validationErrors.Remove(key);
                StateHasChanged();
            }
        }

        private void ShowExportDialog()
        {
            _showExportDialog = true;
            _exportAllSections = true;
            _selectedExportSections.Clear();
            StateHasChanged();
        }

        private void CloseExportDialog()
        {
            _showExportDialog = false;
            StateHasChanged();
        }

        private void ToggleExportSection(string sectionId, bool isSelected)
        {
            if (isSelected)
            {
                _selectedExportSections.Add(sectionId);
            }
            else
            {
                _selectedExportSections.Remove(sectionId);
            }
        }

        private async Task ConfirmExport()
        {
            var exportData = new SettingsExportData
            {
                ExportedAt = DateTime.Now,
                Version = "1.0"
            };

            if (_exportAllSections)
            {
                exportData.Settings = _currentValues;
            }
            else
            {
                var sectionsToExport = Configuration?.Sections
                    .Where(s => _selectedExportSections.Contains(s.Id))
                    .SelectMany(s => s.Items.Select(i => i.Key))
                    .ToList() ?? new List<string>();

                exportData.Settings = _currentValues
                    .Where(kvp => sectionsToExport.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                exportData.IncludedSections = _selectedExportSections.ToList();
            }

            await OnExport.InvokeAsync(exportData);
            CloseExportDialog();
        }

        private void ShowImportDialog()
        {
            _showImportDialog = true;
            _mergeOnImport = true;
            StateHasChanged();
        }

        private void CloseImportDialog()
        {
            _showImportDialog = false;
            StateHasChanged();
        }

        private async Task ConfirmImport()
        {
            // In a real implementation, this would read the file and parse JSON
            // For now, just trigger the callback
            var importData = new SettingsExportData();
            await OnImport.InvokeAsync(importData);
            CloseImportDialog();
        }

        private bool IsItemVisible(SettingItem item)
        {
            if (string.IsNullOrEmpty(item.DependsOn))
                return true;

            if (!_currentValues.ContainsKey(item.DependsOn))
                return false;

            var dependencyValue = _currentValues[item.DependsOn];
            
            if (item.DependsOnValue == null)
                return dependencyValue != null;

            return dependencyValue?.ToString() == item.DependsOnValue?.ToString();
        }

        private bool IsSettingModified(string key)
        {
            return DisplayOptions?.HighlightModified == true && _modifiedSettings.Contains(key);
        }

        private bool HasModifiedSettingsInSection(string sectionId)
        {
            var section = Configuration?.Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return false;

            return _modifiedSettings.Any(k => section.Items.Any(i => i.Key == k));
        }

        private void StartAutoSave()
        {
            if (DisplayOptions == null) return;

            _autoSaveTimer = new System.Threading.Timer(async _ =>
            {
                if (HasUnsavedChanges)
                {
                    await InvokeAsync(async () =>
                    {
                        await SaveSettings();
                    });
                }
            }, null, TimeSpan.FromMilliseconds(DisplayOptions.AutoSaveDelay), TimeSpan.FromMilliseconds(DisplayOptions.AutoSaveDelay));
        }

        private Dictionary<string, object> GetHtmlAttributes()
        {
            var attrs = new Dictionary<string, object>
            {
                { "role", "region" },
                { "aria-label", "Settings Panel" }
            };

            if (IsRTL)
            {
                attrs["dir"] = "rtl";
            }

            return attrs;
        }

        public void Dispose()
        {
            _autoSaveTimer?.Dispose();
        }
    }
}
