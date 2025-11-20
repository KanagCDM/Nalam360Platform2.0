namespace Nalam360Enterprise.UI.Components.Input;

public enum CascaderExpandTrigger
{
    Click,
    Hover
}

public class CascaderChangeEventArgs<TValue, TItem>
{
    public TValue Value { get; }
    public List<TItem> SelectedPath { get; }

    public CascaderChangeEventArgs(TValue value, List<TItem> selectedPath)
    {
        Value = value;
        SelectedPath = selectedPath;
    }
}
