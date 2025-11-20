using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.Layout;

public enum CollapseExpandIconPosition
{
    Left,
    Right
}

public class CollapseItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public RenderFragment? Content { get; set; }
    public RenderFragment? Extra { get; set; }
    public bool Disabled { get; set; }
}
