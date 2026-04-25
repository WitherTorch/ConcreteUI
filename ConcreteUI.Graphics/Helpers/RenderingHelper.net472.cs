#if NET472_OR_GREATER
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Helpers
{
    unsafe static partial class RenderingHelper
    {
        private static readonly Vector<uint> CopySignMaskVector = new Vector<uint>(0x80000000);
        private static readonly Vector<float> RoundVector = new Vector<float>(0.49999997f);

        private static partial RectF RoundInPixelCore(in Rect valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
        {
            if (Vector.IsHardwareAccelerated && IsValidVectorSize())
            {
                return method switch
                {
                    RoundingMethod.Floor => FloorInPixelCore_Vectorized(valueInPoints, pixelsPerPoint),
                    RoundingMethod.Ceiling => CeilingInPixelCore_Vectorized(valueInPoints, pixelsPerPoint),
                    RoundingMethod.Round => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };
            }
            else
            {
                return method switch
                {
                    RoundingMethod.Floor => FloorInPixelCore_Scalarized(valueInPoints, pixelsPerPoint),
                    RoundingMethod.Ceiling => CeilingInPixelCore_Scalarized(valueInPoints, pixelsPerPoint),
                    RoundingMethod.Round => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };
            }
        }

        private static partial RectF RoundInPixelCore(in Rectangle valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
            => RoundInPixelCore((Rect)valueInPoints, pixelsPerPoint, method);

        private static partial RectF RoundInPixelCore(in RectF valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
        {
            if (Vector.IsHardwareAccelerated && IsValidVectorSize())
            {
                Vector<float> scaleVector = ToVector(pixelsPerPoint);
                Vector<float> valueVector = ToVector(valueInPoints);
                Vector<float> vector = RoundInPixelCore_Dispatch(valueVector, scaleVector, method);
                return UnsafeHelper.As<Vector<float>, RectF>(ref vector);
            }
            else
            {
                float left = RoundInPixelCore_Dispatch(valueInPoints.Left, pixelsPerPoint.X, method);
                float top = RoundInPixelCore_Dispatch(valueInPoints.Top, pixelsPerPoint.Y, method);
                float right = RoundInPixelCore_Dispatch(valueInPoints.Right, pixelsPerPoint.X, method);
                float bottom = RoundInPixelCore_Dispatch(valueInPoints.Bottom, pixelsPerPoint.Y, method);
                return new RectF(left, top, right, bottom);
            }
        }

        private static partial RectF RoundInPixelCore(in RectangleF valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
            => RoundInPixelCore((RectF)valueInPoints, pixelsPerPoint, method);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF FloorInPixelCore_Vectorized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Floor);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF FloorInPixelCore_Vectorized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Floor);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF CeilingInPixelCore_Vectorized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Ceiling);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF CeilingInPixelCore_Vectorized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Ceiling);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF RoundInPixelCore_Vectorized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Round);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF RoundInPixelCore_Vectorized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Vectorized(valueInPoints, pixelsPerPoint, RoundingMethod.Round);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF FloorInPixelCore_Scalarized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Floor);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF FloorInPixelCore_Scalarized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Floor);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF CeilingInPixelCore_Scalarized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Ceiling);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF CeilingInPixelCore_Scalarized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Ceiling);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF RoundInPixelCore_Scalarized(in Rect valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Round);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RectF RoundInPixelCore_Scalarized(in RectF valueInPoints, Vector2 pixelsPerPoint)
            => RoundInPixelCore_Scalarized(valueInPoints, pixelsPerPoint, RoundingMethod.Round);

        [Inline(InlineBehavior.Remove)]
        private static RectF RoundInPixelCore_Vectorized(in Rect valueInPoints, Vector2 pixelsPerPoint, [InlineParameter] RoundingMethod method)
        {
            Vector<float> vector = ToVector(valueInPoints);
            if (pixelsPerPoint != Vector2.One)
                vector = RoundInPixelCore_Dispatch(vector, ToVector(pixelsPerPoint), method);
            return UnsafeHelper.As<Vector<float>, RectF>(ref vector);
        }

        [Inline(InlineBehavior.Remove)]
        private static RectF RoundInPixelCore_Vectorized(in RectF valueInPoints, Vector2 pixelsPerPoint, [InlineParameter] RoundingMethod method)
        {
            if (pixelsPerPoint != Vector2.One)
                return valueInPoints;
            Vector<float> vector = RoundInPixelCore_Dispatch(ToVector(valueInPoints), ToVector(pixelsPerPoint), method);
            return UnsafeHelper.As<Vector<float>, RectF>(ref vector);
        }

        [Inline(InlineBehavior.Remove)]
        private static RectF RoundInPixelCore_Scalarized(in Rect valueInPoints, Vector2 pixelsPerPoint, [InlineParameter] RoundingMethod method)
        {
            if (pixelsPerPoint == Vector2.One)
                return (RectF)valueInPoints;
            float left = RoundInPixelCore_Dispatch(valueInPoints.Left, pixelsPerPoint.X, method);
            float top = RoundInPixelCore_Dispatch(valueInPoints.Top, pixelsPerPoint.Y, method);
            float right = RoundInPixelCore_Dispatch(valueInPoints.Right, pixelsPerPoint.X, method);
            float bottom = RoundInPixelCore_Dispatch(valueInPoints.Bottom, pixelsPerPoint.Y, method);
            return new RectF(left, top, right, bottom);
        }

        [Inline(InlineBehavior.Remove)]
        private static RectF RoundInPixelCore_Scalarized(in RectF valueInPoints, Vector2 pixelsPerPoint, [InlineParameter] RoundingMethod method)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float left = RoundInPixelCore_Dispatch(valueInPoints.Left, pixelsPerPoint.X, method);
            float top = RoundInPixelCore_Dispatch(valueInPoints.Top, pixelsPerPoint.Y, method);
            float right = RoundInPixelCore_Dispatch(valueInPoints.Right, pixelsPerPoint.X, method);
            float bottom = RoundInPixelCore_Dispatch(valueInPoints.Bottom, pixelsPerPoint.Y, method);
            return new RectF(left, top, right, bottom);
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidVectorSize() => sizeof(Vector<float>) switch
        {
            32 or 16 => true,
            _ => false
        };

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector(Vector2 vector)
                => sizeof(Vector<float>) switch
                {
                    32 => ToVector256(vector),
                    16 => ToVector128(vector),
                    _ => Throw()
                };

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector(in Rect rect)
                => sizeof(Vector<float>) switch
                {
                    32 => ToVector256(rect),
                    16 => ToVector128(rect),
                    _ => Throw()
                };

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector(in RectF rect)
                => sizeof(Vector<float>) switch
                {
                    32 => ToVector256(rect),
                    16 => ToVector128(rect),
                    _ => Throw()
                };

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector128(Vector2 vector)
        {
            Vector<float> result;
            Vector2* destination = (Vector2*)&result;
            destination[0] = vector;
            destination[1] = vector;
            return result;
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector128(in Rect rect)
            => Vector.ConvertToSingle(UnsafeHelper.As<Rect, Vector<int>>(ref UnsafeHelper.AsRefIn(in rect)));

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly Vector<float> ToVector128(in RectF rect)
            => ref UnsafeHelper.As<RectF, Vector<float>>(ref UnsafeHelper.AsRefIn(in rect));

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector256(Vector2 vector)
        {
            Vector<float> result = Vector<float>.One;
            Vector2* destination = (Vector2*)&result;
            destination[0] = vector;
            destination[1] = vector;
            return result;
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector256(in Rect rect)
        {
            Vector<int> result = Vector<int>.Zero;
            Rect* destination = (Rect*)&result;
            destination[0] = rect;
            return Vector.ConvertToSingle(result);
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<float> ToVector256(in RectF rect)
        {
            Vector<float> result = Vector<float>.Zero;
            RectF* destination = (RectF*)&result;
            destination[0] = rect;
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Vector<float> Throw() => throw new InvalidOperationException();

        [Inline(InlineBehavior.Remove)]
        private static Vector<float> RoundInPixelCore_Dispatch(in Vector<float> valueVector, in Vector<float> scaleVector, [InlineParameter] RoundingMethod method)
            => method switch
            {
                RoundingMethod.Floor => FloorInPixelCore_Dispatch(valueVector, scaleVector),
                RoundingMethod.Ceiling => CeilingInPixelCore_Dispatch(valueVector, scaleVector),
                RoundingMethod.Round => RoundInPixelCore_Dispatch(valueVector, scaleVector),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };

        [Inline(InlineBehavior.Remove)]
        private static Vector<float> FloorInPixelCore_Dispatch(in Vector<float> valueVector, in Vector<float> scaleVector)
        {
            Vector<float> scaledVector = valueVector * scaleVector;
            Vector<int> truncatedScaledVector = Vector.ConvertToInt32(scaledVector);
            return Vector.ConvertToSingle(truncatedScaledVector - Vector.GreaterThan(Vector.ConvertToSingle(truncatedScaledVector), scaledVector) & Vector<int>.One) / scaleVector;
        }

        [Inline(InlineBehavior.Remove)]
        private static Vector<float> CeilingInPixelCore_Dispatch(in Vector<float> valueVector, in Vector<float> scaleVector)
        {
            Vector<float> scaledVector = valueVector * scaleVector;
            Vector<int> truncatedScaledVector = Vector.ConvertToInt32(scaledVector);
            return Vector.ConvertToSingle(truncatedScaledVector + Vector.LessThan(Vector.ConvertToSingle(truncatedScaledVector), scaledVector) & Vector<int>.One) / scaleVector;
        }

        [Inline(InlineBehavior.Remove)]
        private static Vector<float> RoundInPixelCore_Dispatch(in Vector<float> valueVector, in Vector<float> scaleVector)
        {
            return Round(valueVector * scaleVector) / scaleVector;

            [Inline(InlineBehavior.Remove)]
            static Vector<float> CopySign(Vector<float> x, Vector<float> y) // MathF.CopySign 的向量化版本
            {
                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector<uint> xbits = Vector.AsVectorUInt32(x);
                Vector<uint> ybits = Vector.AsVectorUInt32(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                return Vector.AsVectorSingle(Vector.ConditionalSelect(CopySignMaskVector, ybits, xbits));
            }

            [Inline(InlineBehavior.Remove)]
            static Vector<float> Round(Vector<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Truncate(value + CopySign(RoundVector, value));
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static Vector<float> Truncate(in Vector<float> value) => Vector.ConvertToSingle(Vector.ConvertToInt32(value));
    }
}
#endif