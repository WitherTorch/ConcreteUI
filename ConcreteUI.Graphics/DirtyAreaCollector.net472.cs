#if NET472_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static unsafe partial void ScaleRects(RectF* source, uint count, Vector2 pointsPerPixel)
        {
            DebugHelper.ThrowIf(sizeof(Rect) != sizeof(RectF));

            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;

            RectF* sourceEnd = source + count;
            if (Limits.UseVector())
            {
                Vector<float>* sourceLimit = ((Vector<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector<float> zeroVector = new Vector<float>(0.0f);
                    Vector<float> multiplierVector = CreateVector(pointsPerPixelX, pointsPerPixelY);
                    Vector<float> roundAdditionVector = new Vector<float>(0.5f);
                    do
                    {
                        Vector<float> sourceVector = UnsafeHelper.ReadUnaligned<Vector<float>>(source);
                        Vector<int> resultVector = Vector.ConvertToInt32((Vector.Max(sourceVector, zeroVector) * multiplierVector) + roundAdditionVector);
                        UnsafeHelper.WriteUnaligned(source, resultVector);
                        source = (RectF*)sourceLimit;
                    } while (++sourceLimit < sourceEnd);
                }
                if (source >= sourceEnd)
                    return;
            }

            for (; source < sourceEnd; source++)
            {
                RectF sourceRect = *source;
                Rect destinationRect = new Rect(
                    MathI.Round(MathHelper.Max(sourceRect.Left, 0f) * pointsPerPixelX),
                    MathI.Round(MathHelper.Max(sourceRect.Top, 0f) * pointsPerPixelY),
                    MathI.Round(MathHelper.Max(sourceRect.Right, 0f) * pointsPerPixelX),
                    MathI.Round(MathHelper.Max(sourceRect.Bottom, 0f) * pointsPerPixelY));
                *(Rect*)source = destinationRect;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<float> CreateVector(float x, float y)
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
    }
}
#endif