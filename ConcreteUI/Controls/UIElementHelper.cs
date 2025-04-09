using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static void ApplyTheme(ThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            ApplyTheme(provider, brushes, nodes, length);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(ThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, [InlineParameter] int length)
        {
            for (int i = 0; i < length; i++)
                DisposeHelper.SwapDispose(ref brushes[i], provider.TryGetBrush(nodes[i], out D2D1Brush? brush) ? brush.Clone() : null);
        }

        public static void ApplyTheme(ThemeResourceProvider provider, IEnumerable<UIElement> elements)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    ApplyTheme(provider, _array);
                    break;
                case UnwrappableList<UIElement> _list:
                    ApplyTheme(provider, _list);
                    break;
                case ObservableList<UIElement> _list:
                    ApplyTheme(provider, _list.GetUnderlyingList());
                    break;
                default:
                    ApplyThemeCore(provider, elements);
                    break;
            }
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(ThemeResourceProvider provider, UIElement[] elements)
            => ApplyTheme(provider, elements, elements.Length);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void ApplyTheme(ThemeResourceProvider provider, UnwrappableList<UIElement> elements)
            => ApplyTheme(provider, elements.Unwrap(), elements.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyTheme(ThemeResourceProvider provider, UIElement[] elements, int length)
        {
            for (int i = 0; i < length; i++)
                elements[i]?.ApplyTheme(provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyThemeCore(ThemeResourceProvider provider, IEnumerable<UIElement> elements)
        {
            foreach (UIElement element in elements)
                element?.ApplyTheme(provider);
        }

        public static D2D1Resource GetOrCreateCheckSign(ref D2D1Resource? checkSign, D2D1DeviceContext context, D2D1StrokeStyle strokeStyle, in Rect drawingBounds)
        {
            if (checkSign is not null)
                return checkSign;
            D2D1PathGeometry geometry = context.GetFactory()!.CreatePathGeometry();
            D2D1GeometrySink sink = geometry.Open();
            float unit = (drawingBounds.Width - 2) / 7f;
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

        public static void RenderElements(DirtyAreaCollector collector, IEnumerable<UIElement> elements, bool ignoreNeedRefresh)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    RenderElements(collector, _array, ignoreNeedRefresh);
                    break;
                case UnwrappableList<UIElement> _list:
                    RenderElements(collector, _list, ignoreNeedRefresh);
                    break;
                case ObservableList<UIElement> _list:
                    RenderElements(collector, _list.GetUnderlyingList(), ignoreNeedRefresh);
                    break;
                default:
                    RenderElementsCore(collector, elements, ignoreNeedRefresh);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void RenderElementsCore(DirtyAreaCollector collector, IEnumerable<UIElement> elements, bool ignoreNeedRefresh)
        {
            IEnumerator<UIElement> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                RenderElement(collector, element, ignoreNeedRefresh);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void RenderElements(DirtyAreaCollector collector, UIElement[] elements, bool ignoreNeedRefresh)
            => RenderElements(collector, elements, elements.Length, ignoreNeedRefresh);

        [Inline(InlineBehavior.Keep, export: true)]
        public static void RenderElements(DirtyAreaCollector collector, UnwrappableList<UIElement> elements, bool ignoreNeedRefresh)
            => RenderElements(collector, elements.Unwrap(), elements.Count, ignoreNeedRefresh);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElements(DirtyAreaCollector collector, UIElement[] elements, int length, bool ignoreNeedRefresh)
        {
            for (int i = 0; i < length; i++)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                RenderElement(collector, element, ignoreNeedRefresh);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElement(DirtyAreaCollector collector, UIElement element, bool ignoreNeedRefresh)
        {
            IReadOnlyCollection<UIElement>? children = (element as IContainerElement)?.Children;
            if (ignoreNeedRefresh || element.NeedRefresh() || collector.IsEmptyInstance)
            {
                element.Render(collector);
                if (children is null)
                    return;
                RenderElements(DirtyAreaCollector.Empty, children, ignoreNeedRefresh: true);
                return;
            }
            if (children is null)
                return;
            RenderElements(collector, children, ignoreNeedRefresh: ignoreNeedRefresh);
        }
    }
}
