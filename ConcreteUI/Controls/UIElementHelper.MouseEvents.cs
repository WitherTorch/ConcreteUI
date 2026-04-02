using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        [StructLayout(LayoutKind.Auto)]
        public struct MouseMoveData
        {
            public SystemCursorType? CursorType;
            public UIElement? LastHitElement;

            public readonly void Deconstruct(out SystemCursorType? cursorType, out UIElement? lastHitElement)
            {
                cursorType = CursorType;
                lastHitElement = LastHitElement;
            }
        }

        public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DispatchEvent(elements, ref args, args.Location, &OnMouseDownForElement);

        private static void OnMouseDownForElement_OutOfBounds(UIElement element, ref HandleableMouseEventArgs args)
            => OnMouseDownForElement(element, ref args, false);

        public static unsafe void OnMouseDownForElement(UIElement element, ref HandleableMouseEventArgs args, bool isContains)
        {
            if (element is IElementContainer container)
            {
                IEnumerable<UIElement?> elements = container.GetElements();
                if (isContains)
                    DispatchEvent(elements, ref args, args.Location, &OnMouseDownForElement);
                else
                    DispatchEvent(elements, ref args, &OnMouseDownForElement_OutOfBounds);
            }
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseDownGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
            if (isContains && !args.Handled && element is IMouseInteractHandler handler)
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PointToLocal(args.Location), args.Buttons, args.Delta);
                handler.OnMouseDown(ref relativeArgs);
                if (relativeArgs.Handled)
                    args.Handle();
                return;
            }
        }

        public static unsafe void OnMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DispatchReadOnlyEvent(elements, in args, &OnMouseUpForElement);

        public static void OnMouseUpForElement(UIElement element, in MouseEventArgs args)
        {
            if (element is IElementContainer container)
                OnMouseUpForElements(container.GetElements(), in args);
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseUpGlobally(in args);
            if (element is IMouseInteractHandler handler)
                handler.OnMouseUp(new MouseEventArgs(element.PointToLocal(args.Location), args.Buttons, args.Delta));
        }

        public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref MouseMoveData data)
            where TEnumerable : IEnumerable<UIElement?>
            => DispatchEvent(elements, in args, ref data, args.Location, &OnMouseMoveForElement);

        private static void OnMouseMoveForElement_OutOfBounds(UIElement element, in MouseEventArgs args, ref MouseMoveData data)
            => OnMouseMoveForElement(element, in args, ref data, false);

        public static unsafe void OnMouseMoveForElement(UIElement element, in MouseEventArgs args, ref MouseMoveData data, bool isContains)
        {
            if (isContains)
            {
                data.LastHitElement = element;
                if (element is IElementContainer container)
                {
                    DispatchEvent(container.GetElements(), in args, ref data, args.Location, &OnMouseMoveForElement);

                    isContains = ReferenceEquals(data.LastHitElement, element);
                }
                if (element is IGlobalMouseMoveHandler globalHandler)
                    globalHandler.OnMouseMoveGlobally(in args);
                if (element is IMouseMoveHandler handler)
                    handler.OnMouseMove(new MouseEventArgs(element.PointToLocal(args.Location), args.Buttons, args.Delta));
                if (isContains && element is ICursorPredicator predicator)
                    data.CursorType = predicator.PredicatedCursor;
            }
            else
            {
                if (element is IElementContainer container)
                    DispatchEvent(container.GetElements(), in args, ref data, &OnMouseMoveForElement_OutOfBounds);
                if (element is IGlobalMouseMoveHandler globalHandler)
                    globalHandler.OnMouseMoveGlobally(in args);
            }
        }

        public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DispatchEvent(elements, ref args, args.Location, &OnMouseScrollForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElement_OutOfBounds(UIElement element, ref HandleableMouseEventArgs args)
            => OnMouseScrollForElement(element, ref args, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void OnMouseScrollForElement(UIElement element, ref HandleableMouseEventArgs args, bool isContains)
        {
            if (element is IElementContainer container)
            {
                IEnumerable<UIElement?> elements = container.GetElements();
                if (isContains)
                    DispatchEvent(elements, ref args, args.Location, &OnMouseScrollForElement);
                else
                    DispatchEvent(elements, ref args, &OnMouseScrollForElement_OutOfBounds);
            }
            if (element is IGlobalMouseScrollHandler globalHandler)
                globalHandler.OnMouseScrollGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
            if (isContains && !args.Handled && element is IMouseScrollHandler handler)
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PointToLocal(args.Location), args.Buttons, args.Delta);
                handler.OnMouseScroll(ref relativeArgs);
                if (relativeArgs.Handled)
                    args.Handle();
                return;
            }
        }
    }
}
