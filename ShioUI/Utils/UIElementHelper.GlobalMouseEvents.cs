using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ShioUI.Controls;
using ShioUI.Extensions;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseDownForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnGlobalMouseDownForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseDownForElementsUnsafe(ref readonly UIElement? elementsRef,int count, in MouseEventArgs args)
        => DispatchReadOnlyEventUnsafe(in elementsRef, count, in args, &OnGlobalMouseDownForElement);

    public static unsafe void OnGlobalMouseDownForElement(UIElement element, in MouseEventArgs args)
    {
        if (element is IElementContainer container)
        {
            PointF innerPageLocation = args.Location;
            if (element is IRenderWindow window)
                innerPageLocation = window.PageToInnerPage(innerPageLocation);
            else
                innerPageLocation = element.PageToLocal(innerPageLocation);

            DispatchReadOnlyEvent(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta), &OnGlobalMouseDownForElement);
        }
        if (element is IGlobalMouseInteractHandler globalHandler)
            globalHandler.OnMouseDownGlobally(in args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseUpForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnGlobalMouseUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseUpForElementsUnsafe(ref readonly UIElement? elementsRef,int count, in MouseEventArgs args)
        => DispatchReadOnlyEventUnsafe(in elementsRef, count, in args, &OnGlobalMouseUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseUpForElement(UIElement element, in MouseEventArgs args)
    {
        if (element is IElementContainer container)
        {
            PointF innerPageLocation = args.Location;
            if (element is IRenderWindow window)
                innerPageLocation = window.PageToInnerPage(innerPageLocation);
            else
                innerPageLocation = element.PageToLocal(innerPageLocation);

            DispatchReadOnlyEvent(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta), &OnGlobalMouseUpForElement);
        }
        if (element is IGlobalMouseInteractHandler globalHandler)
            globalHandler.OnMouseUpGlobally(in args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseMoveForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnGlobalMouseMoveForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseMoveForElementsUnsafe(ref readonly UIElement? elementsRef,int count, in MouseEventArgs args)
        => DispatchReadOnlyEventUnsafe(in elementsRef, count, in args, &OnGlobalMouseMoveForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseMoveForElement(UIElement element, in MouseEventArgs args)
    {
        if (element is IElementContainer container)
        {
            PointF innerPageLocation = args.Location;
            if (element is IRenderWindow window)
                innerPageLocation = window.PageToInnerPage(innerPageLocation);
            else
                innerPageLocation = element.PageToLocal(innerPageLocation);

            DispatchReadOnlyEvent(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta), &OnGlobalMouseMoveForElement);
        }
        if (element is IGlobalMouseMoveHandler globalHandler)
            globalHandler.OnMouseMoveGlobally(in args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseScrollForElements<TEnumerable>(TEnumerable elements, in MouseEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchReadOnlyEvent(elements, in args, &OnGlobalMouseScrollForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseScrollForElementsUnsafe(ref readonly UIElement? elementsRef,int count, in MouseEventArgs args)
        => DispatchReadOnlyEventUnsafe(in elementsRef, count, in args, &OnGlobalMouseScrollForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnGlobalMouseScrollForElement(UIElement element, in MouseEventArgs args)
    {
        if (element is IElementContainer container)
        {
            PointF innerPageLocation = args.Location;
            if (element is IRenderWindow window)
                innerPageLocation = window.PageToInnerPage(innerPageLocation);
            else
                innerPageLocation = element.PageToLocal(innerPageLocation);

            DispatchReadOnlyEvent(container.GetActiveElements(), new MouseEventArgs(innerPageLocation, args.Buttons, args.Delta), &OnGlobalMouseScrollForElement);
        }
        if (element is IGlobalMouseScrollHandler globalHandler)
            globalHandler.OnMouseScrollGlobally(in args);
    }
}
