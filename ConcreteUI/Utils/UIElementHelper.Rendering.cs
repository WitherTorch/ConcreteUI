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

partial class UIElementHelper
{
    public static bool RenderElements<TEnumerable>(in RegionalRenderingContext context, TEnumerable elements) where TEnumerable : IEnumerable<UIElement?>
        => RenderElements(context, elements, context.IsForceRendering);

    public static bool RenderElements<TEnumerable>(in RegionalRenderingContext context, TEnumerable elements, bool ignoreNeedRefresh) where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);

        int count = scope.Count;
        if (count <= 0)
            return true;
        return RenderElementsCore(context, in scope.GetReferenceOfFirstElement(), (nuint)count, ignoreNeedRefresh);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RenderElementsCore(in RegionalRenderingContext context, ref readonly UIElement? elementArrayRef, nuint length, bool ignoreNeedRefresh)
    {
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), ignoreNeedRefresh))
                return false;
            if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1), ignoreNeedRefresh))
                return false;
            if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2), ignoreNeedRefresh))
                return false;
            if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 3), ignoreNeedRefresh))
                return false;
        }
        switch (length)
        {
            case 3:
                if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2), ignoreNeedRefresh))
                    return false;
                goto case 2;
            case 2:
                if (!RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1), ignoreNeedRefresh))
                    return false;
                goto case 1;
            case 1:
                return RenderElement(context, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i), ignoreNeedRefresh);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RenderElement(in RegionalRenderingContext context, UIElement? element, bool ignoreNeedRefresh)
    {
        if (element is null)
            return true;
        if (element.CheckLayoutOutdated())
            return false;
        if (ignoreNeedRefresh || context.IsForceRendering || element.NeedRefresh())
        {
            bool isOpaque = element.IsBackgroundOpaque();
            ClearTypeSwitcher.SetClearType(context.DeviceContext, isOpaque);
            using (RegionalRenderingContext elementContext = context.WithPixelAlignedClip(
                (RectF)element.Bounds, D2D1AntialiasMode.Aliased, isOpaque, out _))
                element.Render(elementContext);
            if (element is IElementContainer container)
                return RenderElements(context.WithEmptyDirtyCollector(), container.GetActiveElements(), ignoreNeedRefresh: true);
        }
        else
        {
            if (element is IElementContainer container)
                return RenderElements(context, container.GetActiveElements(), ignoreNeedRefresh);
        }
        return true;
    }
}
