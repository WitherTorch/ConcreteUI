using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

#pragma warning disable CS8500

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoInteractEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
            delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement> where TEventArgs : struct, IInteractEventArgs
        {
            if (args.Handled)
                return;

            UIElement[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement[]) || elements is UIElement[])
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement>) || elements is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement>) || elements is ObservableList<UIElement>)
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement>) || elements is IList<UIElement>)
                goto List;

            goto Fallback;

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement element = UnsafeHelper.AddByteOffset(ref elementRef, i * sizeof(UIElement));
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
            return;

        List:
            IList<UIElement> list = UnsafeHelper.As<TEnumerable, IList<UIElement>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
            return;

        Fallback:
            using IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
            delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement> where TEventArgs : struct
        {
            UIElement[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement[]) || elements is UIElement[])
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement>) || elements is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement>) || elements is ObservableList<UIElement>)
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement>) || elements is IList<UIElement>)
                goto List;

            goto Fallback;

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement element = UnsafeHelper.AddByteOffset(ref elementRef, i * sizeof(UIElement));
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
            return;

        List:
            IList<UIElement> list = UnsafeHelper.As<TEnumerable, IList<UIElement>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
            return;

        Fallback:
            using IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data,
            delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement> where TEventArgs : struct
        {
            UIElement[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement[]) || elements is UIElement[])
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement>) || elements is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement>) || elements is ObservableList<UIElement>)
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement>) || elements is IList<UIElement>)
                goto List;

            goto Fallback;

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement element = UnsafeHelper.AddByteOffset(ref elementRef, i * sizeof(UIElement));
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
            return;

        List:
            IList<UIElement> list = UnsafeHelper.As<TEnumerable, IList<UIElement>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
            return;

        Fallback:
            using IEnumerator<UIElement> enumerator = elements.Reverse().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoNotifyEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args,
            delegate* managed<UIElement, in TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement> where TEventArgs : struct
            => DoHybridEventForElements(elements, ref UnsafeHelper.AsRefIn(in args), (delegate* managed<UIElement, ref TEventArgs, void>)eventHandler);
    }
}
