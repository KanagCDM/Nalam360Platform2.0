namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a saved filter preset for the N360FilterBuilder component
    /// </summary>
    public class FilterPreset
    {
        /// <summary>
        /// Unique identifier for the preset
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name of the preset
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of what the filter does
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The filter rule configuration
        /// </summary>
        public FilterRule? Rule { get; set; }

        /// <summary>
        /// Whether this is the default filter to load
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// When the preset was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the preset was last modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// User who created the preset
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Category for organizing presets
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Tags for filtering presets
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Whether the preset is shared with other users
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// Additional metadata as key-value pairs
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a filter rule with multiple conditions
    /// </summary>
    public class FilterRule
    {
        /// <summary>
        /// Logical condition (and/or)
        /// </summary>
        public string Condition { get; set; } = "and";

        /// <summary>
        /// List of individual filter rule items
        /// </summary>
        public List<FilterRuleItem> Rules { get; set; } = new();
    }

    /// <summary>
    /// Represents an individual filter rule item
    /// </summary>
    public class FilterRuleItem
    {
        /// <summary>
        /// Field to filter on
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Operator to apply (equal, contains, greaterthan, etc.)
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// Value to compare against
        /// </summary>
        public string? Value { get; set; }
    }

    /// <summary>
    /// Represents a column definition for the filter builder
    /// </summary>
    public class FilterColumn
    {
        /// <summary>
        /// Field name (property name)
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Display label for the field
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Data type (string, number, date, boolean)
        /// </summary>
        public string Type { get; set; } = "string";
    }

    /// <summary>
    /// Represents a filter operator option
    /// </summary>
    public class FilterOperator
    {
        /// <summary>
        /// Operator value (equal, contains, etc.)
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display label for the operator
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents an AI-generated query suggestion for the filter builder
    /// </summary>
    public class QuerySuggestion
    {
        /// <summary>
        /// Display label for the suggestion
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Description of what the suggestion does
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// SQL preview of the generated query
        /// </summary>
        public string SqlPreview { get; set; } = string.Empty;

        /// <summary>
        /// AI confidence score (0-100)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The filter rules this suggestion would create
        /// </summary>
        public List<FilterRuleItem>? Rules { get; set; }

        /// <summary>
        /// Intent category detected by AI
        /// </summary>
        public string Intent { get; set; } = string.Empty;

        /// <summary>
        /// When the suggestion was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
