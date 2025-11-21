namespace Nalam360Enterprise.UI.Components.Utility;

/// <summary>
/// Defines the severity level of an error
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Warning level - non-critical issue
    /// </summary>
    Warning = 0,
    
    /// <summary>
    /// Error level - standard error that can be recovered
    /// </summary>
    Error = 1,
    
    /// <summary>
    /// Critical level - severe error requiring immediate attention
    /// </summary>
    Critical = 2
}
