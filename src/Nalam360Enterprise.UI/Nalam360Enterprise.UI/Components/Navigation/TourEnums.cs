using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.Navigation;

public enum TourPlacement
{
    Top,
    Left,
    Right,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
    Center
}

public class TourStep
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public RenderFragment? Content { get; set; }
    public string? Target { get; set; } // CSS selector for target element
    public TourPlacement Placement { get; set; } = TourPlacement.Bottom;
    public bool ShowArrow { get; set; } = true;
    public string? NextButtonText { get; set; }
    public string? PreviousButtonText { get; set; }
}
