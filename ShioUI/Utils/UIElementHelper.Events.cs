using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioUI.Utils;

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
}
