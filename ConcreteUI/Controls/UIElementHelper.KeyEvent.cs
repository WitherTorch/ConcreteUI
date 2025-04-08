using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using InlineMethod;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static void OnKeyDownForElements(IEnumerable<UIElement> elements, KeyEventArgs args)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnKeyDownForElements(_array, args);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnKeyDownForElements(_list, args);
                    break;
                case ObservableList<UIElement> _list:
                    OnKeyDownForElements(_list.GetUnderlyingList(), args);
                    break;
                default:
                    OnKeyDownForElementsCore(elements, args);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void OnKeyDownForElementsCore(IEnumerable<UIElement> elements, KeyEventArgs args)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnKeyDownForElement(element, args);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnKeyDownForElements(UIElement[] elements, KeyEventArgs args)
            => OnKeyDownForElements(elements, elements.Length, args);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnKeyDownForElements(UnwrappableList<UIElement> elements, KeyEventArgs args)
            => OnKeyDownForElements(elements.Unwrap(), elements.Count, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyDownForElements(UIElement[] elements, int length, KeyEventArgs args)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnKeyDownForElement(element, args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyDownForElement(UIElement element, KeyEventArgs args)
        {
            if (element is IContainerElement containerElement)
                OnKeyDownForElements(containerElement.Children, args);
            if (element is IKeyEvents keyEvents)
                keyEvents.OnKeyDown(args);
        }

        public static void OnKeyUpForElements(IEnumerable<UIElement> elements, KeyEventArgs args)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnKeyUpForElements(_array, args);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnKeyUpForElements(_list, args);
                    break;
                case ObservableList<UIElement> _list:
                    OnKeyUpForElements(_list.GetUnderlyingList(), args);
                    break;
                default:
                    OnKeyUpForElementsCore(elements, args);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static void OnKeyUpForElementsCore(IEnumerable<UIElement> elements, KeyEventArgs args)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnKeyUpForElement(element, args);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnKeyUpForElements(UIElement[] elements, KeyEventArgs args)
            => OnKeyUpForElements(elements, elements.Length, args);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnKeyUpForElements(UnwrappableList<UIElement> elements, KeyEventArgs args)
            => OnKeyUpForElements(elements.Unwrap(), elements.Count, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyUpForElements(UIElement[] elements, int length, KeyEventArgs args)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnKeyUpForElement(element, args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnKeyUpForElement(UIElement element, KeyEventArgs args)
        {
            if (element is IContainerElement containerElement)
                OnKeyUpForElements(containerElement.Children, args);
            if (element is IKeyEvents keyEvents)
                keyEvents.OnKeyUp(args);
        }

        public static void OnCharacterInputForElements(IEnumerable<UIElement> elements, char character)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnCharacterInputForElements(_array, character);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnCharacterInputForElements(_list, character);
                    break;
                case ObservableList<UIElement> _list:
                    OnCharacterInputForElements(_list.GetUnderlyingList(), character);
                    break;
                default:
                    OnCharacterInputForElementsCore(elements, character);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static void OnCharacterInputForElementsCore(IEnumerable<UIElement> elements, char character)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnCharacterInputForElement(element, character);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnCharacterInputForElements(UIElement[] elements, char character)
            => OnCharacterInputForElements(elements, elements.Length, character);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnCharacterInputForElements(UnwrappableList<UIElement> elements, char character)
            => OnCharacterInputForElements(elements.Unwrap(), elements.Count, character);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnCharacterInputForElements(UIElement[] elements, int length, char character)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnCharacterInputForElement(element, character);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnCharacterInputForElement(UIElement element, char character)
        {
            if (element is IContainerElement containerElement)
                OnCharacterInputForElements(containerElement.Children, character);
            if (element is ICharacterEvents characterEvents)
                characterEvents.OnCharacterInput(character);
        }
    }
}
