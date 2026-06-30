using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ShioUI.Layout;
using ShioUI.Utils;

using InlineMethod;
using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Theme;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

public sealed partial class Label : UIElement
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "fore"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];

    private DWriteTextLayout? _layout;
    private string? _fontName;
    private string _text;
    private TextAlignment _alignment;
    private DWriteFontWeight _fontWeight;
    private DWriteFontStyle _fontStyle;
    private long _rawUpdateFlags;
    private float _fontSize;
    private bool _wordWrap;

    public Label(IElementContainer parent) : base(parent, "app.label")
    {
        _fontSize = UIConstants.DefaultFontSize;
        _alignment = TextAlignment.MiddleLeft;
        _fontWeight = DWriteFontWeight.Normal;
        _fontStyle = DWriteFontStyle.Normal;
        _rawUpdateFlags = -1L;
        _layout = null;
        _text = string.Empty;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        _fontName = provider.FontName;
        DisposeHelper.SwapDisposeInterlocked(ref _layout);
        Update(RenderObjectUpdateFlags.Format);
    }

    [Inline(InlineBehavior.Remove)]
    private void Update(RenderObjectUpdateFlags flags)
    {
        InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
        Update();
    }

    [Inline(InlineBehavior.Remove)]
    private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
        => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

    [Inline(InlineBehavior.Remove)]
    private DWriteTextLayout? GetTextLayout(RenderObjectUpdateFlags flags)
    {
        DWriteTextLayout? layout = Interlocked.Exchange(ref _layout, null);

        if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
        {
            DWriteTextFormat? format = layout;
            if (CheckFormatIsNotAvailable(format, flags))
                format = TextFormatHelper.CreateTextFormat(_alignment, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize, _fontWeight, _fontStyle);
            string text = _text;
            if (StringHelper.IsNullOrEmpty(text))
                layout = null;
            else
                layout = SharedResources.DWriteFactory.CreateTextLayout(text, format);
            format.Dispose();
        }
        return layout;
    }

    [Inline(InlineBehavior.Remove)]
    private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
    {
        if (format is null || format.IsDisposed)
            return true;
        if ((flags & RenderObjectUpdateFlags.Format) == RenderObjectUpdateFlags.Format)
        {
            format.Dispose();
            return true;
        }
        return false;
    }

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        DWriteTextLayout? layout = GetTextLayout(GetAndCleanRenderObjectUpdateFlags());
        D2D1Brush foreBrush = UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.ForeBrush);
        RenderBackground(context);
        if (layout is null)
            return true;
        SizeF renderSize = context.Size;
        layout.MaxWidth = renderSize.Width;
        layout.MaxHeight = renderSize.Height;
        layout.WordWrapping = _wordWrap ? DWriteWordWrapping.EmergencyBreak : DWriteWordWrapping.NoWrap;
        context.DrawTextLayout(PointF.Empty, layout, foreBrush, D2D1DrawTextOptions.EnableColorFont);
        DisposeHelper.NullSwapOrDispose(ref _layout, layout);
        return true;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeInterlocked(ref _layout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
