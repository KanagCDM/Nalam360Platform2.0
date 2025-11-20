namespace Nalam360Enterprise.UI.Components.Inputs;

/// <summary>
/// Represents a date range preset
/// </summary>
public class DateRangePreset
{
    public string Label { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

/// <summary>
/// Event args for date range selection
/// </summary>
public class DateRangeEventArgs
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
