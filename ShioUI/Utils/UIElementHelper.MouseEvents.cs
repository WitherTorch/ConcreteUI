using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ShioUI.Controls;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    [StructLayout(LayoutKind.Auto)]
    public struct HitTestData
    {
        public UIElement? LastHitElement;
    }

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

    public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args, ref HitTestData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchEvent(elements, ref args, ref data, args.Location, &OnMouseDownForElement);

    private static void OnMouseDownForElement_OutOfBounds(UIElement element, ref HandleableMouseEventArgs args, ref HitTestData data)
        => OnMouseDownForElement(element, ref args, ref data, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseDownForElement(UIElement element, ref HandleableMouseEventArgs args, ref HitTestData data) 
        => OnMouseDownForElement(element, ref args, ref data, element.Bounds.Contains(args.Location));

    private static unsafe void OnMouseDownForElement(UIElement element, ref HandleableMouseEventArgs args, ref HitTestData data, bool isContains)
    {
        if (isContains)
        {
            data.LastHitElement = element;
            if (element is IElementContainer container)
            {
                DispatchEvent(container.GetActiveElements(), ref args, ref data, args.Location, &OnMouseDownForElement);

                isContains = ReferenceEquals(data.LastHitElement, element);
            }
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseDownGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
            if (element is IMouseInteractHandler handler)
            {
                HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta);
                handler.OnMouseDown(ref relativeArgs);
                if (relativeArgs.Handled)
                    args.Handle();
            }
        }
        else
        {
            if (element is IElementContainer container)
                DispatchEvent(container.GetActiveElements(), ref args, ref data, &OnMouseDownForElement_OutOfBounds);
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseDownGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        }
    }

    public static unsafe void OnMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnMouseUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseUpForElement(UIElement element, in MouseEventArgs args)
    {
        if (element is IElementContainer container)
            OnMouseUpForElements(container.GetActiveElements(), in args);
        if (element is IGlobalMouseInteractHandler globalHandler)
            globalHandler.OnMouseUpGlobally(in args);
        if (element is IMouseInteractHandler handler)
            handler.OnMouseUp(new MouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta));
    }

    public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref MouseMoveData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, ref data, args.Location, &OnMouseMoveForElement);

    private static void OnMouseMoveForElement_OutOfBounds(UIElement element, in MouseEventArgs args, ref MouseMoveData data)
        => OnMouseMoveForElement(element, in args, ref data, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseMoveForElement(UIElement element, in MouseEventArgs args, ref MouseMoveData data)
        => OnMouseMoveForElement(element, in args, ref data, element.Bounds.Contains(args.Location));

    private static unsafe void OnMouseMoveForElement(UIElement element, in MouseEventArgs args, ref MouseMoveData data, bool isContains)
    {
        if (isContains)
        {
            data.LastHitElement = element;
            if (element is IElementContainer container)
            {
                DispatchReadOnlyEvent(container.GetActiveElements(), in args, ref data, args.Location, &OnMouseMoveForElement);

                isContains = ReferenceEquals(data.LastHitElement, element);
            }
            if (element is IGlobalMouseMoveHandler globalHandler)
                globalHandler.OnMouseMoveGlobally(in args);
            if (element is IMouseMoveHandler handler)
                handler.OnMouseMove(new MouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta));
            if (isContains && element is ICursorStateHandler predicator)
                data.CursorType = predicator.Cursor;
        }
        else
        {
            if (element is IElementContainer container)
                DispatchReadOnlyEvent(container.GetActiveElements(), in args, ref data, &OnMouseMoveForElement_OutOfBounds);
            if (element is IGlobalMouseMoveHandler globalHandler)
                globalHandler.OnMouseMoveGlobally(in args);
        }
    }

    public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchEvent(elements, ref args, args.Location, &OnMouseScrollForElement);

    private static void OnMouseScrollForElement_OutOfBounds(UIElement element, ref HandleableMouseEventArgs args)
        => OnMouseScrollForElement(element, ref args, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseScrollForElement(UIElement element, ref HandleableMouseEventArgs args)
        => OnMouseScrollForElement(element, ref args, element.Bounds.Contains(args.Location));

    private static unsafe void OnMouseScrollForElement(UIElement element, ref HandleableMouseEventArgs args, bool isContains)
    {
        if (element is IElementContainer container)
        {
            IEnumerable<UIElement?> elements = container.GetActiveElements();
            if (isContains)
                DispatchEvent(elements, ref args, args.Location, &OnMouseScrollForElement);
            else
                DispatchEvent(elements, ref args, &OnMouseScrollForElement_OutOfBounds);
        }
        if (element is IGlobalMouseScrollHandler globalHandler)
            globalHandler.OnMouseScrollGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        if (isContains && !args.Handled && element is IMouseScrollHandler handler)
        {
            HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta);
            handler.OnMouseScroll(ref relativeArgs);
            if (relativeArgs.Handled)
                args.Handle();
            return;
        }
    }
}
