#if NET472_OR_GREATER
using System.Numerics;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics
{
    partial class DirtyAreaCollector
    {
        private static unsafe partial void ScaleRects(RectF* source, uint count, float pointsPerPixel)
        {
            DebugHelper.ThrowIf(sizeof(Rect) != sizeof(RectF));

            RectF* sourceEnd = source + count;
            if (Limits.UseVector())
            {
                Vector<float>* sourceLimit = ((Vector<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector<float> zeroVector = new Vector<float>(0.0f);
                    Vector<float> multiplierVector = new Vector<float>(pointsPerPixel);
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
                    MathI.Round(MathHelper.Max(sourceRect.Left, 0f) * pointsPerPixel),
                    MathI.Round(MathHelper.Max(sourceRect.Top, 0f) * pointsPerPixel),
                    MathI.Round(MathHelper.Max(sourceRect.Right, 0f) * pointsPerPixel),
                    MathI.Round(MathHelper.Max(sourceRect.Bottom, 0f) * pointsPerPixel));
                *(Rect*)source = destinationRect;
            }
        }

    }
}
#endif