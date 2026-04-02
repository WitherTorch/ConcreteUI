using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
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
            ScalarizedBulkContains(ptr, length, point.X, point.Y);
        }

        [Inline(InlineBehavior.Remove)]
        private static void VectorizedBulkContains(Rect* ptr, nuint length, PointF point)
            => VectorizedBulkContains((int*)ptr, length * 4, point);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static partial void VectorizedBulkContains(int* ptr, nuint length, PointF point);

        [Inline(InlineBehavior.Remove)]
        private static void ScalarizedBulkContains(Rect* ptr, nuint length, float x, float y)
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

            static void DoSingleOperation(Rect* ptr, float x, float y)
            {
                *ptr = new Rect(
                    left: -MathHelper.BooleanToInt32(ptr->Left <= x),
                    top: -MathHelper.BooleanToInt32(ptr->Top <= y),
                    right: -MathHelper.BooleanToInt32(ptr->Right >= x),
                    bottom: -MathHelper.BooleanToInt32(ptr->Bottom >= y)
                    );
            }
        }
    }
}
