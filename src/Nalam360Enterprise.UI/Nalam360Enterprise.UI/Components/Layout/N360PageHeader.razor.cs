namespace Nalam360Enterprise.UI.Components.Layout;

public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Icon { get; set; }
    public bool PreventDefault { get; set; }
    public string? RequiredPermission { get; set; }
}

public class MetadataItem
{
    public string? Icon { get; set; }
    public string? Label { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ActionButton
{
    public string? Label { get; set; }
    public string? Icon { get; set; }
    public string Type { get; set; } = "default"; // default, success, warning, danger
    public bool IsPrimary { get; set; }
    public bool IsDisabled { get; set; }
    public string? Tooltip { get; set; }
    public string? RequiredPermission { get; set; }
}

public class TabItem
{
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public bool IsDisabled { get; set; }
    public int Count { get; set; }
    public string? RequiredPermission { get; set; }
}
