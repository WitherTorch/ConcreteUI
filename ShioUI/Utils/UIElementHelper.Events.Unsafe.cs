using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchHandleableEventUnsafe<TEventArgs>(UIElement?[] elements, int count, ref TEventArgs args,
        delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEventArgs : struct, IHandleableEventArgs
    {
        if (args.Handled)
            return;

        DispatchHandleableEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), ref args, eventHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEventUnsafe(UIElement?[] elements, int count,
        delegate* managed<UIElement, void> eventHandler)
        => DispatchEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEventUnsafe<TEventArgs>(UIElement?[] elements, int count, in TEventArgs args,
        delegate* managed<UIElement, in TEventArgs, void> eventHandler) where TEventArgs : struct
        => DispatchEventUnsafe(elements, count, ref UnsafeHelper.AsRefIn(in args), (delegate* managed<UIElement, ref TEventArgs, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEventUnsafe<TEventArgs>(UIElement?[] elements, int count, in TEventArgs args, PointF focusPoint,
        delegate* managed<UIElement, in TEventArgs, bool, void> eventHandler) where TEventArgs : struct
        => DispatchEventUnsafe(elements, count, ref UnsafeHelper.AsRefIn(in args), focusPoint, (delegate* managed<UIElement, ref TEventArgs, bool, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEventUnsafe<TEventArgs, TData>(UIElement?[] elements, int count, in TEventArgs args, ref TData data,
        delegate* managed<UIElement, in TEventArgs, ref TData, void> eventHandler) where TEventArgs : struct
        => DispatchEventUnsafe(elements, count, ref UnsafeHelper.AsRefIn(in args), ref data, (delegate* managed<UIElement, ref TEventArgs, ref TData, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchReadOnlyEventUnsafe<TEventArgs, TData>(UIElement?[] elements, int count, in TEventArgs args, ref TData data, PointF focusPoint,
        delegate* managed<UIElement, in TEventArgs, ref TData, bool, void> eventHandler) where TEventArgs : struct
        => DispatchEventUnsafe(elements, count, ref UnsafeHelper.AsRefIn(in args), ref data, focusPoint, (delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void>)eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEventUnsafe<TEventArgs>(UIElement?[] elements, int count, ref TEventArgs args,
        delegate* managed<UIElement, ref TEventArgs, void> eventHandler) where TEventArgs : struct 
        => DispatchEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), ref args, eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEventUnsafe<TEventArgs, TData>(UIElement?[] elements, int count, ref TEventArgs args, ref TData data,
        delegate* managed<UIElement, ref TEventArgs, ref TData, void> eventHandler) where TEventArgs : struct 
        => DispatchEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), ref args, ref data, eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEventUnsafe<TEventArgs>(UIElement?[] elements, int count, ref TEventArgs args, PointF focusPoint,
        delegate* managed<UIElement, ref TEventArgs, bool, void> eventHandler) where TEventArgs : struct 
        => DispatchEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), ref args, focusPoint, eventHandler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DispatchEventUnsafe<TEventArgs, TData>(UIElement?[] elements, int count, ref TEventArgs args, ref TData data, PointF focusPoint,
        delegate* managed<UIElement, ref TEventArgs, ref TData, bool, void> eventHandler) where TEventArgs : struct 
        => DispatchEventCore(in UnsafeHelper.GetArrayDataReference(elements), MathHelper.MakeUnsigned(count), ref args, ref data, focusPoint, eventHandler);
}
