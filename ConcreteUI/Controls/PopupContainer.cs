using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupContainer : PopupElementBase, IElementContainer, IDisposable
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement?> GetElements() => _children;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement?> GetActiveElements() => ElementContainerDefaults.GetActiveElements(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(UIElement element) => _children.Add(element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChild(UIElement element) => _children.Remove(element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderBackground(UIElement element, in RegionalRenderingContext context)
            => RenderBackground(context, _brushes[(int)Brush.BackBrush]);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            D2D1Brush[] brushes = _brushes;
            RenderBackground(context, brushes[(int)Brush.BackBrush]);
            context.DrawBorder(brushes[(int)Brush.BorderBrush]);

            return true;
        }

        private void DisposeCore(bool disposing)
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            if (disposing)
                DisposeHelper.DisposeAll(_brushes);
            SequenceHelper.Clear(_brushes);
            ListHelper.CleanAllWeak<UIElement, ObservableList<UIElement>>(_children, disposing);
        }

        ~PopupContainer() => DisposeCore(disposing: false);

        public void Dispose()
        {
            DisposeCore(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
