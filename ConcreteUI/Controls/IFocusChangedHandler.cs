namespace ConcreteUI.Controls;

public readonly record struct FocusChangedEventArgs(bool State, UIElement? FocusedElement);

public interface IFocusChangedHandler
{
    void OnFocusChanged(in FocusChangedEventArgs args);
}
