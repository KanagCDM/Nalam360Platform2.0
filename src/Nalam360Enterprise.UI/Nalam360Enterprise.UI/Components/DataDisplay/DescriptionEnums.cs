using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.DataDisplay;

public enum DescriptionLayout
{
    Horizontal,
    Vertical
}

public enum DescriptionSize
{
    Small,
    Default,
    Large
}

public class DescriptionItem
{
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public RenderFragment? ValueContent { get; set; }
    public string? HelpText { get; set; }
    public int Span { get; set; } = 1;
}
