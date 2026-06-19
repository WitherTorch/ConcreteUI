using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Element;

using InlineIL;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Internals;

internal static unsafe partial class BoundsHelper
{
    public static void CopyFromElements(Rect* buffer, ref readonly UIElement? elementRef, nuint length)
    {
        nuint offset = 0;
        for (; length >= 4; length -= 4, buffer += 4, offset += 4)
        {
            DoSingleOperation(buffer, in elementRef, offset);
            DoSingleOperation(buffer + 1, in elementRef, offset + 1);
            DoSingleOperation(buffer + 2, in elementRef, offset + 2);
            DoSingleOperation(buffer + 3, in elementRef, offset + 3);
        }
        Rect* bufferEnd = buffer + length;
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer++, in elementRef, offset++);
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer++, in elementRef, offset++);
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer, in elementRef, offset);

        static void DoSingleOperation(Rect* ptr, ref readonly UIElement? elementRef, nuint offset)
        {
            UIElement? element = UnsafeHelper.AddTypedOffset(in elementRef, offset);
            *ptr = element is null ? Rect.Empty : FastGetBounds(element);
        }
    }

    public static void HitTest(Rect* ptr, nuint length, PointF point)
    {
        int x = MathI.Truncate(point.X);
        int y = MathI.Truncate(point.Y);
        if (UnsafeHelper.SizeOf<Rect>() < Limits.GetLimitForVectorizing<int>())
        {
            nuint lengthOfInt32 = length * 4;
            if (lengthOfInt32 > Limits.GetLimitForVectorizing<int>())
            {
                VectorizedHitTest((int*)ptr, lengthOfInt32, x, y);
                return;
            }
        }
        ScalarizedHitTest(ptr, length, x, y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static partial void VectorizedHitTest(int* ptr, nuint length, int x, int y);

    [Inline(InlineBehavior.Remove)]
    private static void ScalarizedHitTest(Rect* ptr, nuint length, int x, int y)
    {
        for (; length >= 4; length -= 4, ptr += 4)
        {
            DoSingleOperation(ptr, x, y);
            DoSingleOperation(ptr + 1, x, y);
            DoSingleOperation(ptr + 2, x, y);
            DoSingleOperation(ptr + 3, x, y);
        }
        Rect* ptrEnd = ptr + length;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);

        [Inline(InlineBehavior.Remove)]
        static void DoSingleOperation(Rect* ptr, int x, int y)
        {
            bool val = ((ptr->Left <= x) & (ptr->Top <= y)) & ((ptr->Right >= x) & (ptr->Bottom >= y));
            StoreBooleanInRect(ptr, val);
        }
    }

    [Inline(InlineBehavior.Remove)]
    private static void StoreBooleanInRect(void* destination, bool value)
    {
#if DEBUG
        UnsafeHelper.InitBlockUnaligned(destination, 0, UnsafeHelper.SizeOf<Rect>());
#endif
        UnsafeHelper.WriteUnaligned(destination, value);
    }

    [Inline(InlineBehavior.Remove)]
    private static Rect FastGetBounds(UIElement element)
    {
        IL.Push(element);
        IL.Emit.Call(MethodRef.PropertyGet(typeof(UIElement), nameof(UIElement.Bounds)));
        IL.Emit.Call(MethodRef.Operator(typeof(Rect), ConversionOperator.Implicit, ConversionDirection.From, typeof(Rectangle)));
        return IL.Return<Rect>();
    }

    [Inline(InlineBehavior.Remove)]
    public static ulong ConvertPointToUInt64(Point value) => (ulong)(uint)value.X << 32 | (uint)value.Y;

    [Inline(InlineBehavior.Remove)]
    public static ulong ConvertSizeToUInt64(Size value) => (ulong)(uint)value.Width << 32 | (uint)value.Height;

    [Inline(InlineBehavior.Remove)]
    public static Point ConvertUInt64ToPoint(ulong value) => new Point((int)(uint)(value >>> 32), (int)(uint)value);

    [Inline(InlineBehavior.Remove)]
    public static Size ConvertUInt64ToSize(ulong value) => new Size((int)(uint)(value >>> 32), (int)(uint)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle ConvertUInt64SlotsToBounds(ulong location, ulong size)
        => new Rectangle(
            location: ConvertUInt64ToPoint(location),
            size: ConvertUInt64ToSize(size)
            );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertBoundsToUInt64Slots(in Rectangle bounds, ref ulong locationSlot, ref ulong sizeSlot)
    {
        locationSlot = ConvertPointToUInt64(bounds.Location);
        sizeSlot = ConvertSizeToUInt64(bounds.Size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle LoadBoundsFromUInt64Fields(ref readonly ulong locationSlot, ref readonly ulong sizeSlot) => new Rectangle(
            location: ConvertUInt64ToPoint(Volatile.Read(ref UnsafeHelper.AsRefIn(in locationSlot))),
            size: ConvertUInt64ToSize(Volatile.Read(ref UnsafeHelper.AsRefIn(in sizeSlot)))
            );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SaveBoundsToUInt64Fields(in Rectangle bounds, ref ulong locationSlot, ref ulong sizeSlot)
    {
        Volatile.Write(ref locationSlot, ConvertPointToUInt64(bounds.Location));
        Volatile.Write(ref sizeSlot, ConvertSizeToUInt64(bounds.Size));
    }
}
