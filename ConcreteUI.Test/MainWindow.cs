using System.Drawing;
using System.IO;
using System.Reflection;

using ConcreteUI.Controls;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Test;

internal sealed partial class MainWindow : TabbedWindow
{
    public MainWindow(CoreWindow? parent) : base(parent, ["頁面A", "頁面B", "頁面C"])
    {
        InitializeBaseInformation();
    }

    protected override CreateWindowInfo GetCreateWindowInfo()
        => base.GetCreateWindowInfo() with { Width = 950, Height = 700 };

    protected override void OnHandleCreated(nint handle)
    {
        base.OnHandleCreated(handle);
        if (!Screen.TryGetBoundsCenteredScreen(handle, out Rectangle bounds))
            return;
        RawBounds = bounds;
    }

    private void InitializeBaseInformation()
    {
        MinimumSize = new Size(640, 560);
        Text = nameof(MainWindow);
        using Stream? stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("ConcreteUI.Test.app-icon.ico");
        if (stream is null)
            return;
        Icon = new Icon(stream);
    }

    private void ComboBox_RequestDropdownListOpening(object? sender, DropdownListEventArgs e)
    {
        ChangeOverlayElement(e.DropdownList);
    }

    private void Button_Click(UIElement sender, in MouseEventArgs args)
    {
        if (CurrentTheme?.IsDarkTheme ?? false)
        {
            if (!ThemeManager.TryGetThemeContext("#light", out IThemeContext? themeContext))
                return;
            ThemeManager.CurrentTheme = themeContext;
        }
        else
        {
            if (!ThemeManager.TryGetThemeContext("#dark", out IThemeContext? themeContext))
                return;
            ThemeManager.CurrentTheme = themeContext;
        }
    }

    private void LeftButton_Click(UIElement sender, in MouseEventArgs args)
    {
        _progressBar!.Value -= 1.0f;
    }

    private void RightButton_Click(UIElement sender, in MouseEventArgs args)
    {
        _progressBar!.Value += 1.0f;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        _ime?.Dispose();
    }
}
