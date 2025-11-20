using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.Input;

public enum ButtonStyle
{
    Default,
    Primary,
    Success,
    Warning,
    Danger,
    Info,
    Link
}

public enum ButtonSize
{
    Small,
    Default,
    Large
}

public enum MenuPlacement
{
    BottomStart,
    BottomEnd,
    TopStart,
    TopEnd
}

public class SplitButtonMenuItem
{
    public string Text { get; set; } = string.Empty;
    public RenderFragment? Icon { get; set; }
    public bool Disabled { get; set; }
    public bool Danger { get; set; }
    public bool IsDivider { get; set; }
    public EventCallback OnClick { get; set; }
    public object? Data { get; set; }
}
