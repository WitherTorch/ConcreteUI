using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Theme;

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public static partial class UIElementHelper
    {
        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            ApplyTheme(provider, brushes, nodes, length);
        }

        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            ApplyTheme(provider, brushes, nodes, nodePrefix, length);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, [InlineParameter] int length)
        {
            if (length <= 0)
                return;
            ref D2D1Brush? brushRef = ref brushes[0];
            for (int i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushRef, i), nodes[i]);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix, [InlineParameter] int length)
        {
            if (length <= 0)
                return;
            ref D2D1Brush? brushRef = ref brushes[0];
            for (int i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushRef, i), nodePrefix + "." + nodes[i]);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(IThemeResourceProvider provider, ref D2D1Brush? brush, string node)
            => DisposeHelper.SwapDispose(ref brush, provider.TryGetBrush(node, out D2D1Brush? result) ? result.Clone() : null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ApplyTheme<TEnumerable>(IThemeResourceProvider provider, TEnumerable elements)
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
            ref UIElement elementRef = ref array[0];
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

        public static D2D1Resource GetOrCreateCheckSign(ref D2D1Resource? checkSign, D2D1DeviceContext context, D2D1StrokeStyle strokeStyle, in RectF renderingBounds)
        {
            if (checkSign is not null)
                return checkSign;
            D2D1PathGeometry geometry = context.GetFactory()!.CreatePathGeometry();
            D2D1GeometrySink sink = geometry.Open();
            float unit = (renderingBounds.Width - 2) / 7f;
            sink.BeginFigure(new PointF(1 + unit, 1 + unit * 3), D2D1FigureBegin.Filled);
            sink.AddLine(new PointF(1 + unit * 3, 1 + unit * 5));
            sink.AddLine(new PointF(1 + unit * 6, 1 + unit * 2));
            sink.EndFigure(D2D1FigureEnd.Open);
            sink.Close();
            if (context is D2D1DeviceContext1 context1)
            {
                checkSign = context1.CreateStrokedGeometryRealization(
                    geometry: geometry,
                    flatteningTolerance: D2D1.ComputeFlatteningTolerance(Matrix3x2.Identity, dpiX: 96f, dpiY: 96f, 4.0f),
                    strokeWidth: 2.0f,
                    strokeStyle: strokeStyle);
                geometry.Dispose();
            }
            else
                checkSign = geometry;
            return checkSign;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RenderElements<TEnumerable>(D2D1DeviceContext context, DirtyAreaCollector collector, float pointPerPixel,
            TEnumerable elements, bool ignoreNeedRefresh) where TEnumerable : IEnumerable<UIElement>
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
            ref UIElement elementRef = ref array[0];
            for (nint i = length - 1; i >= 0; i--)
            {
                UIElement element = UnsafeHelper.AddTypedOffset(ref elementRef, i);
                if (element is null)
                    continue;
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
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
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
            }
            return;

        Fallback:
            using IEnumerator<UIElement> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                RenderElement(context, collector, pointPerPixel, element, ignoreNeedRefresh);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElement(D2D1DeviceContext context, DirtyAreaCollector collector, float pointPerPixel,
            UIElement element, bool ignoreNeedRefresh)
        {
            if (ignoreNeedRefresh || element.NeedRefresh() || collector.IsEmptyInstance)
            {
                using (RegionalRenderingContext renderingContext =
                    RegionalRenderingContext.Create(context, collector, pointPerPixel, (RectF)element.Bounds, D2D1AntialiasMode.Aliased, out _))
                    element.Render(renderingContext);
                collector = DirtyAreaCollector.Empty;
            }
            if (element is not IContainerElement container)
                return;
            RenderElements(context, collector, pointPerPixel, container.Children, ignoreNeedRefresh);
        }
    }
}
