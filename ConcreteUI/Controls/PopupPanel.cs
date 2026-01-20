using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupPanel : PopupElementBase, IElementContainer
    {
        private readonly OneUIElementCollection _collection;

        bool ISafeDisposable.IsDisposed => _collection.IsDisposed;

        public PopupPanel(CoreWindow window) : base(window, string.Empty)
        {
            _collection = new OneUIElementCollection(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement> GetElements() => _collection;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement> GetElementsForRender() => ElementContainerDefaults.GetActiveElements(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(UIElement? element) => _collection.Value ??= element;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(params UIElement[] elements) => AddChild(elements.FirstOrDefault());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(IEnumerable<UIElement> elements) => AddChild(elements.FirstOrDefault());

        public void RemoveChild(UIElement element)
        {
            OneUIElementCollection collection = _collection;
            if (collection.Value == element)
                collection.Value = null;
        }

        public void Dispose()
        {
            _collection.Dispose();
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider) => _collection.Value?.ApplyTheme(provider);

        protected override bool RenderCore(in RegionalRenderingContext context) => true;

        public void RenderBackground(UIElement element, in RegionalRenderingContext context) => RenderBackground(context);

        IEnumerable<UIElement> IElementContainer.GetElements() => _collection;

        IEnumerable<UIElement> IElementContainer.GetActiveElements() => _collection;

        bool ISafeDisposable.MarkAsDisposed() => false;

        void ISafeDisposable.DisposeCore(bool disposing) => _collection.Dispose();

        void IDisposable.Dispose() => SafeDisposableDefaults.Dispose(this);
    }
}
