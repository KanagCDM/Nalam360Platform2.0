using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.DataDisplay;

public enum TimelineMode
{
    Left,
    Alternate,
    Right
}

public class TimelineItem
{
    public string? Label { get; set; }
    public string? Description { get; set; }
    public RenderFragment? Content { get; set; }
    public RenderFragment? Icon { get; set; }
    public RenderFragment? Dot { get; set; }
    public string? Color { get; set; } // blue, red, green, gray
    public DateTime? Timestamp { get; set; }
}
