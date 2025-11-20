using Microsoft.AspNetCore.Components;

namespace Nalam360Enterprise.UI.Core;

/// <summary>
/// Base component class for all Nalam360 Enterprise UI components
/// </summary>
public abstract class N360ComponentBase : ComponentBase
{
    /// <summary>
    /// Additional CSS class names
    /// </summary>
    [Parameter] public string? CssClass { get; set; }
    
    /// <summary>
    /// Additional inline styles
    /// </summary>
    [Parameter] public string? Style { get; set; }
    
    /// <summary>
    /// Unmatched HTML attributes
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    /// <summary>
    /// Gets combined HTML attributes including accessibility, RTL, and custom attributes
    /// </summary>
    protected Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>();
        
        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                attributes[attr.Key] = attr.Value;
            }
        }
        
        return attributes;
    }
}
