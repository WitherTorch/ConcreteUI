namespace ConcreteUI.Controls
{
    public interface IMouseEvents
    {
        void OnMouseUp(in MouseNotifyEventArgs args);
        void OnMouseMove(in MouseNotifyEventArgs args);
    }
}
