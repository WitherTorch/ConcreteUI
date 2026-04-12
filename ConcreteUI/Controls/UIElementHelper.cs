using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Theme;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public static partial class UIElementHelper
    {
        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            if (length <= 0)
                return;
            ApplyThemeUnsafe(provider, brushes, nodes, (nuint)length);
        }

        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            if (length <= 0)
                return;
            ApplyThemeUnsafe(provider, brushes, nodes, nodePrefix, (nuint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, nuint length)
        {
            ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
            ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
            for (nuint i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), UnsafeHelper.AddTypedOffset(in nodesRef, i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix, nuint length)
        {
            ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
            ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
            for (nuint i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), nodePrefix + "." + UnsafeHelper.AddTypedOffset(in nodesRef, i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyTheme(IThemeResourceProvider provider, ref D2D1Brush? brushRef, string node)
            => DisposeHelper.SwapDispose(ref brushRef, provider.TryGetBrush(node, out D2D1Brush? result) ? result.Clone() : null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyTheme<TEnumerable>(IThemeResourceProvider provider, TEnumerable elements)
            where TEnumerable : IEnumerable<UIElement>
        {
            UIElement[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement[]) || elements is UIElement[])
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement>) || elements is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement>) || elements is ObservableList<UIElement>)
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement>) || elements is IList<UIElement>)
                goto List;

            goto Fallback;

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement elementRef = ref UnsafeHelper.GetArrayDataReference(array);
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                element.ApplyTheme(provider);
            }
            return;

        List:
            IList<UIElement> list = UnsafeHelper.As<TEnumerable, IList<UIElement>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement element = list[i];
                if (element is null)
                    continue;
                element.ApplyTheme(provider);
            }
            return;

        Fallback:
            using IEnumerator<UIElement> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                element.ApplyTheme(provider);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElements<TEnumerable>(D2D1DeviceContext context, DirtyAreaCollector collector, Vector2 pointPerPixel,
            TEnumerable elements, bool ignoreNeedRefresh) where TEnumerable : IEnumerable<UIElement?>
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]) || elements is UIElement?[])
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>) || elements is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>) || elements is ObservableList<UIElement?>)
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>) || elements is IList<UIElement?>)
                goto List;

            goto Fallback;

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
            goto List;

        ArrayLike:
            if (length <= 0)
                return;
            ref UIElement? elementRef = ref UnsafeHelper.GetArrayDataReference(array);
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement? element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length <= 0)
                return;
            for (int i = length - 1; i >= 0; i--)
            {
                UIElement? element = list[i];
                if (element is null)
                    continue;
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement? element = enumerator.Current;
                if (element is null)
                    continue;
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElement(D2D1DeviceContext context, DirtyAreaCollector collector, Vector2 pointPerPixel,
            UIElement element, bool ignoreNeedRefresh)
        {
            if (ignoreNeedRefresh || element.NeedRefresh() || collector.IsEmptyInstance)
            {
                bool isOpaque = element.IsBackgroundOpaque();
                ClearTypeSwitcher.SetClearType(context, isOpaque);
                using (RegionalRenderingContext renderingContext =
                    RegionalRenderingContext.Create(context, collector, pointPerPixel, (RectF)element.Bounds, D2D1AntialiasMode.Aliased, isOpaque, out _))
                    element.Render(renderingContext);
                collector = DirtyAreaCollector.Empty;
            }
            if (element is not IElementContainer container)
                return;
            RenderElements(context, collector, pointPerPixel, container.GetActiveElements(), ignoreNeedRefresh);
        }
    }
}
