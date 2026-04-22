#if NET472_OR_GREATER
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static readonly Vector<uint> CopySignMaskVector = new Vector<uint>(0x80000000);
        private static readonly Vector<float> RoundVector = new Vector<float>(0.49999997f);

        private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 pointsPerPixel)
        {
            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;
            Vector<float> multiplierVector = CreatePointVector(pointsPerPixelX, pointsPerPixelY);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector<float> sourceVector = UnsafeHelper.ReadUnaligned<Vector<float>>(ptr) * multiplierVector;
                UnsafeHelper.WriteUnaligned(ptr, Round(sourceVector));
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
                UnsafeHelper.Write(ptr, Round(sourceVector));
                ptr += (nuint)Vector<float>.Count;
                length -= (nuint)Vector<float>.Count;
                continue;
            } while (length >= (nuint)Vector<float>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                RectF* ptr2 = (RectF*)ptr;
                nuint length2 = length / 4;
                ScalarizedScaleRects(ref ptr2, ref length2, pointsPerPixel);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector<float> CopySign(Vector<float> x, Vector<float> y) // MathF.CopySign 的向量化版本
            {
                Vector<uint> mask = CopySignMaskVector;

                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector<uint> xbits = Vector.AsVectorUInt32(x);
                Vector<uint> ybits = Vector.AsVectorUInt32(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                xbits = Vector.ConditionalSelect(mask, ybits, xbits);
                return Vector.AsVectorSingle(xbits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector<int> Round(Vector<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Vector.ConvertToInt32(value + CopySign(RoundVector, value));
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