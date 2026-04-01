using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoHybridEventForElements(elements, ref args, &OnMouseDownForElement);

        public static void OnMouseDownForElement(UIElement? element, ref HandleableMouseEventArgs args)
        {
            if (element is IElementContainer container)
                OnMouseDownForElements(container.GetElements(), ref args);
            if (!args.Handled && element is IMouseInteractHandler mouseInteractEvents && element.Bounds.Contains(args.Location))
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(ToLocalPoint(), args.Buttons, args.Delta));
                mouseInteractEvents.OnMouseDown(ref args);
                return;
            }
        }

        public static unsafe void OnMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoNotifyEventForElements(elements, in args, &OnMouseUpForElement);

        public static void OnMouseUpForElement(UIElement? element, in MouseEventArgs args)
        {
            if (element is IElementContainer container)
                OnMouseUpForElements(container.GetElements(), in args);
            if (element is IMouseInteractHandler mouseEvents)
                mouseEvents.OnMouseUp(in args);
        }

        public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref SystemCursorType? cursorType)
            where TEnumerable : IEnumerable<UIElement?>
            => DoHybridEventForElements(elements, in args, ref cursorType, &OnMouseMoveForElement);

        public static void OnMouseMoveForElement(UIElement? element, in MouseEventArgs args, ref SystemCursorType? cursorType)
        {
            if (element is IElementContainer container)
                OnMouseMoveForElements(container.GetElements(), in args, ref cursorType);
            if (element is IMouseMoveHandler mouseEvents)
                mouseEvents.OnMouseMove(in args);
            if (cursorType is null && element is ICursorPredicator predicator)
                cursorType = predicator.PredicatedCursor;
        }

        public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoInteractEventForElements(elements, ref args, &OnMouseScrollForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElement(UIElement? element, ref HandleableMouseEventArgs args)
        {
            if (element is IElementContainer container)
            {
                OnMouseScrollForElements(container.GetElements(), ref args);
                if (args.Handled)
                    return;
            }
            if (element is IMouseScrollHandler scrollEvent && element.Bounds.Contains(args.Location))
                scrollEvent.OnMouseScroll(ref args);
        }

        private static PointF ToLocalPoint(RectF bounds, PointF point)
            => new PointF(
                x: ToLocalCoordinate(bounds.Left, bounds.Right, point.X),
                y: ToLocalCoordinate(bounds.Top, bounds.Bottom, point.Y)
                );

        private static float ToLocalCoordinate(float left, float right, float coordinate)
        {
            if (coordinate < left)
                return -1;
            else if (coordinate > right)
                return right - left + 1;
            else
                return coordinate - left;
        }
    }
}
