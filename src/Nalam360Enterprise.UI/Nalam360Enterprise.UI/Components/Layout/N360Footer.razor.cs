using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.Layout;

public class FooterColumn
{
    public string Title { get; set; } = string.Empty;
    public List<FooterLink>? Links { get; set; }
    public RenderFragment? CustomContent { get; set; }
}

public class FooterLink
{
    public string Label { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Icon { get; set; }
    public bool OpenInNewTab { get; set; }
    public bool PreventDefault { get; set; }
    public bool IsNew { get; set; }
    public string? RequiredPermission { get; set; }
}

public class SocialLink
{
    public string Platform { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string Icon { get; set; } = string.Empty;
}
