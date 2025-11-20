namespace Nalam360Enterprise.UI.Components.Inputs;

/// <summary>
/// Represents a radio button item
/// </summary>
public class RadioButtonItem<TValue>
{
    public TValue Value { get; set; } = default!;
    public string Text { get; set; } = string.Empty;
    public bool Disabled { get; set; }
}
