namespace ConcreteUI.Controls
{
    public interface IKeyEvents
    {
        void OnKeyDown(ref KeyInteractEventArgs args);
        void OnKeyUp(ref KeyInteractEventArgs args);
    }
}
