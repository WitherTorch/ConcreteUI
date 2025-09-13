#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;

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
            if (Limits.UseVector512())
            {
                Vector512<float>* sourceLimit = ((Vector512<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector512<float> zeroVector = Vector512.Create<float>(0.0f);
                    Vector512<float> multiplierVector = Vector512.Create<float>(pointsPerPixel);
                    Vector512<float> roundAdditionVector = Vector512.Create<float>(0.5f);
                    do
                    {
                        Vector512<float> sourceVector = Vector512.Load((float*)source);
                        Vector512<int> resultVector = Vector512.ConvertToInt32((Vector512.Max(sourceVector, zeroVector) * multiplierVector) + roundAdditionVector);
                        resultVector.Store((int*)source);
                        source = (RectF*)sourceLimit;
                    } while (++sourceLimit < sourceEnd);
                }
                if (source >= sourceEnd)
                    return;
            }
            if (Limits.UseVector256())
            {
                Vector256<float>* sourceLimit = ((Vector256<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector256<float> zeroVector = Vector256.Create<float>(0.0f);
                    Vector256<float> multiplierVector = Vector256.Create<float>(pointsPerPixel);
                    Vector256<float> roundAdditionVector = Vector256.Create<float>(0.5f);
                    do
                    {
                        Vector256<float> sourceVector = Vector256.Load((float*)source);
                        Vector256<int> resultVector = Vector256.ConvertToInt32((Vector256.Max(sourceVector, zeroVector) * multiplierVector) + roundAdditionVector);
                        resultVector.Store((int*)source);
                        source = (RectF*)sourceLimit;
                    } while (++sourceLimit < sourceEnd);
                }
                if (source >= sourceEnd)
                    return;
            }
            if (Limits.UseVector128())
            {
                Vector128<float>* sourceLimit = ((Vector128<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector128<float> zeroVector = Vector128.Create<float>(0.0f);
                    Vector128<float> multiplierVector = Vector128.Create<float>(pointsPerPixel);
                    Vector128<float> roundAdditionVector = Vector128.Create<float>(0.5f);
                    do
                    {
                        Vector128<float> sourceVector = Vector128.Load((float*)source);
                        Vector128<int> resultVector = Vector128.ConvertToInt32((Vector128.Max(sourceVector, zeroVector) * multiplierVector) + roundAdditionVector);
                        resultVector.Store((int*)source);
                        source = (RectF*)sourceLimit;
                    } while (++sourceLimit < sourceEnd);
                }
                if (source >= sourceEnd)
                    return;
            }
            if (Limits.UseVector64())
            {
                Vector64<float>* sourceLimit = ((Vector64<float>*)source) + 1;
                if (sourceLimit < sourceEnd)
                {
                    Vector64<float> zeroVector = Vector64.Create<float>(0.0f);
                    Vector64<float> multiplierVector = Vector64.Create<float>(pointsPerPixel);
                    Vector64<float> roundAdditionVector = Vector64.Create<float>(0.5f);
                    do
                    {
                        Vector64<float> sourceVector = Vector64.Load((float*)source);
                        Vector64<int> resultVector = Vector64.ConvertToInt32((Vector64.Max(sourceVector, zeroVector) * multiplierVector) + roundAdditionVector);
                        resultVector.Store((int*)source);
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