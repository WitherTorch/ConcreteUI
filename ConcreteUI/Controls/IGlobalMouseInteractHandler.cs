namespace ConcreteUI.Controls
{
    public interface IGlobalMouseInteractHandler
    {
        void OnMouseDownGlobally(in MouseEventArgs args);
        void OnMouseUpGlobally(in MouseEventArgs args);
    }
}
