using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Internals
{
    internal static unsafe partial class BoundsHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBoundsInElementsIntoBuffer(Rect* buffer, ref readonly UIElement? elementRef, nuint length)
        {
            nuint offset = 0;
            for (; length >= 4; length -= 4, buffer += 4, offset += 4)
            {
                DoSingleOperation(buffer, in elementRef, offset);
                DoSingleOperation(buffer + 1, in elementRef, offset);
                DoSingleOperation(buffer + 2, in elementRef, offset);
                DoSingleOperation(buffer + 3, in elementRef, offset);
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
                *ptr = element is null ? Rect.Empty : element.Bounds;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBoundsInElementsIntoBuffer_List<TList>(Rect* buffer, TList list, int length) where TList : IList<UIElement>
        {
            int offset = 0;
            for (; length >= 4; length -= 4, buffer += 4, offset += 4)
            {
                DoSingleOperation(buffer, list, offset);
                DoSingleOperation(buffer + 1, list, offset);
                DoSingleOperation(buffer + 2, list, offset);
                DoSingleOperation(buffer + 3, list, offset);
            }
            Rect* bufferEnd = buffer + length;
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer++, list, offset++);
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer++, list, offset++);
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer, list, offset);

            static void DoSingleOperation(Rect* ptr, TList list, int offset)
            {
                UIElement? element = list[offset];
                *ptr = element is null ? Rect.Empty : element.Bounds;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBoundsInElementsIntoBuffer_RList<TList>(Rect* buffer, TList list, int length) where TList : IReadOnlyList<UIElement>
        {
            int offset = 0;
            for (; length >= 4; length -= 4, buffer += 4, offset += 4)
            {
                DoSingleOperation(buffer, list, offset);
                DoSingleOperation(buffer + 1, list, offset);
                DoSingleOperation(buffer + 2, list, offset);
                DoSingleOperation(buffer + 3, list, offset);
            }
            Rect* bufferEnd = buffer + length;
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer++, list, offset++);
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer++, list, offset++);
            if (buffer >= bufferEnd)
                return;
            DoSingleOperation(buffer, list, offset);

            static void DoSingleOperation(Rect* ptr, TList list, int offset)
            {
                UIElement? element = list[offset];
                *ptr = element is null ? Rect.Empty : element.Bounds;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BulkContains(Rect* ptr, nuint length, PointF point)
        {
            if (length * 4 > Limits.GetLimitForVectorizing<int>())
            {
                VectorizedBulkContains(ptr, length, point);
                return;
            }
            ScalarizedBulkContains(ptr, length, point);
        }

        [Inline(InlineBehavior.Remove)]
        private static void VectorizedBulkContains(Rect* ptr, nuint length, PointF point)
            => VectorizedBulkContains((int*)ptr, length * 4, point);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static partial void VectorizedBulkContains(int* ptr, nuint length, PointF point);

        [Inline(InlineBehavior.Remove)]
        private static void ScalarizedBulkContains(Rect* ptr, nuint length, PointF point)
        {
            for (; length >= 4; length -= 4, ptr += 4)
            {
                DoSingleOperation(ptr, point);
                DoSingleOperation(ptr + 1, point);
                DoSingleOperation(ptr + 2, point);
                DoSingleOperation(ptr + 3, point);
            }
            Rect* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return;
            DoSingleOperation(ptr, point);
            ptr++;
            if (ptr >= ptrEnd)
                return;
            DoSingleOperation(ptr, point);
            ptr++;
            if (ptr >= ptrEnd)
                return;
            DoSingleOperation(ptr, point);

            static void DoSingleOperation(Rect* ptr, PointF point)
            {
                *ptr = new Rect(
                    left: -MathHelper.BooleanToInt32(ptr->Left <= point.X),
                    top: -MathHelper.BooleanToInt32(ptr->Top <= point.Y),
                    right: -MathHelper.BooleanToInt32(ptr->Right >= point.X),
                    bottom: -MathHelper.BooleanToInt32(ptr->Bottom >= point.Y)
                    );
            }
        }
    }
}
