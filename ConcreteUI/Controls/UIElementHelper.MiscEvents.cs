using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
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
            if (element is IDpiAwareEvents keyEvents)
                keyEvents.OnDpiChanged(in args);
        }
    }
}
