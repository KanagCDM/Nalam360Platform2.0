namespace Nalam360Enterprise.UI.Components.Input;

public class TreeSelectChangeEventArgs<TValue, TItem>
{
    public TValue? Value { get; }
    public List<TValue>? Values { get; }
    public List<TItem> SelectedItems { get; }

    public TreeSelectChangeEventArgs(TValue? value, List<TValue>? values, List<TItem> selectedItems)
    {
        Value = value;
        Values = values;
        SelectedItems = selectedItems;
    }
}
