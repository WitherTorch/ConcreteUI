using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Internals;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Utils;

partial class UIElementHelper
{
    public static bool RenderElements<TEnumerable>(in RegionalRenderingContext context, TEnumerable elements) where TEnumerable : IEnumerable<UIElement?>
        => RenderElements(context, elements, context.IsForceRendering);

    public static bool RenderElements<TEnumerable>(in RegionalRenderingContext context, TEnumerable elements, bool ignoreNeedRefresh) where TEnumerable : IEnumerable<UIElement?>
    {
        UIElement?[] array;
        int length;

        if (typeof(TEnumerable) == typeof(UIElement?[]))
            goto Array;
        if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
            goto UnwrappableList;
        if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
            goto ObservableList;
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
            default:
                goto Fallback;
        }

    Array:
        array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
        length = array.Length;
        goto ArrayLike;

    UnwrappableList:
        UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
        array = unwrappableList.Unwrap();
        length = unwrappableList.Count;
        goto ArrayLike;

    ObservableList:
        IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
        elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
        if (underlyingList is UIElement?[])
            goto Array;
        if (underlyingList is UnwrappableList<UIElement?>)
            goto UnwrappableList;
        if (underlyingList is ObservableList<UIElement?>)
            goto ObservableList;
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                return RenderElementsCore(context, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                return RenderElementsCore(context, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

    Fallback:
        using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
        if (enumerator.MoveNext())
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            using PooledList<UIElement?> bufferList = new PooledList<UIElement?>(pool, capacity: 16);
            do
            {
                bufferList.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            (UIElement?[] buffer, length) = bufferList;
            try
            {
                return RenderElementsCore(context, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        return true;
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
