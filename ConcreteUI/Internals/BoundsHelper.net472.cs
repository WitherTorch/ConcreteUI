#if NET472_OR_GREATER
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Internals
{
    unsafe partial class BoundsHelper
    {
        private static readonly Vector<int> _blendVector = CreateBlendVector();

        private static partial void VectorizedBulkAABBHitTest(int* ptr, nuint length, int x, int y)
        {
            Vector<int> filterVector = CreatePointVector(x, y);
            Vector<int> blendVector = _blendVector;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<int>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else if (headRemainder % UnsafeHelper.SizeOf<Rect>() != 0)
                goto VectorizedLoop_Unaligned;
            else
            {
                if (length > (nuint)Vector<int>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector<int>>() - headRemainder) / UnsafeHelper.SizeOf<int>(); // 取得數量
                    DebugHelper.ThrowIf(headRemainder % 4 != 0);
                    ScalarizedBulkAABBHitTest((Rect*)ptr, headRemainder / 4, x, y);
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector<int>.Count * 2)
                {
                    int* ptr2 = ptr + Vector<int>.Count;
                    Vector<int> sourceVector = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr);
                    Vector<int> sourceVector2 = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr2);
                    Vector<int> resultVector = Vector.ConditionalSelect(
                            condition: blendVector,
                            left: Vector.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    Vector<int> resultVector2 = Vector.ConditionalSelect(
                            condition: blendVector,
                            left: Vector.LessThanOrEqual(sourceVector2, filterVector),
                            right: Vector.GreaterThanOrEqual(sourceVector2, filterVector)
                            );
                    UnsafeHelper.WriteUnaligned(ptr, resultVector);
                    UnsafeHelper.WriteUnaligned(ptr2, resultVector2);
                    return;
                }
                else
                {
                    Vector<int> sourceVector = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr);
                    Vector<int> resultVector = Vector.ConditionalSelect(
                            condition: blendVector,
                            left: Vector.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    UnsafeHelper.WriteUnaligned(ptr, resultVector);
                    ptr += (nuint)Vector<int>.Count;
                    length -= (nuint)Vector<int>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop_Unaligned:
            do
            {
                Vector<int> sourceVector = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr);
                Vector<int> resultVector = Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                UnsafeHelper.WriteUnaligned(ptr, resultVector);
                ptr += (nuint)Vector<int>.Count;
                length -= (nuint)Vector<int>.Count;
                continue;
            } while (length >= (nuint)Vector<int>.Count);
            goto TailProcess;

        VectorizedLoop:
            do
            {
                Vector<int> sourceVector = UnsafeHelper.Read<Vector<int>>(ptr);
                Vector<int> resultVector = Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                UnsafeHelper.Write(ptr, resultVector);
                ptr += (nuint)Vector<int>.Count;
                length -= (nuint)Vector<int>.Count;
                continue;
            } while (length >= (nuint)Vector<int>.Count);
            goto TailProcess;

        TailProcess:
            DebugHelper.ThrowIf(length % 4 != 0);
            ScalarizedBulkAABBHitTest((Rect*)ptr, length / 4, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<int> CreatePointVector(int x, int y)
        {
            Vector<int> result = new Vector<int>(x);
            if (x == y)
                return result;
            DebugHelper.ThrowIf(Vector<int>.Count % 2 != 0);
            int* ptr = (int*)&result;
            for (int i = 1; i < Vector<int>.Count; i += 2)
                ptr[i] = y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<int> CreateBlendVector()
        {
            Vector<int> result = Vector<int>.Zero;
            int* ptr = (int*)&result;
            for (int i = 0; i < Vector<int>.Count; i += 4)
            {
                ptr[i] = -1;
                ptr[i + 1] = -1;
            }
            return result;
        }
    }
}
#endif