#if NET8_0_OR_GREATER
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Internals
{
    unsafe partial class BoundsHelper
    {
        private static readonly Vector512<int> _blendVector_512 = CreateBlendVector_512();
        private static readonly Vector256<int> _blendVector_256 = CreateBlendVector_256();
        private static readonly Vector128<int> _blendVector_128 = CreateBlendVector_128();

        private static partial void VectorizedBulkAABBHitTest(int* ptr, nuint length, int x, int y)
        {
            if (Limits.UseVector512() && length >= (nuint)Vector512<float>.Count)
                VectorizedBulkAABBHitTest_512(ref ptr, ref length, x, y);
            else if (Limits.UseVector256() && length >= (nuint)Vector256<float>.Count)
                VectorizedBulkAABBHitTest_256(ref ptr, ref length, x, y);
            else
                VectorizedBulkAABBHitTest_128(ref ptr, ref length, x, y);
        }

        [Inline(InlineBehavior.Remove)]
        private static void VectorizedBulkAABBHitTest_512(ref int* ptr, ref nuint length, int x, int y)
        {
            Vector512<int> filterVector = CreatePointVector_512(x, y);
            Vector512<int> blendVector = _blendVector_512;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector512<int>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else if (headRemainder % UnsafeHelper.SizeOf<Rect>() != 0)
                goto VectorizedLoop_Unaligned;
            else
            {
                if (length > (nuint)Vector512<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector512<int>>() - headRemainder) / UnsafeHelper.SizeOf<int>(); // 取得數量
                    DebugHelper.ThrowIf(headRemainder % 4 != 0);
                    ScalarizedBulkAABBHitTest((Rect*)ptr, headRemainder / 4, x, y);
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector512<float>.Count * 2)
                {
                    int* ptr2 = ptr + Vector512<int>.Count;
                    Vector512<int> sourceVector = Vector512.Load(ptr);
                    Vector512<int> sourceVector2 = Vector512.Load(ptr2);
                    Vector512<int> resultVector = Vector512.ConditionalSelect(
                            condition: blendVector,
                            left: Vector512.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector512.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    Vector512<int> resultVector2 = Vector512.ConditionalSelect(
                            condition: blendVector,
                            left: Vector512.LessThanOrEqual(sourceVector2, filterVector),
                            right: Vector512.GreaterThanOrEqual(sourceVector2, filterVector)
                            );
                    resultVector.Store(ptr);
                    resultVector2.Store(ptr2);
                    return;
                }
                else
                {
                    Vector512<int> sourceVector = Vector512.Load(ptr);
                    Vector512<int> resultVector = Vector512.ConditionalSelect(
                            condition: blendVector,
                            left: Vector512.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector512.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    resultVector.Store(ptr);
                    ptr += (nuint)Vector512<float>.Count;
                    length -= (nuint)Vector512<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop_Unaligned:
            do
            {
                Vector512<int> sourceVector = Vector512.Load(ptr);
                Vector512<int> resultVector = Vector512.ConditionalSelect(
                        condition: blendVector,
                        left: Vector512.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector512.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.Store(ptr);
                ptr += (nuint)Vector512<int>.Count;
                length -= (nuint)Vector512<int>.Count;
                continue;
            } while (length >= (nuint)Vector512<int>.Count);
            goto TailProcess;

        VectorizedLoop:
            do
            {
                Vector512<int> sourceVector = Vector512.LoadAligned(ptr);
                Vector512<int> resultVector = Vector512.ConditionalSelect(
                        condition: blendVector,
                        left: Vector512.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector512.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.StoreAligned(ptr);
                ptr += (nuint)Vector512<int>.Count;
                length -= (nuint)Vector512<int>.Count;
                continue;
            } while (length >= (nuint)Vector512<int>.Count);
            goto TailProcess;

        TailProcess:
            DebugHelper.ThrowIf(length % 4 != 0);
            ScalarizedBulkAABBHitTest((Rect*)ptr, length / 4, x, y);
        }

        [Inline(InlineBehavior.Remove)]
        private static void VectorizedBulkAABBHitTest_256(ref int* ptr, ref nuint length, int x, int y)
        {
            Vector256<int> filterVector = CreatePointVector_256(x, y);
            Vector256<int> blendVector = _blendVector_256;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector256<int>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else if (headRemainder % UnsafeHelper.SizeOf<Rect>() != 0)
                goto VectorizedLoop_Unaligned;
            else
            {
                if (length > (nuint)Vector256<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector256<int>>() - headRemainder) / UnsafeHelper.SizeOf<int>(); // 取得數量
                    DebugHelper.ThrowIf(headRemainder % 4 != 0);
                    ScalarizedBulkAABBHitTest((Rect*)ptr, headRemainder / 4, x, y);
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector256<float>.Count * 2)
                {
                    int* ptr2 = ptr + Vector256<int>.Count;
                    Vector256<int> sourceVector = Vector256.Load(ptr);
                    Vector256<int> sourceVector2 = Vector256.Load(ptr2);
                    Vector256<int> resultVector = Vector256.ConditionalSelect(
                            condition: blendVector,
                            left: Vector256.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector256.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    Vector256<int> resultVector2 = Vector256.ConditionalSelect(
                            condition: blendVector,
                            left: Vector256.LessThanOrEqual(sourceVector2, filterVector),
                            right: Vector256.GreaterThanOrEqual(sourceVector2, filterVector)
                            );
                    resultVector.Store(ptr);
                    resultVector2.Store(ptr2);
                    return;
                }
                else
                {
                    Vector256<int> sourceVector = Vector256.Load(ptr);
                    Vector256<int> resultVector = Vector256.ConditionalSelect(
                            condition: blendVector,
                            left: Vector256.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector256.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    resultVector.Store(ptr);
                    ptr += (nuint)Vector256<float>.Count;
                    length -= (nuint)Vector256<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop_Unaligned:
            do
            {
                Vector256<int> sourceVector = Vector256.Load(ptr);
                Vector256<int> resultVector = Vector256.ConditionalSelect(
                        condition: blendVector,
                        left: Vector256.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector256.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.Store(ptr);
                ptr += (nuint)Vector256<int>.Count;
                length -= (nuint)Vector256<int>.Count;
                continue;
            } while (length >= (nuint)Vector256<int>.Count);
            goto TailProcess;

        VectorizedLoop:
            do
            {
                Vector256<int> sourceVector = Vector256.LoadAligned(ptr);
                Vector256<int> resultVector = Vector256.ConditionalSelect(
                        condition: blendVector,
                        left: Vector256.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector256.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.StoreAligned(ptr);
                ptr += (nuint)Vector256<int>.Count;
                length -= (nuint)Vector256<int>.Count;
                continue;
            } while (length >= (nuint)Vector256<int>.Count);
            goto TailProcess;

        TailProcess:
            DebugHelper.ThrowIf(length % 4 != 0);
            ScalarizedBulkAABBHitTest((Rect*)ptr, length / 4, x, y);
        }

        [Inline(InlineBehavior.Remove)]
        private static void VectorizedBulkAABBHitTest_128(ref int* ptr, ref nuint length, int x, int y)
        {
            Vector128<int> filterVector = CreatePointVector_128(x, y);
            Vector128<int> blendVector = _blendVector_128;

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector128<int>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else if (headRemainder % UnsafeHelper.SizeOf<Rect>() != 0)
                goto VectorizedLoop_Unaligned;
            else
            {
                if (length > (nuint)Vector128<float>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector128<int>>() - headRemainder) / UnsafeHelper.SizeOf<int>(); // 取得數量
                    DebugHelper.ThrowIf(headRemainder % 4 != 0);
                    ScalarizedBulkAABBHitTest((Rect*)ptr, headRemainder / 4, x, y);
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else if (length == (nuint)Vector128<float>.Count * 2)
                {
                    int* ptr2 = ptr + Vector128<int>.Count;
                    Vector128<int> sourceVector = Vector128.Load(ptr);
                    Vector128<int> sourceVector2 = Vector128.Load(ptr2);
                    Vector128<int> resultVector = Vector128.ConditionalSelect(
                            condition: blendVector,
                            left: Vector128.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector128.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    Vector128<int> resultVector2 = Vector128.ConditionalSelect(
                            condition: blendVector,
                            left: Vector128.LessThanOrEqual(sourceVector2, filterVector),
                            right: Vector128.GreaterThanOrEqual(sourceVector2, filterVector)
                            );
                    resultVector.Store(ptr);
                    resultVector2.Store(ptr2);
                    return;
                }
                else
                {
                    Vector128<int> sourceVector = Vector128.Load(ptr);
                    Vector128<int> resultVector = Vector128.ConditionalSelect(
                            condition: blendVector,
                            left: Vector128.LessThanOrEqual(sourceVector, filterVector),
                            right: Vector128.GreaterThanOrEqual(sourceVector, filterVector)
                            );
                    resultVector.Store(ptr);
                    ptr += (nuint)Vector128<float>.Count;
                    length -= (nuint)Vector128<float>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop_Unaligned:
            do
            {
                Vector128<int> sourceVector = Vector128.Load(ptr);
                Vector128<int> resultVector = Vector128.ConditionalSelect(
                        condition: blendVector,
                        left: Vector128.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector128.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.Store(ptr);
                ptr += (nuint)Vector128<int>.Count;
                length -= (nuint)Vector128<int>.Count;
                continue;
            } while (length >= (nuint)Vector128<int>.Count);
            goto TailProcess;

        VectorizedLoop:
            do
            {
                Vector128<int> sourceVector = Vector128.LoadAligned(ptr);
                Vector128<int> resultVector = Vector128.ConditionalSelect(
                        condition: blendVector,
                        left: Vector128.LessThanOrEqual(sourceVector, filterVector),
                        right: Vector128.GreaterThanOrEqual(sourceVector, filterVector)
                        );
                resultVector.StoreAligned(ptr);
                ptr += (nuint)Vector128<int>.Count;
                length -= (nuint)Vector128<int>.Count;
                continue;
            } while (length >= (nuint)Vector128<int>.Count);
            goto TailProcess;

        TailProcess:
            DebugHelper.ThrowIf(length % 4 != 0);
            ScalarizedBulkAABBHitTest((Rect*)ptr, length / 4, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector512<int> CreatePointVector_512(int x, int y)
        {
            if (x == y)
                return Vector512.Create(x);
            else
                return Vector512.Create(x, y, x, y, x, y, x, y, x, y, x, y, x, y, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> CreatePointVector_256(int x, int y)
        {
            if (x == y)
                return Vector256.Create(x);
            else
                return Vector256.Create(x, y, x, y, x, y, x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> CreatePointVector_128(int x, int y)
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