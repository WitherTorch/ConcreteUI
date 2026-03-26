#if NET8_0_OR_GREATER
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static readonly Vector512<int> _blendVector_512 = CreateBlendVector_512();
        private static readonly Vector256<int> _blendVector_256 = CreateBlendVector_256();
        private static readonly Vector128<int> _blendVector_128 = CreateBlendVector_128();

        private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 pointsPerPixel)
        {
            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;
            if (Limits.UseVector512() && length >= (nuint)Vector512<float>.Count)
                VectorizedScaleRects_512(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);
            else if (Limits.UseVector256() && length >= (nuint)Vector256<float>.Count)
                VectorizedScaleRects_256(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);
            else
                VectorizedScaleRects_128(ref ptr, ref length, pointsPerPixelX, pointsPerPixelY);
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_512(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector512<float> multiplierVector = CreatePointVector_512(pointsPerPixelX, pointsPerPixelY);
            Vector512<int> blendVector = _blendVector_512;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector512<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector512<float> sourceVector = Vector512.Load(ptr) * multiplierVector;
                Vector512<int> resultVector = Vector512.ConvertToInt32(sourceVector);
                Vector512<float> resultVectorAsFloat = Vector512.ConvertToSingle(resultVector);
                resultVector += Vector512.ConditionalSelect(
                        condition: blendVector,
                        left: Vector512.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector512.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
                if (length > (nuint)Vector512<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector512<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else
                {
                    ptr += (nuint)Vector512<float>.Count;
                    length -= (nuint)Vector512<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop:
            do
            {
                Vector512<float> sourceVector = Vector512.LoadAligned(ptr) * multiplierVector;
                Vector512<int> resultVector = Vector512.ConvertToInt32(sourceVector);
                Vector512<float> resultVectorAsFloat = Vector512.ConvertToSingle(resultVector);
                resultVector += Vector512.ConditionalSelect(
                        condition: blendVector,
                        left: Vector512.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector512.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.StoreAligned((int*)ptr);
                ptr += (nuint)Vector512<float>.Count;
                length -= (nuint)Vector512<float>.Count;
                continue;
            } while (length >= (nuint)Vector512<float>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector512<float>.Count;
                Vector512<float> sourceVector = Vector512.Load(ptr) * multiplierVector;
                Vector512<int> resultVector = Vector512.ConvertToInt32(sourceVector);
                Vector512<float> resultVectorAsFloat = Vector512.ConvertToSingle(resultVector);
                resultVector += Vector512.ConditionalSelect(
                        condition: blendVector,
                        left: Vector512.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector512.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_256(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector256<float> multiplierVector = CreatePointVector_256(pointsPerPixelX, pointsPerPixelY);
            Vector256<int> blendVector = _blendVector_256;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector256<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector256<float> sourceVector = Vector256.Load(ptr) * multiplierVector;
                Vector256<int> resultVector = Vector256.ConvertToInt32(sourceVector);
                Vector256<float> resultVectorAsFloat = Vector256.ConvertToSingle(resultVector);
                resultVector += Vector256.ConditionalSelect(
                        condition: blendVector,
                        left: Vector256.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector256.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
                if (length > (nuint)Vector256<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector256<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else
                {
                    ptr += (nuint)Vector256<float>.Count;
                    length -= (nuint)Vector256<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop:
            do
            {
                Vector256<float> sourceVector = Vector256.LoadAligned(ptr) * multiplierVector;
                Vector256<int> resultVector = Vector256.ConvertToInt32(sourceVector);
                Vector256<float> resultVectorAsFloat = Vector256.ConvertToSingle(resultVector);
                resultVector += Vector256.ConditionalSelect(
                        condition: blendVector,
                        left: Vector256.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector256.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.StoreAligned((int*)ptr);
                ptr += (nuint)Vector256<float>.Count;
                length -= (nuint)Vector256<float>.Count;
                continue;
            } while (length >= (nuint)Vector256<float>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector256<float>.Count;
                Vector256<float> sourceVector = Vector256.Load(ptr) * multiplierVector;
                Vector256<int> resultVector = Vector256.ConvertToInt32(sourceVector);
                Vector256<float> resultVectorAsFloat = Vector256.ConvertToSingle(resultVector);
                resultVector += Vector256.ConditionalSelect(
                        condition: blendVector,
                        left: Vector256.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector256.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects_128(ref float* ptr, ref nuint length, float pointsPerPixelX, float pointsPerPixelY)
        {
            Vector128<float> multiplierVector = CreatePointVector_128(pointsPerPixelX, pointsPerPixelY);
            Vector128<int> blendVector = _blendVector_128;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector128<float>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector128<float> sourceVector = Vector128.Load(ptr) * multiplierVector;
                Vector128<int> resultVector = Vector128.ConvertToInt32(sourceVector);
                Vector128<float> resultVectorAsFloat = Vector128.ConvertToSingle(resultVector);
                resultVector += Vector128.ConditionalSelect(
                        condition: blendVector,
                        left: Vector128.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector128.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
                if (length > (nuint)Vector128<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector128<float>>() - headRemainder) / UnsafeHelper.SizeOf<float>(); // 取得數量
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else
                {
                    ptr += (nuint)Vector128<float>.Count;
                    length -= (nuint)Vector128<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop:
            do
            {
                Vector128<float> sourceVector = Vector128.LoadAligned(ptr) * multiplierVector;
                Vector128<int> resultVector = Vector128.ConvertToInt32(sourceVector);
                Vector128<float> resultVectorAsFloat = Vector128.ConvertToSingle(resultVector);
                resultVector += Vector128.ConditionalSelect(
                        condition: blendVector,
                        left: Vector128.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector128.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.StoreAligned((int*)ptr);
                ptr += (nuint)Vector128<float>.Count;
                length -= (nuint)Vector128<float>.Count;
                continue;
            } while (length >= (nuint)Vector128<float>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector128<float>.Count;
                Vector128<float> sourceVector = Vector128.Load(ptr) * multiplierVector;
                Vector128<int> resultVector = Vector128.ConvertToInt32(sourceVector);
                Vector128<float> resultVectorAsFloat = Vector128.ConvertToSingle(resultVector);
                resultVector += Vector128.ConditionalSelect(
                        condition: blendVector,
                        left: Vector128.LessThan(sourceVector, resultVectorAsFloat).AsInt32(),
                        right: -Vector128.GreaterThan(sourceVector, resultVectorAsFloat).AsInt32()
                        );
                resultVector.Store((int*)ptr);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector512<int> CreateBlendVector_512()
            => Vector512.Create(-1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> CreateBlendVector_256()
            => Vector256.Create(-1, -1, 0, 0, -1, -1, 0, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> CreateBlendVector_128()
            => Vector128.Create(-1, -1, 0, 0);
    }
}
#endif