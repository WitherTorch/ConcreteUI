using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref MouseInteractEventArgs args)
            where TEnumerable : IEnumerable<UIElement>
            => DoHybridEventForElements(elements, ref args, &OnMouseDownForElement);

        public static void OnMouseDownForElement(UIElement element, ref MouseInteractEventArgs args)
        {
            if (element is IElementContainer container)
                OnMouseDownForElements(container.GetElements(), ref args);
            if (!args.Handled && element is IMouseInteractEvents mouseInteractEvents && element.Bounds.Contains(args.Location))
            {
                mouseInteractEvents.OnMouseDown(ref args);
                return;
            }
            if (element is IMouseNotifyEvents mouseNotifyEvents)
            {
                mouseNotifyEvents.OnMouseDown(in UnsafeHelper.As<MouseInteractEventArgs, MouseNotifyEventArgs>(ref args));
                return;
            }
        }

        public static unsafe void OnMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseNotifyEventArgs args)
            where TEnumerable : IEnumerable<UIElement>
            => DoNotifyEventForElements(elements, in args, &OnMouseUpForElement);

        public static void OnMouseUpForElement(UIElement element, in MouseNotifyEventArgs args)
        {
            if (element is IElementContainer container)
                OnMouseUpForElements(container.GetElements(), in args);
            if (element is IMouseEvents mouseEvents)
                mouseEvents.OnMouseUp(in args);
        }

        public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseNotifyEventArgs args, ref SystemCursorType? cursorType)
            where TEnumerable : IEnumerable<UIElement>
            => DoHybridEventForElements(elements, in args, ref cursorType, &OnMouseMoveForElement);

        public static void OnMouseMoveForElement(UIElement element, in MouseNotifyEventArgs args, ref SystemCursorType? cursorType)
        {
            if (element is IElementContainer container)
                OnMouseMoveForElements(container.GetElements(), in args, ref cursorType);
            if (element is IMouseEvents mouseEvents)
                mouseEvents.OnMouseMove(in args);
            if (cursorType is null && element is ICursorPredicator predicator)
                cursorType = predicator.PredicatedCursor;
        }

        public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref MouseInteractEventArgs args)
            where TEnumerable : IEnumerable<UIElement>
            => DoInteractEventForElements(elements, ref args, &OnMouseScrollForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElement(UIElement element, ref MouseInteractEventArgs args)
        {
            if (element is IElementContainer container)
            {
                OnMouseScrollForElements(container.GetElements(), ref args);
                if (args.Handled)
                    return;
            }
            if (element is IMouseScrollEvent scrollEvent && element.Bounds.Contains(args.Location))
                scrollEvent.OnMouseScroll(ref args);
        }
    }
}
