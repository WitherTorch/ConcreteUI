#if NET472_OR_GREATER
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Internals
{
    partial class BoundsHelper
    {
        private static readonly Vector<int> _blendVector = CreateBlendVector();

        private static unsafe partial void VectorizedBulkContains(int* ptr, nuint length, PointF point)
        {
            Vector<float> filterVector = CreatePointVector(point.X, point.Y);
            Vector<int> blendVector = _blendVector;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<int>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector<int> sourceVector = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr);
                Vector<float> sourceVectorAsFloat = Vector.ConvertToSingle(sourceVector);
                Vector<int> resultVector = Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.GreaterThanOrEqual(sourceVectorAsFloat, filterVector),
                        right: Vector.LessThanOrEqual(sourceVectorAsFloat, filterVector)
                        );
                UnsafeHelper.WriteUnaligned(ptr, resultVector);
                if (length > (nuint)Vector<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector<int>>() - headRemainder) / UnsafeHelper.SizeOf<int>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else
                {
                    ptr += (nuint)Vector<float>.Count;
                    length -= (nuint)Vector<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop:
            do
            {
                Vector<int> sourceVector = UnsafeHelper.Read<Vector<int>>(ptr);
                Vector<float> sourceVectorAsFloat = Vector.ConvertToSingle(sourceVector);
                Vector<int> resultVector = Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.GreaterThanOrEqual(sourceVectorAsFloat, filterVector),
                        right: Vector.LessThanOrEqual(sourceVectorAsFloat, filterVector)
                        );
                UnsafeHelper.Write(ptr, resultVector);
                ptr += (nuint)Vector<int>.Count;
                length -= (nuint)Vector<int>.Count;
                continue;
            } while (length >= (nuint)Vector<int>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector<int>.Count;
                Vector<int> sourceVector = UnsafeHelper.ReadUnaligned<Vector<int>>(ptr);
                Vector<float> sourceVectorAsFloat = Vector.ConvertToSingle(sourceVector);
                Vector<int> resultVector = Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.GreaterThanOrEqual(sourceVectorAsFloat, filterVector),
                        right: Vector.LessThanOrEqual(sourceVectorAsFloat, filterVector)
                        );
                UnsafeHelper.WriteUnaligned(ptr, resultVector);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<float> CreatePointVector(float x, float y)
        {
            Vector<float> result = new Vector<float>(x);
            if (x == y)
                return result;
            DebugHelper.ThrowIf(Vector<float>.Count % 2 != 0);
            float* ptr = (float*)&result;
            for (int i = 1; i < Vector<float>.Count; i += 2)
                ptr[i] = y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<int> CreateBlendVector()
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