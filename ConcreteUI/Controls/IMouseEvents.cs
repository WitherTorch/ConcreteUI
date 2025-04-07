using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    public interface IMouseEvents
    {
        void OnMouseMove(in MouseInteractEventArgs args);
        void OnMouseDown(in MouseInteractEventArgs args);
        void OnMouseUp(in MouseInteractEventArgs args);
    }
}
