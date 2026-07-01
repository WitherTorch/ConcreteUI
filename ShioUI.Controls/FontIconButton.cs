using System.Drawing;
using System.Threading;

using ShioUI.Utils;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

public sealed partial class FontIconButton : ButtonBase
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "face",
        "face.hovered",
        "face.pressed"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    private FontIcon? _icon;

    public FontIconButton(IElementContainer parent) : base(parent, "app.fontIconButton")
    {
        _icon = null;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
        => UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        RenderBackground(context);
        FontIcon? icon = Interlocked.Exchange(ref _icon, null);
        if (icon is null)
            return true;

        uint pressState = (uint)PressState;
        if (pressState >= 3)
            return true;
        
        icon.Render(context, new RectangleF(PointF.Empty, context.Size), UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), pressState));
        DisposeHelper.NullSwapOrDispose(ref _icon, icon);
        return true;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeInterlocked(ref _icon);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
