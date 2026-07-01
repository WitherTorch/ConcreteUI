using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ShioUI.Controls;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnKeyDownForElements<TEnumerable>(TEnumerable elements, ref KeyEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchHandleableEvent(elements, ref args, &OnKeyDownForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnKeyDownForElementsUnsafe(ref readonly UIElement? elementsRef,int count, ref KeyEventArgs args)
        => DispatchHandleableEventUnsafe(in elementsRef, count, ref args, &OnKeyDownForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnKeyDownForElement(UIElement? element, ref KeyEventArgs args)
    {
        if (element is IElementContainer container)
        {
            OnKeyDownForElements(container.GetActiveElements(), ref args);
            if (args.Handled)
                return;
        }
        if (element is IKeyboardInteractHandler keyEvents)
            keyEvents.OnKeyDown(ref args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnKeyUpForElements<TEnumerable>(TEnumerable elements, ref KeyEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchHandleableEvent(elements, ref args, &OnKeyUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnKeyUpForElementsUnsafe(ref readonly UIElement? elementsRef,int count, ref KeyEventArgs args)
        => DispatchHandleableEventUnsafe(in elementsRef, count, ref args, &OnKeyUpForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnKeyUpForElement(UIElement? element, ref KeyEventArgs args)
    {
        if (element is IElementContainer container)
        {
            OnKeyUpForElements(container.GetActiveElements(), ref args);
            if (args.Handled)
                return;
        }
        if (element is IKeyboardInteractHandler keyEvents)
            keyEvents.OnKeyUp(ref args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnCharacterInputForElements<TEnumerable>(TEnumerable elements, ref CharacterEventArgs args)
        where TEnumerable : IEnumerable<UIElement?>
        => DispatchHandleableEvent(elements, ref args, &OnCharacterInputForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void OnCharacterInputForElementsUnsafe(ref readonly UIElement? elementsRef,int count, ref CharacterEventArgs args)
        => DispatchHandleableEventUnsafe(in elementsRef, count, ref args, &OnCharacterInputForElement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnCharacterInputForElement(UIElement? element, ref CharacterEventArgs args)
    {
        if (element is IElementContainer container)
        {
            OnCharacterInputForElements(container.GetActiveElements(), ref args);
            if (args.Handled)
                return;
        }
        if (element is ICharacterInputHandler characterEvents)
            characterEvents.OnCharacterInput(ref args);
    }
}
