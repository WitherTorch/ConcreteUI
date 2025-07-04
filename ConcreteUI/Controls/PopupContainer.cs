using System;
using System.Collections.Generic;
using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupContainer : PopupElementBase, IContainerElement
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "border"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly ObservableList<UIElement> _children;

        private bool _disposed;

        public PopupContainer(CoreWindow window) : base(window, "app.control")
        {
            _children = new ObservableList<UIElement>(new UnwrappableList<UIElement>());
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            foreach (UIElement child in _children)
                child.ApplyTheme(provider);
        }

        public void AddChild(UIElement element) => _children.Add(element);

        public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

        public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

        public void RemoveChild(UIElement element) => _children.Remove(element);

        public void RenderChildBackground(UIElement child, D2D1DeviceContext context)
            => RenderBackground(context, _brushes[(int)Brush.BackBrush]);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            IRenderer renderer = Renderer;
            Rectangle bounds = Bounds;
            D2D1DeviceContext context = renderer.GetDeviceContext();
            float lineWidth = renderer.GetBaseLineWidth();

            D2D1Brush[] brushes = _brushes;
            RenderBackground(context, brushes[(int)Brush.BackBrush]);
            context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(bounds, lineWidth), brushes[(int)Brush.BorderBrush], lineWidth);

            return true;
        }

        ~PopupContainer()
        {
            DisposeCore(disposing: false);
        }

        public void Dispose()
        {
            DisposeCore(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void DisposeCore(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
            DisposeChildren(disposing);
        }

        [Inline(InlineBehavior.Remove)]
        private void DisposeChildren(bool disposing)
        {
            IList<UIElement> children = _children.GetUnderlyingList();
            if (disposing)
            {
                int count = children.Count;
                if (children is UnwrappableList<UIElement> castedList)
                {
                    UIElement[] childrenArray = castedList.Unwrap();
                    for (int i = 0; i < count; i++)
                        (childrenArray[i] as IDisposable)?.Dispose();
                }
                else
                {
                    foreach (UIElement child in children)
                        (child as IDisposable)?.Dispose();
                }
            }
            children.Clear();
        }
    }
}
