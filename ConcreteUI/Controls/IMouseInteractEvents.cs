namespace ConcreteUI.Controls
{
    public interface IMouseInteractEvents : IMouseEvents
    {
        void OnMouseDown(ref MouseInteractEventArgs args);
    }
}
