using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Extensions;
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

        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchHandleableEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), ref args, eventHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEvent<TEnumerable>(TEnumerable elements,
        delegate* managed<UIElement, void> eventHandler) where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), eventHandler);
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
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), ref args, eventHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, ref TEventArgs args, ref TData data,
        delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), ref args, ref data, eventHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs>(TEnumerable elements, ref TEventArgs args, PointF focusPoint,
        delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), ref args, focusPoint, eventHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEvent<TEnumerable, TEventArgs, TData>(TEnumerable elements, ref TEventArgs args, ref TData data, PointF focusPoint,
        delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void> eventHandler) where TEnumerable : IEnumerable<UIElement?> where TEventArgs : struct
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchEventCore(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), ref args, ref data, focusPoint, eventHandler);
    }

    private static unsafe void DispatchEventCore(ref readonly UIElement? elementArrayRef, nuint length,
        delegate* managed<UIElement, void> eventHandler)
    {
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

    private static unsafe void DispatchEventCore<TEventArgs>(ref readonly UIElement? elementArrayRef, nuint length,
        ref TEventArgs args, PointF focusPoint, delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEventArgs : struct
    {
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
