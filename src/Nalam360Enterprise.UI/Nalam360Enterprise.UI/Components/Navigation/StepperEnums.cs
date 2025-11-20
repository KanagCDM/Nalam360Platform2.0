using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Components.Navigation;

public enum StepperOrientation
{
    Horizontal,
    Vertical
}

public enum StepperSize
{
    Small,
    Default,
    Large
}

public enum StepStatus
{
    Pending,
    Active,
    Completed,
    Error
}

public class StepItem
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RenderFragment? SubTitle { get; set; }
    public RenderFragment? Icon { get; set; }
    public RenderFragment? CompletedIcon { get; set; }
    public RenderFragment? ErrorIcon { get; set; }
    public RenderFragment? Content { get; set; }
    public bool Disabled { get; set; }
    public StepStatus? Status { get; set; }
}
