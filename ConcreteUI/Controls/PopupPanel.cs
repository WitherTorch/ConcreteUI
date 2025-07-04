using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupPanel : PopupElementBase, IContainerElement
    {
        private readonly OneUIElementCollection _collection;

        public PopupPanel(CoreWindow window) : base(window, string.Empty)
        {
            _collection = new OneUIElementCollection(this);
        }

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

        protected override bool RenderCore(DirtyAreaCollector collector) => true;

        public void RenderChildBackground(UIElement child, D2D1DeviceContext context) => RenderBackground(context);
    }
}
