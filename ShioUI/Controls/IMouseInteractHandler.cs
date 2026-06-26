namespace ShioUI.Controls;

public interface IMouseInteractHandler
{
    void OnMouseDown(ref HandleableMouseEventArgs args);
    void OnMouseUp(in MouseEventArgs args);
}
