namespace ConcreteUI.Controls
{
    public interface IMouseNotifyEvents : IMouseEvents
    {
        void OnMouseDown(in MouseNotifyEventArgs args);
    }
}
