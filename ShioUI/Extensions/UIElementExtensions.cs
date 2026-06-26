using System.Runtime.CompilerServices;

using ShioUI.Controls;

namespace ShioUI.Extensions;

public static class UIElementExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Focus<TElement>(this TElement _this) where TElement : UIElement, IFocusChangedHandler
        => _this.Window.ChangeFocusElement(_this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFocus<TElement>(this TElement _this, bool state) where TElement : UIElement, IFocusChangedHandler
    {
        if (state)
            _this.Window.ChangeFocusElement(_this);
        else
            _this.Window.ClearFocusElement(elementForValidation: _this);
    }
}
