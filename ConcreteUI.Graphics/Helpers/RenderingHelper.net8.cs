#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Helpers
{
    unsafe static partial class RenderingHelper
    {
        private static readonly Vector128<uint> CopySignMaskVector = Vector128.Create(0x80000000u);
        private static readonly Vector128<float> RoundVector = Vector128.Create(0.49999997f);

        private static partial RectF RoundInPixelCore(in Rect valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
        {
            if (Vector128.IsHardwareAccelerated)
            {
                Vector128<float> vector = ToVector(valueInPoints);
                if (pixelsPerPoint != Vector2.One)
                    vector = RoundInPixelCore_Dispatch(vector, ToVector(pixelsPerPoint), method);
                return UnsafeHelper.As<Vector128<float>, RectF>(ref vector);
            }
            else
            {
                if (pixelsPerPoint == Vector2.One)
                    return (RectF)valueInPoints;
                float left = RoundInPixelCore_Dispatch(valueInPoints.Left, pixelsPerPoint.X, method);
                float top = RoundInPixelCore_Dispatch(valueInPoints.Top, pixelsPerPoint.Y, method);
                float right = RoundInPixelCore_Dispatch(valueInPoints.Right, pixelsPerPoint.X, method);
                float bottom = RoundInPixelCore_Dispatch(valueInPoints.Bottom, pixelsPerPoint.Y, method);
                return new RectF(left, top, right, bottom);
            }
        }

        private static partial RectF RoundInPixelCore(in Rectangle valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
            => RoundInPixelCore((Rect)valueInPoints, pixelsPerPoint, method);

        private static partial RectF RoundInPixelCore(in RectF valueInPoints, Vector2 pixelsPerPoint, RoundingMethod method)
        {
            if (Vector128.IsHardwareAccelerated)
            {
                Vector128<float> scaleVector = ToVector(pixelsPerPoint);
                Vector128<float> valueVector = ToVector(valueInPoints);
                Vector128<float> vector = RoundInPixelCore_Dispatch(valueVector, scaleVector, method);
                return UnsafeHelper.As<Vector128<float>, RectF>(ref vector);
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
        private static Vector128<float> ToVector(Vector2 vector)
            => Vector128.Create([vector.X, vector.Y, vector.X, vector.Y]);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<float> ToVector(in Rect rect)
            => Vector128.ConvertToSingle(UnsafeHelper.As<Rect, Vector128<int>>(ref UnsafeHelper.AsRefIn(in rect)));

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly Vector128<float> ToVector(in RectF rect)
            => ref UnsafeHelper.As<RectF, Vector128<float>>(ref UnsafeHelper.AsRefIn(in rect));

        [Inline(InlineBehavior.Remove)]
        private static Vector128<float> RoundInPixelCore_Dispatch(in Vector128<float> valueVector, in Vector128<float> scaleVector, [InlineParameter] RoundingMethod method)
            => method switch
            {
                RoundingMethod.Floor => FloorInPixelCore_Dispatch(valueVector, scaleVector),
                RoundingMethod.Ceiling => CeilingInPixelCore_Dispatch(valueVector, scaleVector),
                RoundingMethod.Round => RoundInPixelCore_Dispatch(valueVector, scaleVector),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };

        [Inline(InlineBehavior.Remove)]
        private static Vector128<float> FloorInPixelCore_Dispatch(in Vector128<float> valueVector, in Vector128<float> scaleVector)
            => Vector128.Floor(valueVector * scaleVector) / scaleVector;

        [Inline(InlineBehavior.Remove)]
        private static Vector128<float> CeilingInPixelCore_Dispatch(in Vector128<float> valueVector, in Vector128<float> scaleVector)
            => Vector128.Ceiling(valueVector * scaleVector) / scaleVector;

        [Inline(InlineBehavior.Remove)]
        private static Vector128<float> RoundInPixelCore_Dispatch(in Vector128<float> valueVector, in Vector128<float> scaleVector)
        {
            return Round(valueVector * scaleVector) / scaleVector;

            [Inline(InlineBehavior.Remove)]
            static Vector128<float> CopySign(Vector128<float> x, Vector128<float> y) // MathF.CopySign 的向量化版本
            {
                // This method is required to work for all inputs,
                // including NaN, so we operate on the raw bits.
                Vector128<uint> xbits = Vector128.AsUInt32(x);
                Vector128<uint> ybits = Vector128.AsUInt32(y);

                // Remove the sign from x, and remove everything but the sign from y
                // Then, simply OR them to get the correct sign
                return Vector128.AsSingle(Vector128.ConditionalSelect(CopySignMaskVector, ybits, xbits));
            }

            [Inline(InlineBehavior.Remove)]
            static Vector128<float> Round(Vector128<float> value) // MathI.Round 的向量化版本
            {
                // result = Truncate(value + MathF.CopySign(0.49999997f, value))
                return Vector128.ConvertToSingle(Vector128.ConvertToInt32(value + CopySign(RoundVector, value)));
            }
        }
    }
}
#endif