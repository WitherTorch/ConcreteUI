using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ShioUI.Utils;
using ShioUI.Windows;
using ShioUI.Graphics;
using ShioUI.Theme;

using RiceTea.Core;
using RiceTea.Core.Extensions;

namespace ShioUI.Controls;

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

    protected override void ApplyThemeCore(IThemeResourceProvider provider) => UIElementHelper.ApplyTheme(provider, _collection.Value);

    protected override bool RenderCore(in RegionalRenderingContext context) => true;

    public void RenderBackground(UIElement element, in RegionalRenderingContext context) => RenderBackground(context);

    IEnumerable<UIElement> IElementContainer.GetElements() => _collection;

    IEnumerable<UIElement> IElementContainer.GetActiveElements() => _collection;

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            _collection.Dispose();
    }
}
