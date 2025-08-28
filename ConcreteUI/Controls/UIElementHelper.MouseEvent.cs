using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Utils;
using ConcreteUI.Window2;

using InlineMethod;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        public static void OnMouseDownForElements(IEnumerable<UIElement> elements, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnMouseDownForElements(_array, args, ref allowRegionalMouseEvent);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnMouseDownForElements(_list, args, ref allowRegionalMouseEvent);
                    break;
                case ObservableList<UIElement> _list:
                    OnMouseDownForElements(_list.GetUnderlyingList(), args, ref allowRegionalMouseEvent);
                    break;
                default:
                    OnMouseDownForElementsCore(elements, args, ref allowRegionalMouseEvent);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void OnMouseDownForElementsCore(IEnumerable<UIElement> elements, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnMouseDownForElement(element, args, ref allowRegionalMouseEvent);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseDownForElements(UIElement[] elements, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
            => OnMouseDownForElements(elements, elements.Length, in args, ref allowRegionalMouseEvent);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseDownForElements(UnwrappableList<UIElement> elements, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
            => OnMouseDownForElements(elements.Unwrap(), elements.Count, in args, ref allowRegionalMouseEvent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseDownForElements(UIElement[] elements, int length, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnMouseDownForElement(element, args, ref allowRegionalMouseEvent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseDownForElement(UIElement element, in MouseInteractEventArgs args, ref bool allowRegionalMouseEvent)
        {
            if (element is IContainerElement containerElement)
                OnMouseDownForElements(containerElement.Children, args, ref allowRegionalMouseEvent);
            if (element is IMouseEvents mouseEvents)
            {
                if (element is IGlobalMouseEvents)
                {
                    mouseEvents.OnMouseDown(args);
                    return;
                }
                if (allowRegionalMouseEvent && element.Bounds.Contains(args.Location))
                {
                    allowRegionalMouseEvent = false;
                    mouseEvents.OnMouseDown(args);
                }
            }
        }

        public static void OnMouseUpForElements(IEnumerable<UIElement> elements, in MouseInteractEventArgs args)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnMouseUpForElements(_array, args);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnMouseUpForElements(_list, args);
                    break;
                case ObservableList<UIElement> _list:
                    OnMouseUpForElements(_list.GetUnderlyingList(), args);
                    break;
                default:
                    OnMouseUpForElementsCore(elements, args);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static void OnMouseUpForElementsCore(IEnumerable<UIElement> elements, in MouseInteractEventArgs args)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnMouseUpForElements(element, args);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseUpForElements(UIElement[] elements, in MouseInteractEventArgs args)
            => OnMouseUpForElements(elements, elements.Length, in args);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseUpForElements(UnwrappableList<UIElement> elements, in MouseInteractEventArgs args)
            => OnMouseUpForElements(elements.Unwrap(), elements.Count, in args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseUpForElements(UIElement[] elements, int length, in MouseInteractEventArgs args)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnMouseUpForElements(element, args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseUpForElements(UIElement element, in MouseInteractEventArgs args)
        {
            if (element is IContainerElement containerElement)
                OnMouseUpForElements(containerElement.Children, args);
            if (element is IMouseEvents mouseEvents)
                mouseEvents.OnMouseUp(args);
        }

        public static void OnMouseMoveForElements(IEnumerable<UIElement> elements, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnMouseMoveForElements(_array, args, ref cursorType);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnMouseMoveForElements(_list, args, ref cursorType);
                    break;
                case ObservableList<UIElement> _list:
                    OnMouseMoveForElements(_list.GetUnderlyingList(), args, ref cursorType);
                    break;
                default:
                    OnMouseMoveForElementsCore(elements, args, ref cursorType);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void OnMouseMoveForElementsCore(IEnumerable<UIElement> elements, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnMouseMoveForElement(element, args, ref cursorType);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseMoveForElements(UIElement[] elements, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
            => OnMouseMoveForElements(elements, elements.Length, in args, ref cursorType);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseMoveForElements(UnwrappableList<UIElement> elements, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
            => OnMouseMoveForElements(elements.Unwrap(), elements.Count, in args, ref cursorType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseMoveForElements(UIElement[] elements, int length, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnMouseMoveForElement(element, args, ref cursorType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseMoveForElement(UIElement element, in MouseInteractEventArgs args, ref SystemCursorType? cursorType)
        {
            if (element is IContainerElement containerElement)
                OnMouseMoveForElements(containerElement.Children, args, ref cursorType);
            if (element is IMouseEvents mouseEvents)
                mouseEvents.OnMouseMove(args);
            if (cursorType is null && element is ICursorPredicator predicator)
                cursorType = predicator.PredicatedCursor;
        }

        public static void OnMouseScrollForElements(IEnumerable<UIElement> elements, in MouseInteractEventArgs args)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    OnMouseScrollForElements(_array, args);
                    break;
                case UnwrappableList<UIElement> _list:
                    OnMouseScrollForElements(_list, args);
                    break;
                case ObservableList<UIElement> _list:
                    OnMouseScrollForElements(_list.GetUnderlyingList(), args);
                    break;
                default:
                    OnMouseScrollForElementsCore(elements, args);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void OnMouseScrollForElementsCore(IEnumerable<UIElement> elements, in MouseInteractEventArgs args)
        {
            IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                OnMouseScrollForElement(element, args);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseScrollForElements(UIElement[] elements, in MouseInteractEventArgs args)
            => OnMouseScrollForElements(elements, elements.Length, args);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void OnMouseScrollForElements(UnwrappableList<UIElement> elements, in MouseInteractEventArgs args)
            => OnMouseScrollForElements(elements.Unwrap(), elements.Count, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElements(UIElement[] elements, int length, in MouseInteractEventArgs args)
        {
            for (int count = elements.Length, i = count - 1; i >= 0; i--)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                OnMouseScrollForElement(element, args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMouseScrollForElement(UIElement element, in MouseInteractEventArgs args)
        {
            if (element is IContainerElement containerElement)
                OnMouseScrollForElements(containerElement.Children, args);
            if (element is IMouseScrollEvent scrollEvent && element.Bounds.Contains(args.Location))
                scrollEvent.OnMouseScroll(args);
        }
    }
}
