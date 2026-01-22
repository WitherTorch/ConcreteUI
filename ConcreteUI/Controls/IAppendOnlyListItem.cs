namespace ConcreteUI.Controls
{
    public interface IAppendOnlyListItem<TSelf> : IListItem where TSelf : IAppendOnlyListItem<TSelf>
    {
        int CalculateHeight(TSelf? reference, bool force);
    }
}
