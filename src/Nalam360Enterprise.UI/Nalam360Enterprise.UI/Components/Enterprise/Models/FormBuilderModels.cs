using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise.Models
{
    /// <summary>
    /// Represents a form definition
    /// </summary>
    public class FormDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public List<FormSection> Sections { get; set; } = new();
        public FormSettings Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public FormStatus Status { get; set; } = FormStatus.Draft;
        public List<FormVersion> VersionHistory { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a section within a form
    /// </summary>
    public class FormSection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCollapsible { get; set; } = true;
        public bool IsCollapsed { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public List<FormField> Fields { get; set; } = new();
        public string? ConditionalLogic { get; set; }
        public int ColumnsCount { get; set; } = 1;
    }

    /// <summary>
    /// Represents a form field
    /// </summary>
    public class FormField
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public FormFieldType Type { get; set; } = FormFieldType.Text;
        public bool IsRequired { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public object? DefaultValue { get; set; }
        public int Order { get; set; }
        public int ColumnSpan { get; set; } = 1;
        public List<FormFieldOption> Options { get; set; } = new();
        public FormFieldValidation Validation { get; set; } = new();
        public string? ConditionalLogic { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Form field types
    /// </summary>
    public enum FormFieldType
    {
        Text,
        TextArea,
        Number,
        Email,
        Phone,
        Url,
        Date,
        DateTime,
        Time,
        Checkbox,
        Radio,
        Dropdown,
        MultiSelect,
        FileUpload,
        Signature,
        Rating,
        Slider,
        ColorPicker,
        RichText,
        Address,
        Currency,
        Barcode,
        QRCode,
        Location
    }

    /// <summary>
    /// Represents a field option for dropdowns, radio buttons, etc.
    /// </summary>
    public class FormFieldOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public int Order { get; set; }
    }

    /// <summary>
    /// Field validation rules
    /// </summary>
    public class FormFieldValidation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? Pattern { get; set; }
        public string? PatternMessage { get; set; }
        public List<string> AllowedFileTypes { get; set; } = new();
        public long? MaxFileSize { get; set; }
        public string? CustomValidator { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Form settings
    /// </summary>
    public class FormSettings
    {
        public bool ShowProgressBar { get; set; } = true;
        public bool AllowSaveAsDraft { get; set; } = true;
        public bool RequireAuthentication { get; set; } = false;
        public bool EnableAutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 30; // seconds
        public string SubmitButtonText { get; set; } = "Submit";
        public string CancelButtonText { get; set; } = "Cancel";
        public string SaveDraftButtonText { get; set; } = "Save as Draft";
        public string SuccessMessage { get; set; } = "Form submitted successfully";
        public string? SuccessRedirectUrl { get; set; }
        public string Theme { get; set; } = "default";
        public bool ShowFieldNumbers { get; set; } = false;
        public bool ShowRequiredIndicator { get; set; } = true;
    }

    /// <summary>
    /// Form status
    /// </summary>
    public enum FormStatus
    {
        Draft,
        Published,
        Archived,
        Deprecated
    }

    /// <summary>
    /// Form version history
    /// </summary>
    public class FormVersion
    {
        public string Version { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string? ChangeNotes { get; set; }
        public string FormJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Form submission
    /// </summary>
    public class FormSubmission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FormId { get; set; }
        public string FormVersion { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string? SubmittedBy { get; set; }
        public FormSubmissionStatus Status { get; set; } = FormSubmissionStatus.Submitted;
        public List<FormSubmissionFile> Files { get; set; } = new();
    }

    /// <summary>
    /// Submission status
    /// </summary>
    public enum FormSubmissionStatus
    {
        Draft,
        Submitted,
        InReview,
        Approved,
        Rejected
    }

    /// <summary>
    /// File attachment in submission
    /// </summary>
    public class FormSubmissionFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FieldName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Form template for quick creation
    /// </summary>
    public class FormTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public FormDefinition Definition { get; set; } = new();
        public string PreviewImageUrl { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;
        public int UsageCount { get; set; } = 0;
    }

    /// <summary>
    /// Field type metadata for designer
    /// </summary>
    public class FieldTypeMetadata
    {
        public FormFieldType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> SupportedProperties { get; set; } = new();
    }

    /// <summary>
    /// Conditional logic rule
    /// </summary>
    public class ConditionalRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FieldName { get; set; } = string.Empty;
        public ConditionalOperator Operator { get; set; } = ConditionalOperator.Equals;
        public object? Value { get; set; }
        public ConditionalAction Action { get; set; } = ConditionalAction.Show;
        public List<string> TargetFields { get; set; } = new();
    }

    /// <summary>
    /// Conditional operators
    /// </summary>
    public enum ConditionalOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        NotContains,
        IsEmpty,
        IsNotEmpty
    }

    /// <summary>
    /// Conditional actions
    /// </summary>
    public enum ConditionalAction
    {
        Show,
        Hide,
        Enable,
        Disable,
        SetValue,
        SetRequired
    }

    /// <summary>
    /// AI-generated field suggestion for form builder
    /// </summary>
    public class FieldSuggestion
    {
        /// <summary>
        /// Suggested field name (camelCase)
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Suggested field type
        /// </summary>
        public FormFieldType FieldType { get; set; }

        /// <summary>
        /// Description of why this field is suggested
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// AI confidence score (0-100)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Suggested validation rule description
        /// </summary>
        public string? ValidationRule { get; set; }

        /// <summary>
        /// Suggested field options (for dropdowns, radio buttons)
        /// </summary>
        public List<string>? SuggestedOptions { get; set; }

        /// <summary>
        /// Suggested placeholder text
        /// </summary>
        public string? PlaceholderText { get; set; }

        /// <summary>
        /// When the suggestion was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Context category that triggered this suggestion
        /// </summary>
        public string Context { get; set; } = string.Empty;
    }
}
