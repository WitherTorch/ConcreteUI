using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Controls;
using ShioUI.Extensions;

namespace ShioUI.Utils;

[StructLayout(LayoutKind.Auto)]
public struct HitTestData
{
    public UIElement? LastHitElement;
}

[StructLayout(LayoutKind.Auto)]
public readonly struct MouseUpData
{
    public readonly PooledList<UIElement> HitElements;

    public MouseUpData(PooledList<UIElement> hitElements) => HitElements = hitElements;
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

partial class UIElementHelper
{
    public static unsafe void OnMouseDownForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args, ref HitTestData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchEvent(elements, ref args, ref data, args.Location, &OnMouseDownForElement);

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
                PointF innerPageLocation = args.Location;
                if (element is IRenderWindow window)
                    innerPageLocation = window.PageToInnerPage(innerPageLocation);
                else
                    innerPageLocation = element.PageToLocal(innerPageLocation);

                HandleableMouseEventArgs innerArgs = new HandleableMouseEventArgs(innerPageLocation, args.Buttons, args.Delta);
                DispatchEvent(container.GetActiveElements(), ref args, ref data, innerPageLocation, &OnMouseDownForElement);
                if (innerArgs.Handled)
                    args.Handle();

                DoGlobalEvent(element, ref args);
                if (ReferenceEquals(data.LastHitElement, element))
                    goto Contains;
            }
            else
            {
                DoGlobalEvent(element, ref args);
                goto Contains;
            }
        }
        else
        {
            OnGlobalMouseDownForElement(element, in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        }

        return;

    Contains:
        if (element is IMouseInteractHandler handler)
        {
            HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta);
            handler.OnMouseDown(ref relativeArgs);
            if (relativeArgs.Handled)
                args.Handle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DoGlobalEvent(UIElement element, ref HandleableMouseEventArgs args)
        {
            if (element is IGlobalMouseInteractHandler globalHandler)
                globalHandler.OnMouseDownGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        }
    }

    public static unsafe void OnMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref MouseUpData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, ref data, &OnMouseUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseUpForElement(UIElement element, in MouseEventArgs args, ref MouseUpData data)
    {
        if (element is IElementContainer container)
        {
            PointF innerPageLocation = args.Location;
            if (element is IRenderWindow window)
                innerPageLocation = window.PageToInnerPage(innerPageLocation);
            else
                innerPageLocation = element.PageToLocal(innerPageLocation);
            if (data.HitElements.Count <= 0)
                OnGlobalMouseUpForElements(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta));
            else
                OnMouseUpForElements(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta), ref data);
        }
        if (element is IGlobalMouseInteractHandler globalHandler)
            globalHandler.OnMouseUpGlobally(in args);
        if (data.HitElements.Remove(element) && element is IMouseInteractHandler handler)
            handler.OnMouseUp(new MouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta));
    }

    public static unsafe void OnMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args, ref MouseMoveData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, ref data, args.Location, &OnMouseMoveForElement);

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
                PointF innerPageLocation = args.Location;
                if (element is IRenderWindow window)
                    innerPageLocation = window.PageToInnerPage(innerPageLocation);
                else
                    innerPageLocation = element.PageToLocal(innerPageLocation);

                DispatchReadOnlyEvent(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta),
                    ref data, innerPageLocation, &OnMouseMoveForElement);

                DoGlobalEvent(element, args);
                if (ReferenceEquals(data.LastHitElement, element))
                    goto Contains;
            }
            else
            {
                DoGlobalEvent(element, args);
                goto Contains;
            }
        }
        else
            OnGlobalMouseMoveForElement(element, args);
        return;

    Contains:
        if (element is IMouseMoveHandler handler)
            handler.OnMouseMove(new MouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta));
        if (element is ICursorStateHandler predicator)
            data.CursorType = predicator.Cursor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DoGlobalEvent(UIElement element, in MouseEventArgs args)
        {
            if (element is IGlobalMouseMoveHandler globalHandler)
                globalHandler.OnMouseMoveGlobally(in args);
        }
    }

    public static unsafe void OnMouseScrollForElements<TEnumerable>(TEnumerable elements, ref HandleableMouseEventArgs args, ref HitTestData data)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchEvent(elements, ref args, ref data, args.Location, &OnMouseScrollForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnMouseScrollForElement(UIElement element, ref HandleableMouseEventArgs args, ref HitTestData data)
        => OnMouseScrollForElement(element, ref args, ref data, element.Bounds.Contains(args.Location));

    private static unsafe void OnMouseScrollForElement(UIElement element, ref HandleableMouseEventArgs args, ref HitTestData data, bool isContains)
    {
        if (isContains)
        {
            data.LastHitElement = element;
            if (element is IElementContainer container)
            {
                PointF innerPageLocation = args.Location;
                if (element is IRenderWindow window)
                    innerPageLocation = window.PageToInnerPage(innerPageLocation);
                else
                    innerPageLocation = element.PageToLocal(innerPageLocation);

                HandleableMouseEventArgs innerArgs = new HandleableMouseEventArgs(innerPageLocation, args.Buttons, args.Delta);
                DispatchEvent(container.GetActiveElements(), ref args, ref data, innerPageLocation, &OnMouseScrollForElement);
                if (innerArgs.Handled)
                    args.Handle();

                DoGlobalEvent(element, ref args);
                if (ReferenceEquals(data.LastHitElement, element))
                    goto Contains;
            }
            else
            {
                DoGlobalEvent(element, ref args);
                goto Contains;
            }
        }
        else
        {
            OnGlobalMouseScrollForElement(element, in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        }

        return;

    Contains:
        if (element is IMouseScrollHandler handler)
        {
            HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(element.PageToLocal(args.Location), args.Buttons, args.Delta);
            handler.OnMouseScroll(ref relativeArgs);
            if (relativeArgs.Handled)
                args.Handle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DoGlobalEvent(UIElement element, ref HandleableMouseEventArgs args)
        {
            if (element is IGlobalMouseScrollHandler globalHandler)
                globalHandler.OnMouseScrollGlobally(in UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args));
        }
    }
}
