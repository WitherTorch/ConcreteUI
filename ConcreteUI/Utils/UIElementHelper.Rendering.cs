using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Internals;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Utils;

public readonly record struct RenderInformation(bool IgnoreNeedRefresh, ulong LayoutTimestamp, ulong LastRenderTimestamp, ulong CurrentRenderTimestamp);

// Incremental result enum
public enum RenderResult : uint
{
    Successed = 0,
    RenderDesync = 1,
    LayoutDesync = 2,
}

partial class UIElementHelper
{
    public static RenderResult RenderElements<TEnumerable>(in RegionalRenderingContext context, TEnumerable elements, in RenderInformation information) where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        return RenderElementsCore(context, in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), information);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RenderResult RenderElementsCore(in RegionalRenderingContext context, ref readonly UIElement? elementArrayRef, nuint length, in RenderInformation information)
    {
        RenderResult result = RenderResult.Successed;

        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            RenderResult result1, result2, result3, result4;
            result1 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), information);
            result2 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1), information);
            result3 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2), information);
            result4 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 3), information);
            result |= (result1 | result2) | (result3 | result4);
            if (result.ShouldImmediatelyReturn())
                return result;
        }

        switch (length)
        {
            case 3:
                {
                    RenderResult result1, result2, result3;
                    result1 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), information);
                    result2 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1), information);
                    result3 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2), information);
                    return (result | result1) | (result2 | result3);
                }
            case 2:
                {
                    RenderResult result1, result2;
                    result1 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), information);
                    result2 = RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1), information);
                    return result | (result1 | result2);
                }
            case 1:
                return result | RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), information);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderResult RenderElement(in RegionalRenderingContext context, UIElement? element, in RenderInformation information)
    {
        if (element is null)
            goto Tail;

        if (element.CheckLayoutOutdated(information.LayoutTimestamp))
            return RenderResult.LayoutDesync;

        if (information.IgnoreNeedRefresh || context.IsForceRendering || element.NeedRefresh())
        {
            bool isOpaque = element.IsBackgroundOpaque();
            ClearTypeSwitcher.SetClearType(context.DeviceContext, isOpaque);
            using (RegionalRenderingContext elementContext = context.WithPixelAlignedClip(
                (RectF)element.Bounds, D2D1AntialiasMode.Aliased, isOpaque, out _))
            {
                element.Render(elementContext, information.CurrentRenderTimestamp);
            }
            if (element is IElementContainer container)
                return RenderElements(context.WithEmptyDirtyCollector(), container.GetActiveElements(), information with { IgnoreNeedRefresh = true });
        }
        else
        {
            RenderResult result = RenderResult.RenderDesync &
                (RenderResult)(MathHelper.BooleanToUInt32(element.TrySyncRenderCheckTimestamp(information.LastRenderTimestamp, information.CurrentRenderTimestamp)) - 1U);

            if (element is IElementContainer container)
                return result | RenderElements(context, container.GetActiveElements(), information);
            else
                return result;
        }

    Tail:
        return RenderResult.Successed;
    }
}
