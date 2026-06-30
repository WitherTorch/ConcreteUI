using System.Runtime.CompilerServices;

using ShioUI.Layout;

using RiceTea.Core.Helpers;
using RiceTea.Core.Extensions;

namespace ShioUI.Controls;

partial class Button : IAutoWidthElement, IAutoHeightElement
{
    public float FontSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _fontSize;
        set
        {
            if (_fontSize == value)
                return;
            _fontSize = value;
            DisposeHelper.SwapDisposeInterlocked(ref _layout);
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _text;
        set
        {
            if (_text == value)
                return;
            _text = value;
            Update(RenderObjectUpdateFlags.Layout);
        }
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
