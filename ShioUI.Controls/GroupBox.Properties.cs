using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Extensions;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox : IAutoWidthElement, IAutoHeightElement
{
    public string Title
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _title;
        set
        {
            if (ReferenceEquals(_title, value))
                return;
            _title = value ?? string.Empty;
            Update(RenderObjectUpdateFlags.Title, RedrawType.RedrawAllContent);
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _text;
        set
        {
            if (ReferenceEquals(_text, value))
                return;
            _text = value ?? string.Empty;
            Update(RenderObjectUpdateFlags.Text, RedrawType.RedrawText);
        }
    }

    public UIElement? FirstChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.GetUnderlyingList().FirstOrDefault();
    }

    public UIElement? LastChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.GetUnderlyingList().LastOrDefault();
    }

    public int ContentLeft
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLeftCore();
    }

    public LayoutNode ContentLeftDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Left);
    }

    public int ContentTop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentTopCore();
    }

    public LayoutNode ContentTopDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Top);
    }

    public int ContentRight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentRightCore(Width);
    }

    public LayoutNode ContentRightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Right);
    }

    public int ContentBottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentBottomCore(Height);
    }

    public LayoutNode ContentBottomDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Bottom);
    }

    public int ContentWidth
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Width - (ContentLeftPadding + ContentRightPadding);
    }

    public LayoutNode ContentWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Width);
    }

    public int ContentHeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentHeightCore(Height);
    }

    public LayoutNode ContentHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Height);
    }

    public int TextTop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetTextTopCore();
    }

    public LayoutNode TextTopDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _textTopReference ??= new TextTopNode(GetWeakReference());
    }

    public Point ContentLocation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Point(GetContentLeftCore(), GetContentTopCore());
    }

    public Point TextLocation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Point(GetContentLeftCore(), GetTextTopCore());
    }

    public LayoutNode AutoWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[0] ??= new AutoWidthNode(GetWeakReference());
    }

    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[1] ??= new AutoHeightNode(GetWeakReference());
    }
}
