using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

#pragma warning disable CS8500

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        private static readonly Rect AllBitSetsRect = new Rect(-1, -1, -1, -1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoInteractEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
            delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct, IHandleableEventArgs
        {
            if (args.Handled)
                return;

            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement? elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = readOnlyList[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.ReverseOptimized().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement? element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, ref args);
                if (args.Handled)
                    return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoNotifyEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args,
            delegate* managed<UIElement, in TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
            => DoHybridEventForElements(elements, ref UnsafeHelper.AsRefIn(in args), (delegate* managed<UIElement, ref TEventArgs, void>)eventHandler);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoNotifyEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args, Point focusPoint,
            delegate* managed<UIElement, in TEventArgs, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
            => DoHybridEventForElements(elements, ref UnsafeHelper.AsRefIn(in args), focusPoint, (delegate* managed<UIElement, ref TEventArgs, bool, void>)eventHandler);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
            delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement? elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = readOnlyList[i];
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.ReverseOptimized().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement? element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, ref args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data,
            delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement? elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = list[i];
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = readOnlyList[i];
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.ReverseOptimized().GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement? element = enumerator.Current;
                if (element is null)
                    continue;
                eventHandler(element, in args, ref data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args, PointF focusPoint,
            delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    list.CopyTo(buffer, 0);
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    for (int i = 0, j = 0; i < length; i++)
                        buffer[j++] = readOnlyList[i];
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        Fallback:
            {
                using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
                if (!enumerator.MoveNext())
                    return;
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                using PooledList<UIElement?> bufferList = new(pool);
                do
                {
                    bufferList.Add(enumerator.Current);
                } while (enumerator.MoveNext());
                (UIElement?[] buffer, int count) = bufferList;
                try
                {
                    DoHybridEventForElementsCore(in buffer[0], (nuint)count, ref args, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DoHybridEventForElements<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data, PointF focusPoint,
            delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    list.CopyTo(buffer, 0);
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    for (int i = 0, j = 0; i < length; i++)
                        buffer[j++] = readOnlyList[i];
                    DoHybridEventForElementsCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        Fallback:
            {
                using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
                if (!enumerator.MoveNext())
                    return;
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                using PooledList<UIElement?> bufferList = new(pool);
                do
                {
                    bufferList.Add(enumerator.Current);
                } while (enumerator.MoveNext());
                (UIElement?[] buffer, int count) = bufferList;
                try
                {
                    DoHybridEventForElementsCore(in buffer[0], (nuint)count, in args, ref data, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        private static unsafe void DoHybridEventForElementsCore<TEventArgs>(ref readonly UIElement? elementArrayRef, nuint length,
            ref TEventArgs args, PointF focusPoint, delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            ArrayPool<Rect> pool = ArrayPool<Rect>.Shared;
            Rect[] buffer = pool.Rent(length);
            try
            {
                ref Rect bufferRef = ref buffer[0];
                fixed (Rect* ptr = buffer)
                {
                    BoundsHelper.CopyBoundsInElementsIntoBuffer(ptr, in elementArrayRef, length);
                    BoundsHelper.BulkContains(ptr, length, focusPoint);
                    Rect filter = AllBitSetsRect;
                    Rect* pFilter = &filter;
                    do
                    {
                        UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, --length);
                        if (element is null)
                            continue;
                        eventHandler(element, ref args, SequenceHelper.Equals(ptr + length, pFilter, UnsafeHelper.SizeOf<Rect>()));
                    } while (length > 0);
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        private static unsafe void DoHybridEventForElementsCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
            in TEventArgs args, ref TData data, PointF focusPoint, delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            ArrayPool<Rect> pool = ArrayPool<Rect>.Shared;
            Rect[] buffer = pool.Rent(length);
            try
            {
                ref Rect bufferRef = ref buffer[0];
                fixed (Rect* ptr = buffer)
                {
                    BoundsHelper.CopyBoundsInElementsIntoBuffer(ptr, in elementArrayRef, length);
                    BoundsHelper.BulkContains(ptr, length, focusPoint);
                    Rect filter = AllBitSetsRect;
                    Rect* pFilter = &filter;
                    do
                    {
                        UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, --length);
                        if (element is null)
                            continue;
                        eventHandler(element, in args, ref data, SequenceHelper.Equals(ptr + length, pFilter, UnsafeHelper.SizeOf<Rect>()));
                    } while (length > 0);
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
