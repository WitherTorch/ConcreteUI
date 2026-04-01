using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoHybridEventForElements(elements, ref args, args.Location, &OnMouseDownForElement);

        private static void OnMouseDownForElement_OutOfBounds(UIElement? element, ref HandleableMouseEventArgs args)
            => OnMouseDownForElement(element, ref args, false);

        public static unsafe void OnMouseDownForElement(UIElement? element, ref HandleableMouseEventArgs args, bool isContains)
        {
            if (element is IElementContainer container)
            {
                IEnumerable<UIElement?> elements = container.GetElements();
                if (isContains)
                    OnMouseDownForElements(elements, ref args);
                else
                    DoHybridEventForElements(elements, ref args, &OnMouseDownForElement_OutOfBounds);
            }
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseDownGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
            if (isContains && !args.Handled && element is IMouseInteractHandler handler)
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(ToLocalPoint(element.Bounds, args.Location), args.Buttons, args.Delta);
                handler.OnMouseDown(ref relativeArgs);
                if (relativeArgs.Handled)
                    args.Handle();
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
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseUpGlobally(in args);
            if (element is IMouseInteractHandler handler)
                handler.OnMouseUp(new MouseEventArgs(ToLocalPoint(element.Bounds, args.Location), args.Buttons, args.Delta));
        }

        public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref SystemCursorType? cursorType)
            where TEnumerable : IEnumerable<UIElement?>
            => DoHybridEventForElements(elements, in args, ref cursorType, args.Location, &OnMouseMoveForElement);

        private static void OnMouseMoveForElement_OutOfBounds(UIElement? element, in MouseEventArgs args, ref SystemCursorType? cursorType)
            => OnMouseMoveForElement(element, in args, ref cursorType, false);

        public static unsafe void OnMouseMoveForElement(UIElement? element, in MouseEventArgs args, ref SystemCursorType? cursorType, bool isContains)
        {
            if (element is IElementContainer container)
            {
                IEnumerable<UIElement?> elements = container.GetElements();
                if (isContains)
                    OnMouseMoveForElements(container.GetElements(), in args, ref cursorType);
                else
                    DoHybridEventForElements(elements, in args, ref cursorType, &OnMouseMoveForElement_OutOfBounds);
            }
            if (element is IGlobalMouseMoveHandler globalHandler)
                globalHandler.OnMouseMoveGlobally(in args);
            if (element is IMouseMoveHandler handler)
                handler.OnMouseMove(new MouseEventArgs(ToLocalPoint(element.Bounds, args.Location), args.Buttons, args.Delta));
            if (isContains && cursorType is null && element is ICursorPredicator predicator)
                cursorType = predicator.PredicatedCursor;
        }

        public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoHybridEventForElements(elements, ref args, args.Location, &OnMouseScrollForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElement_OutOfBounds(UIElement? element, ref HandleableMouseEventArgs args)
            => OnMouseScrollForElement(element, ref args, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void OnMouseScrollForElement(UIElement? element, ref HandleableMouseEventArgs args, bool isContains)
        {
            if (element is IElementContainer container)
            {
                IEnumerable<UIElement?> elements = container.GetElements();
                if (isContains)
                    OnMouseScrollForElements(elements, ref args);
                else
                    DoHybridEventForElements(elements, ref args, &OnMouseScrollForElement_OutOfBounds);
            }
            if (element is IGlobalMouseScrollHandler globalHandler)
                globalHandler.OnMouseScrollGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
            if (isContains && !args.Handled && element is IMouseScrollHandler handler)
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(ToLocalPoint(element.Bounds, args.Location), args.Buttons, args.Delta);
                handler.OnMouseScroll(ref relativeArgs);
                if (relativeArgs.Handled)
                    args.Handle();
                return;
            }
        }

        private static PointF ToLocalPoint(in Rectangle bounds, PointF point)
            => new PointF(
                x: ToLocalCoordinate(bounds.Left, bounds.Right, point.X),
                y: ToLocalCoordinate(bounds.Top, bounds.Bottom, point.Y)
                );

        private static float ToLocalCoordinate(int left, int right, float coordinate)
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
