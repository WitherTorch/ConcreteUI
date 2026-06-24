using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

#pragma warning disable CS8500

namespace ConcreteUI.Utils;

partial class UIElementHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchHandleableEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
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
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, eventHandler);
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
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)count, ref args, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchHandleableEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, ref TEventArgs args, ref TData data,
        delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct, IHandleableEventArgs
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
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, eventHandler);
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
                DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)count, ref args, ref data, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable>(TEnumerable elements,
        delegate* managed<UIElement, void> eventHandler) where TEnumerable : IEnumerable<UIElement?>
    {
        UIElement?[] array;
        int length;

        if (typeof(TEnumerable) == typeof(UIElement?[]))
            goto Array;
        if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
            goto UnwrappableList;
        if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
            goto ObservableList;
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Fallback:
        using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
        if (enumerator.MoveNext())
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            using PooledList<UIElement?> bufferList = new(pool);
            do
            {
                bufferList.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            (UIElement?[] buffer, length) = bufferList;
            try
            {
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args,
        delegate* managed<UIElement, in TEventArgs, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), (delegate* managed<UIElement, ref TEventArgs, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs>(TEnumerable elements, in TEventArgs args, PointF focusPoint,
        delegate* managed<UIElement, in TEventArgs, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), focusPoint, (delegate* managed<UIElement, ref TEventArgs, bool, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data,
        delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), ref data, (delegate* managed<UIElement, ref TEventArgs, ref TData, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, in TEventArgs args, ref TData data, PointF focusPoint,
        delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
        => DispatchEvent(elements, ref UnsafeHelper.AsRefIn(in args), ref data, focusPoint, (delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void>)eventHandler);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args,
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
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Fallback:
        using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
        if (enumerator.MoveNext())
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            using PooledList<UIElement?> bufferList = new(pool);
            do
            {
                bufferList.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            (UIElement?[] buffer, length) = bufferList;
            try
            {
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, ref TEventArgs args, ref TData data,
        delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
    {
        UIElement?[] array;
        int length;

        if (typeof(TEnumerable) == typeof(UIElement?[]))
            goto Array;
        if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
            goto UnwrappableList;
        if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
            goto ObservableList;
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, eventHandler);
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
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)count, ref args, ref data, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args, PointF focusPoint,
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
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, focusPoint, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, focusPoint, eventHandler);
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
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)count, ref args, focusPoint, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, ref TEventArgs args, ref TData data, PointF focusPoint,
        delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
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
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, focusPoint, eventHandler);
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
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ref args, ref data, focusPoint, eventHandler);
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
            ref UIElement? bufferRef = ref UnsafeHelper.GetArrayDataReference(buffer);
            try
            {
                for (int i = 0; i < length; i++)
                    UnsafeHelper.AddTypedOffset(ref bufferRef, i) = readOnlyList[i];
                DispatchEventCore(in bufferRef, (nuint)length, ref args, ref data, focusPoint, eventHandler);
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
                DispatchEventCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)count, ref args, ref data, focusPoint, eventHandler);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    private static unsafe void DispatchEventCore(ref readonly UIElement? elementArrayRef, nuint length,
        delegate* managed<UIElement, void> eventHandler)
    {
        DebugHelper.ThrowIf(length < 1);
        for (; length >= 4; length -= 4)
        {
            CallEventHandler(in elementArrayRef, length - 1, eventHandler);
            CallEventHandler(in elementArrayRef, length - 2, eventHandler);
            CallEventHandler(in elementArrayRef, length - 3, eventHandler);
            CallEventHandler(in elementArrayRef, length - 4, eventHandler);
        }
        switch (length)
        {
            case 3:
                CallEventHandler(in elementArrayRef, length - 1, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, eventHandler);
                break;
            case 2:
                CallEventHandler(in elementArrayRef, length - 1, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, eventHandler);
                break;
            case 1:
                CallEventHandler(in elementArrayRef, length - 1, eventHandler);
                break;
        }

        static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
            delegate* managed<UIElement, void> eventHandler)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            eventHandler(element);
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
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            eventHandler(element, ref args);
        }
    }

    private static unsafe void DispatchEventCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
        ref TEventArgs args, ref TData data, delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEventArgs : struct
    {
        DebugHelper.ThrowIf(length < 1);
        for (; length >= 4; length -= 4)
        {
            CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler);
            CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler);
            CallEventHandler(in elementArrayRef, length - 3, ref args, ref data, eventHandler);
            CallEventHandler(in elementArrayRef, length - 4, ref args, ref data, eventHandler);
        }
        switch (length)
        {
            case 3:
                CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, ref args, ref data, eventHandler);
                break;
            case 2:
                CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler);
                break;
            case 1:
                CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler);
                break;
        }

        static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
            ref TEventArgs args, ref TData data, delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            eventHandler(element, ref args, ref data);
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
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return false;
            eventHandler(element, ref args);
            return args.Handled;
        }
    }

    private static unsafe void DispatchHandleableEventCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
        ref TEventArgs args, ref TData data, delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEventArgs : struct, IHandleableEventArgs
    {
        DebugHelper.ThrowIf(length < 1);
        for (; length >= 4; length -= 4)
        {
            if (CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler))
                return;
            if (CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler))
                return;
            if (CallEventHandler(in elementArrayRef, length - 3, ref args, ref data, eventHandler))
                return;
            if (CallEventHandler(in elementArrayRef, length - 4, ref args, ref data, eventHandler))
                return;
        }
        switch (length)
        {
            case 3:
                if (CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler))
                    return;
                if (CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler))
                    return;
                CallEventHandler(in elementArrayRef, length - 3, ref args, ref data, eventHandler);
                break;
            case 2:
                if (CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler))
                    return;
                CallEventHandler(in elementArrayRef, length - 2, ref args, ref data, eventHandler);
                break;
            case 1:
                CallEventHandler(in elementArrayRef, length - 1, ref args, ref data, eventHandler);
                break;
        }

        static bool CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i,
            ref TEventArgs args, ref TData data, delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return false;
            eventHandler(element, ref args, ref data);
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
            BoundsHelper.CopyFromElements(ptr, in elementArrayRef, length);
            BoundsHelper.HitTest(ptr, length, focusPoint);
            for (; length >= 4; length -= 4)
            {
                CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, ptr, ref args, eventHandler);
                CallEventHandler(in elementArrayRef, length - 4, ptr, ref args, eventHandler);
            }
            switch (length)
            {
                case 3:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, ptr, ref args, eventHandler);
                    break;
                case 2:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, eventHandler);
                    break;
                case 1:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, eventHandler);
                    break;
            }
        }
        finally
        {
            pool.Return(buffer);
        }

        static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i, Rect* boundsBuffer,
            ref TEventArgs args, delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            eventHandler(element, ref args, UnsafeHelper.ReadUnaligned<bool>(boundsBuffer + i));
        }
    }

    private static unsafe void DispatchEventCore<TEventArgs, TData>(ref readonly UIElement? elementArrayRef, nuint length,
        ref TEventArgs args, ref TData data, PointF focusPoint, delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void> eventHandler) where TEventArgs : struct
    {
        DebugHelper.ThrowIf(length < 1);
        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<Rect> buffer = pool.Rent<Rect>(length);
        Rect* ptr = buffer.NativePointer;
        try
        {
            BoundsHelper.CopyFromElements(ptr, in elementArrayRef, length);
            BoundsHelper.HitTest(ptr, length, focusPoint);
            for (; length >= 4; length -= 4)
            {
                CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 3, ptr, ref args, ref data, eventHandler);
                CallEventHandler(in elementArrayRef, length - 4, ptr, ref args, ref data, eventHandler);
            }
            switch (length)
            {
                case 3:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 3, ptr, ref args, ref data, eventHandler);
                    break;
                case 2:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, ref data, eventHandler);
                    CallEventHandler(in elementArrayRef, length - 2, ptr, ref args, ref data, eventHandler);
                    break;
                case 1:
                    CallEventHandler(in elementArrayRef, length - 1, ptr, ref args, ref data, eventHandler);
                    break;
            }
        }
        finally
        {
            pool.Return(buffer);
        }

        static void CallEventHandler(ref readonly UIElement? elementArrayRef, nuint i, Rect* boundsBuffer,
            ref TEventArgs args, ref TData data, delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void> eventHandler)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            eventHandler(element, ref args, ref data, UnsafeHelper.ReadUnaligned<bool>(boundsBuffer + i));
        }

    }
}
