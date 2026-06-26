using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ShioUI.Controls;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    public static unsafe void OnDpiChangedForElements<TEnumerable>(TEnumerable elements, in DpiChangedEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnDpiChangedForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnDpiChangedForElement(UIElement? element, in DpiChangedEventArgs args)
    {
        if (element is IElementContainer container)
            OnDpiChangedForElements(container.GetActiveElements(), in args);
        if (element is IDpiChangedHandler keyEvents)
            keyEvents.OnDpiChanged(in args);
    }

    public static unsafe void DisposeForElements<TEnumerable>(TEnumerable elements)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, &DisposeForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeForElement(UIElement? element) => element?.Dispose(); // 無需替 IElementContainer 清理其子項，因為元件自己有自己的清理規則
}
