using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static unsafe void OnKeyDownForElements<TEnumerable>(TEnumerable elements, ref KeyInteractEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoInteractEventForElements(elements, ref args, &OnKeyDownForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyDownForElement(UIElement? element, ref KeyInteractEventArgs args)
        {
            if (element is IElementContainer container)
            {
                OnKeyDownForElements(container.GetElements(), ref args);
                if (args.Handled)
                    return;
            }
            if (element is IKeyEvents keyEvents)
                keyEvents.OnKeyDown(ref args);
        }

        public static unsafe void OnKeyUpForElements<TEnumerable>(TEnumerable elements, ref KeyInteractEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoInteractEventForElements(elements, ref args, &OnKeyUpForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyUpForElement(UIElement? element, ref KeyInteractEventArgs args)
        {
            if (element is IElementContainer container)
            {
                OnKeyUpForElements(container.GetElements(), ref args);
                if (args.Handled)
                    return;
            }
            if (element is IKeyEvents keyEvents)
                keyEvents.OnKeyUp(ref args);
        }

        public static unsafe void OnCharacterInputForElements<TEnumerable>(TEnumerable elements, ref CharacterInteractEventArgs args)
            where TEnumerable : IEnumerable<UIElement?>
            => DoInteractEventForElements(elements, ref args, &OnCharacterInputForElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnCharacterInputForElement(UIElement? element, ref CharacterInteractEventArgs args)
        {
            if (element is IElementContainer container)
            {
                OnCharacterInputForElements(container.GetElements(), ref args);
                if (args.Handled)
                    return;
            }
            if (element is ICharacterEvents characterEvents)
                characterEvents.OnCharacterInput(ref args);
        }
    }
}
