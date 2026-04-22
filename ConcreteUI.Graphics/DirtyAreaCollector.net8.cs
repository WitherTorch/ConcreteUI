#if NET8_0_OR_GREATER
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static readonly Vector512<uint> CopySignMaskVector_512 = Vector512.Create(0x80000000u);
        private static readonly Vector256<uint> CopySignMaskVector_256 = Vector256.Create(0x80000000u);
        private static readonly Vector128<uint> CopySignMaskVector_128 = Vector128.Create(0x80000000u);
        private static readonly Vector512<float> RoundVector_512 = Vector512.Create(0.49999997f);
        private static readonly Vector256<float> RoundVector_256 = Vector256.Create(0.49999997f);
        private static readonly Vector128<float> RoundVector_128 = Vector128.Create(0.49999997f);
        private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 pointsPerPixel)
        {
            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;
            if (Limits.UseVector512() && length >= (nuint)Vector512<float>.Count)
                VectorizedScaleRects_512(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);
            else if (Limits.UseVector256() && length >= (nuint)Vector256<float>.Count)
                VectorizedScaleRects_256(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);
            else
                VectorizedScaleRects_128(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);

            if (length > 0)
            {
                RectF* ptr2 = (RectF*)ptr;
                nuint length2 = length / 4;
                ScalarizedScaleRects(ref ptr2, ref length2, pointsPerPixel);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_512(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector512<float> multiplierVector = CreatePointVector_512(pointsPerPixelX, pointsPerPixelY);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector512<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                if (length > (nuint)Vector512<float>.Count * 2)
                {
                    Vector512<float> sourceVector = Vector512.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    headRemainder = (UnsafeHelper.SizeOf<Vector512<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector512<float>.Count * 2)
                {
                    float* ptr2 = ptr + (nuint)Vector512<float>.Count;
                    Vector512<float> sourceVector = Vector512.Load(ptr) * multiplierVector;
                    Vector512<float> sourceVector2 = Vector512.Load(ptr2) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);
                    Round(sourceVector2).Store((int*)ptr2);
                    length = 0;
                    return;
                }
                else
                {
                    Vector512<float> sourceVector = Vector512.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    ptr += (nuint)Vector512<float>.Count;
                    length -= (nuint)Vector512<float>.Count;
                    return;
                }
            }

        VectorizedLoop:
            do
            {
                Vector512<float> sourceVector = Vector512.LoadAligned(ptr) * multiplierVector;
                Round(sourceVector).StoreAligned((int*)ptr);
                ptr += (nuint)Vector512<float>.Count;
                length -= (nuint)Vector512<float>.Count;
                continue;
            } while (length >= (nuint)Vector512<float>.Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector512<float> CopySign(Vector512<float> x, Vector512<float> y) // MathF.CopySign 的向量化版本
            {
                Vector512<uint> mask = CopySignMaskVector_512;

                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector512<uint> xbits = Vector512.As<float, uint>(x);
                Vector512<uint> ybits = Vector512.As<float, uint>(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                xbits = Vector512.ConditionalSelect(mask, ybits, xbits);
                return Vector512.As<uint, float>(xbits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector512<int> Round(Vector512<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Vector512.ConvertToInt32(value + CopySign(RoundVector_512, value));
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_256(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector256<float> multiplierVector = CreatePointVector_256(pointsPerPixelX, pointsPerPixelY);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector256<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                if (length > (nuint)Vector256<float>.Count * 2)
                {
                    Vector256<float> sourceVector = Vector256.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    headRemainder = (UnsafeHelper.SizeOf<Vector256<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector256<float>.Count * 2)
                {
                    float* ptr2 = ptr + (nuint)Vector256<float>.Count;
                    Vector256<float> sourceVector = Vector256.Load(ptr) * multiplierVector;
                    Vector256<float> sourceVector2 = Vector256.Load(ptr2) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);
                    Round(sourceVector2).Store((int*)ptr2);
                    length = 0;
                    return;
                }
                else
                {
                    Vector256<float> sourceVector = Vector256.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    ptr += (nuint)Vector256<float>.Count;
                    length -= (nuint)Vector256<float>.Count;
                    return;
                }
            }

        VectorizedLoop:
            do
            {
                Vector256<float> sourceVector = Vector256.LoadAligned(ptr) * multiplierVector;
                Round(sourceVector).StoreAligned((int*)ptr);
                ptr += (nuint)Vector256<float>.Count;
                length -= (nuint)Vector256<float>.Count;
                continue;
            } while (length >= (nuint)Vector256<float>.Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector256<float> CopySign(Vector256<float> x, Vector256<float> y) // MathF.CopySign 的向量化版本
            {
                Vector256<uint> mask = CopySignMaskVector_256;

                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector256<uint> xbits = Vector256.As<float, uint>(x);
                Vector256<uint> ybits = Vector256.As<float, uint>(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                xbits = Vector256.ConditionalSelect(mask, ybits, xbits);
                return Vector256.As<uint, float>(xbits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector256<int> Round(Vector256<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Vector256.ConvertToInt32(value + CopySign(RoundVector_256, value));
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_128(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector128<float> multiplierVector = CreatePointVector_128(pointsPerPixelX, pointsPerPixelY);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector128<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                if (length > (nuint)Vector128<float>.Count * 2)
                {
                    Vector128<float> sourceVector = Vector128.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    headRemainder = (UnsafeHelper.SizeOf<Vector128<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector128<float>.Count * 2)
                {
                    float* ptr2 = ptr + (nuint)Vector128<float>.Count;
                    Vector128<float> sourceVector = Vector128.Load(ptr) * multiplierVector;
                    Vector128<float> sourceVector2 = Vector128.Load(ptr2) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);
                    Round(sourceVector2).Store((int*)ptr2);
                    length = 0;
                    return;
                }
                else
                {
                    Vector128<float> sourceVector = Vector128.Load(ptr) * multiplierVector;
                    Round(sourceVector).Store((int*)ptr);

                    ptr += (nuint)Vector128<float>.Count;
                    length -= (nuint)Vector128<float>.Count;
                    return;
                }
            }

        VectorizedLoop:
            do
            {
                Vector128<float> sourceVector = Vector128.LoadAligned(ptr) * multiplierVector;
                Round(sourceVector).StoreAligned((int*)ptr);
                ptr += (nuint)Vector128<float>.Count;
                length -= (nuint)Vector128<float>.Count;
                continue;
            } while (length >= (nuint)Vector128<float>.Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector128<float> CopySign(Vector128<float> x, Vector128<float> y) // MathF.CopySign 的向量化版本
            {
                Vector128<uint> mask = CopySignMaskVector_128;

                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector128<uint> xbits = Vector128.As<float, uint>(x);
                Vector128<uint> ybits = Vector128.As<float, uint>(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                xbits = Vector128.ConditionalSelect(mask, ybits, xbits);
                return Vector128.As<uint, float>(xbits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector128<int> Round(Vector128<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Vector128.ConvertToInt32(value + CopySign(RoundVector_128, value));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector512<float> CreatePointVector_512(float x, float y)
        {
            if (x == y)
                return Vector512.Create(x);
            else
                return Vector512.Create(x, y, x, y, x, y, x, y, x, y, x, y, x, y, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<float> CreatePointVector_256(float x, float y)
        {
            if (x == y)
                return Vector256.Create(x);
            else
                return Vector256.Create(x, y, x, y, x, y, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<float> CreatePointVector_128(float x, float y)
        {
            if (x == y)
                return Vector128.Create(x);
            else
                return Vector128.Create(x, y, x, y);
        }
    }
}
#endif