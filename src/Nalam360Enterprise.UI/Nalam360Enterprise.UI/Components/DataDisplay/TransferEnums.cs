namespace Nalam360Enterprise.UI.Components.DataDisplay;

public enum TransferDirection
{
    ToSource,
    ToTarget
}

public class TransferChangeEventArgs<TItem>
{
    public List<TItem> MovedItems { get; }
    public TransferDirection Direction { get; }
    public List<object> TargetKeys { get; }

    public TransferChangeEventArgs(List<TItem> movedItems, TransferDirection direction, List<object> targetKeys)
    {
        MovedItems = movedItems;
        Direction = direction;
        TargetKeys = targetKeys;
    }
}
