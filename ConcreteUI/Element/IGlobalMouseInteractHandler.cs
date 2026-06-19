namespace ConcreteUI.Element;

public interface IGlobalMouseInteractHandler
{
    void OnMouseDownGlobally(in MouseEventArgs args);
    void OnMouseUpGlobally(in MouseEventArgs args);
}
