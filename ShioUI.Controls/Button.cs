using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Helpers;

using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class Button : ButtonBase
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "border",
        "border.hovered",
        "face",
        "face.hovered",
        "face.pressed",
        "fore",
        "fore.inactive",
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];

    private DWriteTextLayout? _layout;
    private WeakReference<Button>? _reference;
    private string? _fontName;
    private string _text;

    private float _fontSize;
    private long _rawUpdateFlags;

    public Button(IElementContainer parent) : base(parent, "app.button")
    {
        _fontSize = UIConstants.BoxFontSize;
        _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
        _text = string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WeakReference<Button> GetWeakReference()
    {
        WeakReference<Button>? reference = InterlockedHelper.Read(ref _reference);
        if (reference is null)
        {
            reference = new WeakReference<Button>(this);
            WeakReference<Button>? oldReference = InterlockedHelper.CompareExchange(ref _reference, reference, null);
            if (oldReference is not null)
                reference = oldReference;
        }
        return reference;
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
                format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleCenter, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
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

    protected override bool IsBackgroundOpaqueCore()
    {
        uint pressState = (uint)PressState;
        return GraphicsUtils.CheckBrushIsSolid(UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes),
            (uint)Brush.FaceBrush + (pressState >= 3 ? 0 : pressState)));
    }

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        uint pressState = (uint)PressState;
        D2D1Brush faceBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (uint)Brush.FaceBrush + (pressState >= 3 ? 0 : pressState));
        D2D1Brush borderBrush = UnsafeHelper.AddTypedOffset(ref brushesRef,
            (int)Brush.BorderBrush + MathHelper.BooleanToUInt32(pressState >= (uint)ButtonTriState.Hovered));
        RenderBackground(context, faceBrush);

        DWriteTextLayout? layout = GetTextLayout(flags);
        if (layout is not null)
        {
            SizeF renderSize = context.Size;
            D2D1Brush brush = UnsafeHelper.AddTypedOffset(ref brushesRef, (uint)Brush.TextBrush + MathHelper.BooleanToUInt32(!Enabled));
            layout.MaxWidth = renderSize.Width;
            layout.MaxHeight = renderSize.Height;
            context.DrawTextLayout(PointF.Empty, layout, brush, D2D1DrawTextOptions.Clip);
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
        }
        context.DrawBorder(borderBrush);
        return true;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDispose(ref _layout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
