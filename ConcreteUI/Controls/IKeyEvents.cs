namespace ConcreteUI.Controls
{
    public interface IKeyEvents
    {
        void OnKeyDown(in KeyInteractEventArgs args);
        void OnKeyUp(in KeyInteractEventArgs args);
    }
}
