using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupPanel : PopupElementBase, IElementContainer, ICheckableDisposable
    {
        private readonly OneUIElementCollection _collection;

        public PopupPanel(CoreWindow window) : base(window, string.Empty)
        {
            _collection = new OneUIElementCollection(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement?> GetElements() => _collection;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<UIElement?> GetElementsForRender() => ElementContainerDefaults.GetActiveElements(this);

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

        protected override void ApplyThemeCore(IThemeResourceProvider provider) => _collection.Value?.ApplyTheme(provider);

        protected override bool RenderCore(in RegionalRenderingContext context) => true;

        public void RenderBackground(UIElement element, in RegionalRenderingContext context) => RenderBackground(context);

        IEnumerable<UIElement> IElementContainer.GetElements() => _collection;

        IEnumerable<UIElement> IElementContainer.GetActiveElements() => _collection;

#if NET472_OR_GREATER
        Point IElementContainer.PointToGlobal(UIElement element, Point point) => ElementContainerDefaults.PointToGlobal(element, point);

        PointF IElementContainer.PointToGlobal(UIElement element, PointF point) => ElementContainerDefaults.PointToGlobal(element, point);

        Point IElementContainer.PointToLocal(UIElement element, Point point) => ElementContainerDefaults.PointToLocal(element, point);

        PointF IElementContainer.PointToLocal(UIElement element, PointF point) => ElementContainerDefaults.PointToLocal(element, point);
#endif

        bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

        public void Dispose() => _collection.Dispose();
    }
}
