namespace Nalam360Enterprise.UI.Components.Layout;

public class HeaderNavItem
{
    public string Label { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public int Badge { get; set; }
    public bool PreventDefault { get; set; }
    public bool IsDivider { get; set; }
    public bool IsWarning { get; set; }
    public bool IsDanger { get; set; }
    public string? RequiredPermission { get; set; }
    public List<HeaderNavItem>? Children { get; set; }
}
