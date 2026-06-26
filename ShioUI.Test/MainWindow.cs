using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using ShioUI.Graphics;
using ShioUI.Layout;
using ShioUI.Windows;
using ShioUI.Controls;
using ShioUI.Theme;
using ShioUI.Utils;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Test;

internal sealed partial class MainWindow : TabbedWindow
{
    private CancellationTokenSource? _tokenSource;
    private ulong _startingTime;
    private bool _isAnimating = false;

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
        using Stream? stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("ShioUI.Test.app-icon.ico");
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

    private void RollingButton_Click(UIElement sender, in MouseEventArgs args)
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        DisposeHelper.SwapDispose(ref _tokenSource, tokenSource);
        DoAnimationTimer(sender, tokenSource);
    }

    private async void DoAnimationTimer(UIElement sender, CancellationTokenSource cancellationTokenSource)
    {
        const int MinimumRotatingRange = 250;
        const int MaximumRotatingRange = 100;

        RenderingController? controller = GetRenderingController();
        if (controller is null)
            return;

        LayoutNode sharedNode = LayoutNode.Custom(SharedFunc);

        controller.Lock();
        sender.LeftExpression = LayoutNode.Custom(CustomXFunc);
        sender.TopExpression = LayoutNode.Custom(CustomYFunc);
        _startingTime = NativeMethods.GetTicksForSystem();
        _isAnimating = true;
        UpdateAndResize();
        controller.Unlock();

        try
        {
            await Task.Delay(10000, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        if (cancellationTokenSource.IsCancellationRequested)
            return;
        try
        {
            cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        Volatile.Write(ref _isAnimating, false);

        controller.Lock();
        sender.LeftExpression = (PageWidthDefinition - sender.WidthDefinition) / 2;
        sender.TopExpression = (PageHeightDefinition - sender.HeightDefinition) / 2;
        UpdateAndResize();
        controller.Unlock();

        int SharedFunc(in LayoutNodeManager manager) => (int)((NativeMethods.GetTicksForSystem() - _startingTime) / (TimeSpan.TicksPerSecond / 60));

        int CustomXFunc(in LayoutNodeManager manager)
        {
            int stateDiff = manager.GetComputedValue(sharedNode);
            double rotateRange = MinimumRotatingRange + ((stateDiff % 60) * 1.0 / 60) * (MaximumRotatingRange - MinimumRotatingRange);
            return manager.GetPageSize().Width / 2 - MathI.Round(Math.Cos(stateDiff * (Math.PI / 180.0)) * rotateRange);
        }

        int CustomYFunc(in LayoutNodeManager manager)
        {
            int stateDiff = manager.GetComputedValue(sharedNode);
            double rotateRange = MinimumRotatingRange + ((stateDiff % 60) * 1.0 / 60) * (MaximumRotatingRange - MinimumRotatingRange);
            return manager.GetPageSize().Height / 2 - MathI.Round(Math.Sin(stateDiff * (Math.PI / 180.0)) * rotateRange);
        }
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        _ime?.Dispose();
    }
}
