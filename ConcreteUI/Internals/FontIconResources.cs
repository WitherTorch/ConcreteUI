using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Utils;

namespace ConcreteUI.Internals;

internal sealed class FontIconResources : IDisposable
{
    private static readonly FontIconResources _instance = new FontIconResources();

    private readonly FontIcon? _maxIcon, _restoreIcon, _minIcon, _closeIcon;

    private bool _disposed;

    public static FontIconResources Instance => _instance;

    private FontIconResources()
    {
        FontIconFactory factory = FontIconFactory.Instance;
        _maxIcon = GetMaxIcon(factory);
        _restoreIcon = GetRestoreIcon(factory);
        _minIcon = GetMinIcon(factory);
        _closeIcon = GetCloseIcon(factory);
    }

    private static FontIcon? GetMaxIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xE922, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F5D6, UIConstantsPrivate.TitleBarIconSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF031, UIConstantsPrivate.TitleBarIconSize, out result))
            return result;
        return null;
    }

    private static FontIcon? GetRestoreIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xE923, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F5D7, UIConstantsPrivate.TitleBarIconSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF032, UIConstantsPrivate.TitleBarIconSize, out result))
            return result;
        return null;
    }

    private static FontIcon? GetMinIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xE921, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F5D5, UIConstantsPrivate.TitleBarIconSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF030, UIConstantsPrivate.TitleBarIconSize, out result))
            return result;
        return null;
    }

    private static FontIcon? GetCloseIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xE8BB, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F5D9, UIConstantsPrivate.TitleBarIconSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF072, UIConstantsPrivate.TitleBarIconSize, out result))
            return result;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderMaximizeButton(D2D1DeviceContext context, in RectangleF rect, D2D1Brush brush)
        => _maxIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderRestoreButton(D2D1DeviceContext context, in RectangleF rect, D2D1Brush brush)
        => _restoreIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderMinimizeButton(D2D1DeviceContext context, in RectangleF rect, D2D1Brush brush)
        => _minIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderCloseButton(D2D1DeviceContext context, in RectangleF rect, D2D1Brush brush)
        => _closeIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;
        if (disposing)
        {
            _maxIcon?.Dispose();
            _restoreIcon?.Dispose();
            _minIcon?.Dispose();
            _closeIcon?.Dispose();
        }
    }

    ~FontIconResources()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
