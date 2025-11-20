namespace Nalam360Enterprise.UI.Components.Navigation;

/// <summary>
/// Represents a menu item
/// </summary>
public class MenuItem
{
    public string? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? IconCss { get; set; }
    public string? Url { get; set; }
    public bool Separator { get; set; }
    public List<MenuItem>? Children { get; set; }
    public string? RequiredPermission { get; set; }
    public string? ParentId { get; set; }
}
