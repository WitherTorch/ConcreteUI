using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

#pragma warning disable CS8500

namespace ConcreteUI.Controls
{
    partial class UIElementHelper
    {
        private static readonly Rect AllBitSetsRect = new Rect(-1, -1, -1, -1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchHandleableEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
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
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    DispatchHandleableEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchHandleableEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchHandleableEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchHandleableEventCore(in buffer[0], (nuint)count, ref args, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args,
            delegate* managed<UIElement, in TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
            => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), (delegate* managed<UIElement, ref TEventArgs, void>)eventHandler);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args, Point focusPoint,
            delegate* managed<UIElement, in TEventArgs, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
            => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), focusPoint, (delegate* managed<UIElement, ref TEventArgs, bool, void>)eventHandler);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
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
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)count, ref args, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data,
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)count, in args, ref data, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args, PointF focusPoint,
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
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, ref args, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)count, ref args, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data, PointF focusPoint,
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)length, in args, ref data, focusPoint, eventHandler);
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
                    DispatchEventCore(in buffer[0], (nuint)count, in args, ref data, focusPoint, eventHandler);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        private static unsafe void DispatchEventCore<TEventArgs>(ref readonly UIElement? elementArrayRef, nuint length,
            ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            for (; length >= 4; length -= 4)
            {
                CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 4, ref args, eventHandler);
            }
            switch (length)
            {
                case 3:
                    CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, ref args, eventHandler);
                    break;
                case 2:
                    CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler);
                    break;
                case 1:
                    CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler);
                    break;
            }

            static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
                ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, void> eventHandler)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, i);
                if (element is null)
                    return;
                eventHandler(element, ref args);
            }
        }

        private static unsafe void DispatchEventCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
            in TEventArgs args, ref TData data, delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            for (; length >= 4; length -= 4)
            {
                CallEventHandler(in elementArrayRef, length - 1, in args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, in args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, in args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 4, in args, ref data, eventHandler);
            }
            switch (length)
            {
                case 3:
                    CallEventHandler(in elementArrayRef, length - 1, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, in args, ref data, eventHandler);
                    break;
                case 2:
                    CallEventHandler(in elementArrayRef, length - 1, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, in args, ref data, eventHandler);
                    break;
                case 1:
                    CallEventHandler(in elementArrayRef, length - 1, in args, ref data, eventHandler);
                    break;
            }

            static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
                in TEventArgs args, ref TData data, delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, i);
                if (element is null)
                    return;
                eventHandler(element, in args, ref data);
            }
        }

        private static unsafe void DispatchHandleableEventCore<TEventArgs>(ref readonly UIElement? elementArrayRef, nuint length,
            ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEventArgs : struct, IHandleableEventArgs
        {
            DebugHelper.ThrowIf(length < 1);
            for (; length >= 4; length -= 4)
            {
                if (CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler))
                    return;
                if (CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler))
                    return;
                if (CallEventHandler(in elementArrayRef, length - 3, ref args, eventHandler))
                    return;
                if (CallEventHandler(in elementArrayRef, length - 4, ref args, eventHandler))
                    return;
            }
            switch (length)
            {
                case 3:
                    if (CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler))
                        return;
                    if (CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler))
                        return;
                    CallEventHandler(in elementArrayRef, length - 3, ref args, eventHandler);
                    break;
                case 2:
                    if (CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler))
                        return;
                    CallEventHandler(in elementArrayRef, length - 2, ref args, eventHandler);
                    break;
                case 1:
                    CallEventHandler(in elementArrayRef, length - 1, ref args, eventHandler);
                    break;
            }

            static bool CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
                ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, void> eventHandler)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, i);
                if (element is null)
                    return false;
                eventHandler(element, ref args);
                return args.Handled;
            }
        }

        private static unsafe void DispatchEventCore<TEventArgs>(ref readonly UIElement? elementArrayRef, nuint length,
            ref TEventArgs args, PointF focusPoint, delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            NativeMemoryPool pool = NativeMemoryPool.Shared;
            TypedNativeMemoryBlock<Rect> buffer = pool.Rent<Rect>(length);
            Rect* ptr = buffer.NativePointer;
            try
            {
                BoundsHelper.CopyBoundsInElementsIntoBuffer(ptr, in elementArrayRef, length);
                BoundsHelper.BulkContains(ptr, length, focusPoint);
                Rect filter = AllBitSetsRect;
                Rect* pFilter = &filter;
                for (; length >= 4; length -= 4)
                {
                    CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, ptr, pFilter, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 4, ptr, pFilter, ref args, eventHandler);
                }
                switch (length)
                {
                    case 3:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, ref args, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, ref args, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 3, ptr, pFilter, ref args, eventHandler);
                        break;
                    case 2:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, ref args, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, ref args, eventHandler);
                        break;
                    case 1:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, ref args, eventHandler);
                        break;
                }
            }
            finally
            {
                pool.Return(buffer);
            }

            static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i, Rect* boundsBuffer, Rect* comparee,
                ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, i);
                if (element is null)
                    return;
                eventHandler(element, ref args, SequenceHelper.Equals(boundsBuffer + i, comparee, 1u));
            }
        }

        private static unsafe void DispatchEventCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
            in TEventArgs args, ref TData data, PointF focusPoint, delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler) where TEventArgs : struct
        {
            DebugHelper.ThrowIf(length < 1);
            NativeMemoryPool pool = NativeMemoryPool.Shared;
            TypedNativeMemoryBlock<Rect> buffer = pool.Rent<Rect>(length);
            Rect* ptr = buffer.NativePointer;
            try
            {
                BoundsHelper.CopyBoundsInElementsIntoBuffer(ptr, in elementArrayRef, length);
                BoundsHelper.BulkContains(ptr, length, focusPoint);
                Rect filter = AllBitSetsRect;
                Rect* pFilter = &filter;
                for (; length >= 4; length -= 4)
                {
                    CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, ptr, pFilter, in args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 4, ptr, pFilter, in args, ref data, eventHandler);
                }
                switch (length)
                {
                    case 3:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, in args, ref data, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, in args, ref data, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 3, ptr, pFilter, in args, ref data, eventHandler);
                        break;
                    case 2:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, in args, ref data, eventHandler);
                        CallEventHandler(in elementArrayRef, length - 2, ptr, pFilter, in args, ref data, eventHandler);
                        break;
                    case 1:
                        CallEventHandler(in elementArrayRef, length - 1, ptr, pFilter, in args, ref data, eventHandler);
                        break;
                }
            }
            finally
            {
                pool.Return(buffer);
            }

            static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i, Rect* boundsBuffer, Rect* comparee,
                in TEventArgs args, ref TData data, delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(in elementArrayRef, i);
                if (element is null)
                    return;
                eventHandler(element, in args, ref data, SequenceHelper.Equals(boundsBuffer + i, comparee, 1u));
            }

        }
    }
}
