using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models
{
    /// <summary>
    /// Represents a collection of settings organized into sections
    /// </summary>
    public class SettingsConfiguration
    {
        public List<SettingsSection> Sections { get; set; } = new();
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ModifiedBy { get; set; }
    }

    /// <summary>
    /// Represents a section of related settings
    /// </summary>
    public class SettingsSection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<SettingItem> Items { get; set; } = new();
        public bool IsCollapsible { get; set; } = true;
        public bool IsExpanded { get; set; } = true;
        public string? Category { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// Represents an individual setting item
    /// </summary>
    public class SettingItem
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SettingType Type { get; set; } = SettingType.Text;
        public object? Value { get; set; }
        public object? DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? Placeholder { get; set; }
        public string? ValidationRegex { get; set; }
        public string? ValidationMessage { get; set; }
        public List<SettingOption>? Options { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public string? Group { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string? Unit { get; set; }
        public bool RequiresRestart { get; set; }
        public string? DependsOn { get; set; }
        public object? DependsOnValue { get; set; }
        public string? Permission { get; set; }
        public string? HelpUrl { get; set; }
        public SettingLevel Level { get; set; } = SettingLevel.User;
    }

    /// <summary>
    /// Types of setting controls
    /// </summary>
    public enum SettingType
    {
        Text,
        Number,
        Boolean,
        Dropdown,
        MultiSelect,
        Color,
        Date,
        Time,
        DateTime,
        Email,
        Password,
        Url,
        TextArea,
        Slider,
        Radio,
        File,
        Json,
        Code
    }

    /// <summary>
    /// Level/scope of a setting
    /// </summary>
    public enum SettingLevel
    {
        System,      // System-wide settings (admin only)
        Application, // Application-level settings
        User,        // User-specific settings
        Session      // Session-specific settings
    }

    /// <summary>
    /// Option for dropdown/radio/multi-select settings
    /// </summary>
    public class SettingOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsDisabled { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Validation result for a setting
    /// </summary>
    public class SettingValidationResult
    {
        public bool IsValid { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Change tracking for a setting
    /// </summary>
    public class SettingChange
    {
        public string Key { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Settings import/export data
    /// </summary>
    public class SettingsExportData
    {
        public DateTime ExportedAt { get; set; } = DateTime.Now;
        public string? ExportedBy { get; set; }
        public string? Version { get; set; }
        public Dictionary<string, object?> Settings { get; set; } = new();
        public List<string>? IncludedSections { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Settings search filter
    /// </summary>
    public class SettingsSearchFilter
    {
        public string? Query { get; set; }
        public List<string>? Sections { get; set; }
        public List<SettingType>? Types { get; set; }
        public List<SettingLevel>? Levels { get; set; }
        public bool? IsModified { get; set; }
        public bool? RequiresRestart { get; set; }
    }

    /// <summary>
    /// Settings display options
    /// </summary>
    public class SettingsDisplayOptions
    {
        public bool ShowSearch { get; set; } = true;
        public bool ShowReset { get; set; } = true;
        public bool ShowExport { get; set; } = true;
        public bool ShowImport { get; set; } = true;
        public bool ShowHistory { get; set; }
        public bool ShowDescriptions { get; set; } = true;
        public bool ShowDefaults { get; set; }
        public bool GroupByCategory { get; set; }
        public bool EnableAutoSave { get; set; }
        public int AutoSaveDelay { get; set; } = 1000;
        public bool ConfirmBeforeReset { get; set; } = true;
        public bool HighlightModified { get; set; } = true;
        public bool ShowPermissionIndicators { get; set; }
    }

    /// <summary>
    /// Settings validation rules
    /// </summary>
    public class SettingsValidationRules
    {
        public Dictionary<string, Func<object?, SettingValidationResult>> Rules { get; set; } = new();
        public bool ValidateOnChange { get; set; } = true;
        public bool PreventInvalidSave { get; set; } = true;
    }
}
