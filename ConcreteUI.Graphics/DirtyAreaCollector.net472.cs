#if NET472_OR_GREATER
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static readonly Vector<int> _blendVector = CreateBlendVector();

        private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 pointsPerPixel)
        {
            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;
            Vector<float> multiplierVector = CreatePointVector(pointsPerPixelX, pointsPerPixelY);
            Vector<int> blendVector = _blendVector;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector<float> sourceVector = UnsafeHelper.ReadUnaligned<Vector<float>>(ptr) * multiplierVector;
                Vector<int> resultVector = Vector.ConvertToInt32(sourceVector);
                Vector<float> resultVectorAsFloat = Vector.ConvertToSingle(resultVector);
                resultVector += Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.LessThan(sourceVector, resultVectorAsFloat),
                        right: -Vector.GreaterThan(sourceVector, resultVectorAsFloat)
                        );
                UnsafeHelper.WriteUnaligned(ptr, resultVector);
                if (length > (nuint)Vector<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
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
                Vector<float> sourceVector = UnsafeHelper.Read<Vector<float>>(ptr) * multiplierVector;
                Vector<int> resultVector = Vector.ConvertToInt32(sourceVector);
                Vector<float> resultVectorAsFloat = Vector.ConvertToSingle(resultVector);
                resultVector += Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.LessThan(sourceVector, resultVectorAsFloat),
                        right: -Vector.GreaterThan(sourceVector, resultVectorAsFloat)
                        );
                UnsafeHelper.Write(ptr, resultVector);
                ptr += (nuint)Vector<float>.Count;
                length -= (nuint)Vector<float>.Count;
                continue;
            } while (length >= (nuint)Vector<float>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector<float>.Count;
                Vector<float> sourceVector = UnsafeHelper.ReadUnaligned<Vector<float>>(ptr) * multiplierVector;
                Vector<int> resultVector = Vector.ConvertToInt32(sourceVector);
                Vector<float> resultVectorAsFloat = Vector.ConvertToSingle(resultVector);
                resultVector += Vector.ConditionalSelect(
                        condition: blendVector,
                        left: Vector.LessThan(sourceVector, resultVectorAsFloat),
                        right: -Vector.GreaterThan(sourceVector, resultVectorAsFloat)
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