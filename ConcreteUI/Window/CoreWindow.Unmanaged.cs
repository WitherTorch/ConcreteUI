using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window
{
    public unsafe partial class CoreWindow
    {
        #region Fields
        private UnwrappableList<IWindowMessageFilter> _filterList = new UnwrappableList<IWindowMessageFilter>(1);
        private SizeF _minimumSize, _maximumSize;
        private MouseButtons _lastMouseDownButtons;
        private IntPtr _associatedMonitor;
        private nint _beforeHitTest;
        private uint _sizeModeState;
        private int _borderWidthInPixels;
        private bool _isMaximized, _isCreateByDefaultX, _isCreateByDefaultY, _hasMouseCapture, _isSystemPrepareBoosting;
        #endregion

        #region Special Fields
        private object? _fixLagObject;
        #endregion

        #region Properties
        public SizeF MinimumSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _minimumSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value.Width < 0)
                    value.Width = 0;
                if (value.Height < 0)
                    value.Height = 0;
                _minimumSize = value;
                SizeF maximumSize = _maximumSize;
                if (maximumSize != SizeF.Empty)
                {
                    _maximumSize = new SizeF(MathHelper.Max(value.Width, maximumSize.Width),
                        MathHelper.Max(value.Height, maximumSize.Height));
                }

                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                User32.SetWindowPos(handle, IntPtr.Zero,
                    WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
            }
        }

        public SizeF MaximumSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maximumSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == SizeF.Empty)
                    _maximumSize = SizeF.Empty;
                else
                {
                    SizeF minimumSize = _minimumSize;
                    _maximumSize = new SizeF(MathHelper.Max(value.Width, minimumSize.Width),
                        MathHelper.Max(value.Height, minimumSize.Height));
                }

                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                User32.SetWindowPos(handle, IntPtr.Zero,
                    WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
            }
        }

        protected int FormBorderWidth => _borderWidthInPixels;
        #endregion

        #region Initialize
        [Inline(InlineBehavior.RemovePrivate)]
        private void InitUnmanagedPart() { }
        #endregion

        #region WndProcs
        private void OnActivate(IntPtr hwnd, nint wParam)
        {
            RenderingController? controller = _controller;
            if (controller is null)
                _isSystemPrepareBoosting = wParam != 0;
            else
                controller.SetSystemBoosting(wParam != 0);
            if (_windowMaterial == WindowMaterial.Integrated || IsUsingBackdrop())
            {
                Margins margins = new Margins(-1);
                DwmApi.DwmExtendFrameIntoClientArea(hwnd, &margins);
            }
            else
            {
                Margins margins = default;
                DwmApi.DwmExtendFrameIntoClientArea(hwnd, &margins);
            }
        }

        protected override bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            int count = filterList.Count;
            if (count <= 0)
                goto Default;
            ref IWindowMessageFilter filterRef = ref filterList.Unwrap()[0];
            for (nuint i = 0, limit = unchecked((nuint)count); i < limit; i++)
            {
                if (UnsafeHelper.AddTypedOffset(ref filterRef, i).TryProcessWindowMessage(hwnd, message, wParam, lParam, out result))
                    return true;
            }

        Default:
            return base.TryProcessWindowMessage(hwnd, message, wParam, lParam, out result);
        }

        protected override bool TryProcessSystemWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            result = 0;
            switch (message)
            {
                case WindowMessage.Activate:
                    OnActivate(hwnd, wParam);
                    return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
                case WindowMessage.DpiChanged:
                    {
                        (ushort dpiX, ushort dpiY) = wParam.GetWords();
                        ChangeDpi(dpiX, dpiY);
                        if (lParam != 0)
                        {
                            Rect clientRect = UnsafeHelper.ReadUnaligned<Rect>((void*)lParam);
                            User32.SetWindowPos(Handle,
                                IntPtr.Zero,
                                clientRect.Left,
                                clientRect.Top,
                                clientRect.Width,
                                clientRect.Height,
                                WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
                        }
                        break;
                    }
                case WindowMessage.Sizing:
                    {
                        SizeF minimumSize = _minimumSize;
                        SizeF maximumSize = _maximumSize;
                        if (minimumSize == SizeF.Empty && maximumSize == SizeF.Empty)
                            goto default;

                        Rect rect = UnsafeHelper.ReadUnaligned<Rect>((void*)lParam);
                        Size oldSize = rect.Size;
                        Size newSize = GraphicsUtils.AdjustSize(oldSize, minimumSize, maximumSize, _pointsPerPixel);
                        if (newSize == oldSize)
                            goto default;

                        switch (wParam)
                        {
                            case 1: // WMSZ_LEFT
                            case 7: // WMSZ_BOTTOMLEFT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Right - newSize.Width, rect.Top, rect.Right, rect.Top + newSize.Height));
                                break;
                            case 2: // WMSZ_RIGHT
                            case 6: // WMSZ_BOTTOM
                            case 8: // WMSZ_BOTTOMRIGHT
                                UnsafeHelper.WriteUnaligned((void*)lParam, Rect.FromXYWH(rect.Location, newSize));
                                break;
                            case 3: // WMSZ_TOP
                            case 5: // WMSZ_TOPRIGHT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Left, rect.Bottom - newSize.Height, rect.Left + newSize.Width, rect.Bottom));
                                break;
                            case 4: // WMSZ_TOPLEFT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Right - newSize.Width, rect.Bottom - newSize.Height, rect.Right, rect.Bottom));
                                break;
                        }
                    }
                    goto default;
                case WindowMessage.Size:
                    {
                        ReferenceHelper.CompareExchange(ref _sizeModeState, 2u, 1u);
                        switch (wParam)
                        {
                            case 2: // SIZE_MAXIMIZED
                                _isMaximized = true;
                                break;
                            case 1: // SIZE_MINIMIZED
                            case 0: // SIZE_RESTORED
                                _isMaximized = false;
                                break;
                            default:
                                break;
                        }
                        SizeF minimumSize = _minimumSize;
                        SizeF maximumSize = _maximumSize;
                        if (minimumSize == SizeF.Empty && maximumSize == SizeF.Empty)
                            goto default;

                        IntPtr handle = Handle;
                        if (handle == IntPtr.Zero)
                            goto default;

                        (ushort width, ushort height) = lParam.GetWords();
                        Size oldSize = new Size(width, height);
                        Size newSize = GraphicsUtils.AdjustSize(oldSize, minimumSize, maximumSize, _pointsPerPixel);

                        if (oldSize == newSize)
                            goto default;

                        User32.SetWindowPos(handle, IntPtr.Zero, Point.Empty, newSize,
                            WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
                        break;
                    }
                case WindowMessage.EnterSizeMove:
                    _sizeModeState = 1u;
                    goto default;
                case WindowMessage.ExitSizeMove:
                    if (ReferenceHelper.Exchange(ref _sizeModeState, 0u) > 1u)
                        _controller?.RequestResize(temporarily: false);
                    goto default;
                #region Normal input events
                case WindowMessage.Char:
                    {
                        CharacterInteractEventArgs args = new CharacterInteractEventArgs((char)wParam);
                        OnCharacterInputForElements(ref args);
                        if (args.Handled)
                            break;
                        goto default;
                    }
                case WindowMessage.KeyDown:
                    {
                        KeyInteractEventArgs args = new KeyInteractEventArgs((VirtualKey)wParam, (ushort)lParam);
                        OnKeyDown(ref args);
                        if (args.Handled)
                            break;
                        goto default;
                    }
                case WindowMessage.KeyUp:
                    {
                        KeyInteractEventArgs args = new KeyInteractEventArgs((VirtualKey)wParam, (ushort)lParam);
                        OnKeyUp(ref args);
                        if (args.Handled)
                            break;
                        goto default;
                    }
                case WindowMessage.MouseWheel:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        (ushort buttons, ushort delta) = wParam.GetWords();
                        MouseInteractEventArgs args = new MouseInteractEventArgs(
                            point: PointToClient(point),
                            buttons: (MouseButtons)buttons,
                            delta: UnsafeHelper.As<ushort, short>(delta));
                        OnMouseScroll(ref args);
                        if (args.Handled)
                            break;
                        goto default;
                    }
                case WindowMessage.XButtonDown:
                    result = Booleans.TrueNative;
                    goto case WindowMessage.LeftButtonDown;
                case WindowMessage.LeftButtonDown:
                case WindowMessage.MiddleButtonDown:
                case WindowMessage.RightButtonDown:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        MouseButtons buttons = ((MouseButtons)wParam) & MouseButtons._Mask;
                        MouseButtons oldButtons = ReferenceHelper.Exchange(ref _lastMouseDownButtons, buttons);
                        buttons &= ~oldButtons;
                        if (buttons == MouseButtons.None)
                            goto default;
                        if (_hasMouseCapture)
                        {
                            _hasMouseCapture = false;
                            User32.ReleaseCapture();
                        }
                        else
                        {
                            _hasMouseCapture = true;
                            User32.SetCapture(hwnd);
                        }
                        MouseInteractEventArgs args = new MouseInteractEventArgs(
                            point: GraphicsUtils.ScalingPoint(point, _pixelsPerPoint),
                            buttons: buttons);
                        OnMouseDown(ref args);
                        if (args.Handled)
                            break;
                        goto default;
                    }
                case WindowMessage.XButtonUp:
                    result = Booleans.TrueNative;
                    goto case WindowMessage.LeftButtonUp;
                case WindowMessage.LeftButtonUp:
                case WindowMessage.MiddleButtonUp:
                case WindowMessage.RightButtonUp:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        MouseButtons buttons = ((MouseButtons)wParam) & MouseButtons._Mask;
                        MouseButtons oldButtons = ReferenceHelper.Exchange(ref _lastMouseDownButtons, buttons);
                        (buttons, oldButtons) = (oldButtons & ~buttons, buttons);
                        if (_hasMouseCapture)
                        {
                            _hasMouseCapture = false;
                            User32.ReleaseCapture();
                        }
                        if (buttons == MouseButtons.None)
                            goto default;
                        MouseNotifyEventArgs args = new MouseNotifyEventArgs(
                            point: GraphicsUtils.ScalingPoint(point, _pixelsPerPoint),
                            buttons: buttons);
                        OnMouseUp(in args);
                        goto default;
                    }
                #endregion
                #region Rendering
                case WindowMessage.NCMouseMove:
                    nint hitTest = wParam;
                    if (_beforeHitTest != hitTest)
                    {
                        switch ((HitTestValue)_beforeHitTest)
                        {
                            case HitTestValue.CloseButton:
                                _titleBarButtonStatus[2] = false;
                                _titleBarButtonChangedStatus[2] = true;
                                break;
                            case HitTestValue.MaximizeButton:
                                _titleBarButtonStatus[1] = false;
                                _titleBarButtonChangedStatus[1] = true;
                                break;
                            case HitTestValue.MinimizeButton:
                                _titleBarButtonStatus[0] = false;
                                _titleBarButtonChangedStatus[0] = true;
                                break;
                            case HitTestValue.Client:
                                _titleBarButtonChangedStatus |= _titleBarButtonStatus.Exchange(0UL);
                                break;
                        }
                        _beforeHitTest = hitTest;
                        switch ((HitTestValue)hitTest)
                        {
                            case HitTestValue.CloseButton:
                                _titleBarButtonStatus[2] = true;
                                _titleBarButtonChangedStatus[2] = true;
                                break;
                            case HitTestValue.MaximizeButton:
                                _titleBarButtonStatus[1] = true;
                                _titleBarButtonChangedStatus[1] = true;
                                break;
                            case HitTestValue.MinimizeButton:
                                _titleBarButtonStatus[0] = true;
                                _titleBarButtonChangedStatus[0] = true;
                                break;
                        }
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        OnMouseMove(new MouseInteractEventArgs(
                            point: PointToClientCore(hwnd, point)));
                        _controller?.RequestUpdate(false);
                    }
                    goto default;
                case WindowMessage.MouseMove:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        OnMouseMove(new MouseInteractEventArgs(
                            point: GraphicsUtils.ScalingPoint(point, _pixelsPerPoint)));
                        if (_beforeHitTest != (nint)HitTestValue.Client)
                        {
                            switch ((HitTestValue)_beforeHitTest)
                            {
                                case HitTestValue.CloseButton:
                                    _titleBarButtonStatus[2] = false;
                                    _titleBarButtonChangedStatus[2] = true;
                                    break;
                                case HitTestValue.MaximizeButton:
                                    _titleBarButtonStatus[1] = false;
                                    _titleBarButtonChangedStatus[1] = true;
                                    break;
                                case HitTestValue.MinimizeButton:
                                    _titleBarButtonStatus[0] = false;
                                    _titleBarButtonChangedStatus[0] = true;
                                    break;
                            }
                            _beforeHitTest = (nint)HitTestValue.Client;
                            _controller?.RequestUpdate(false);
                        }
                    }
                    goto default;
                case WindowMessage.MouseLeave:
                case WindowMessage.NCMouseLeave:
                    {
                        _beforeHitTest = (nint)HitTestValue.NoWhere;
                        _titleBarButtonStatus.Reset();
                        _titleBarButtonChangedStatus.Set();
                        _controller?.RequestUpdate(false);
                    }
                    goto default;
                case WindowMessage.Paint:
                    Update();
                    goto default;
                case WindowMessage.DisplayChange:
                    UpdateWindowFps(hwnd);
                    goto default;
                case WindowMessage.WindowPositionChanged:
                    IntPtr monitor = User32.MonitorFromWindow(hwnd, MonitorFromWindowFlags.DefaultToNearest);
                    if (ReferenceHelper.Exchange(ref _associatedMonitor, monitor) != monitor)
                        UpdateWindowFps(hwnd);
                    goto default;
                #endregion
                default:
                    if (_windowMaterial == WindowMaterial.Integrated)
                        return TryProcessUIWindowMessage_Integrated(hwnd, message, wParam, lParam, out result);
                    else
                        return TryProcessUIWindowMessage_Default(hwnd, message, wParam, lParam, out result);
            }

            return true;
        }

        private bool TryProcessUIWindowMessage_Default(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            switch (message)
            {
                case WindowMessage.NCCalcSize:
                    {
                        if (wParam == default)
                        {
                            if (lParam != default)
                            {
                                Size clientSize = RawClientSize;
                                ref Rect newClientRect = ref *(Rect*)lParam;
                                newClientRect.Right = newClientRect.Left + clientSize.Width;
                                newClientRect.Bottom = newClientRect.Top + clientSize.Height;
                            }
                        }
                        else
                        {
                            AppBarData data = new AppBarData() { cbSize = sizeof(AppBarData) };
                            if (((Shell32.SHAppBarMessage(0x00000004, &data).ToInt64() & 0x1) == 0x1) && HasSizableBorder)
                            {
                                WindowPlacement windowPlacement = new WindowPlacement() { Length = sizeof(WindowPlacement) };
                                User32.GetWindowPlacement(hwnd, &windowPlacement);
                                if (windowPlacement.ShowCmd == ShowWindowCommands.ShowMaximized)
                                {
                                    NCCalcSizeParameters* lpParams = (NCCalcSizeParameters*)lParam;
                                    Rect clientRect = lpParams->rcNewWindow;
                                    int metrics_paddedBorder = User32.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
                                    int yBorder = User32.GetSystemMetrics(SystemMetric.SM_CYFRAME) + metrics_paddedBorder;
                                    float factorY = _pointsPerPixel.Y;
                                    clientRect.Bottom -= yBorder + (factorY == 1.0f ? 1 : MathI.Ceiling(1 * factorY));
                                    lpParams->rcNewWindow = clientRect;
                                }
                            }
                        }
                        result = 0;
                    }
                    break;
                case WindowMessage.NCHitTest:
                    {
                        HitTestValue hitTest = DoHitTestConcreteUI(lParam);
                        result = hitTest == HitTestValue.NoWhere ? (nint)HitTestValue.Client : (nint)hitTest;
                    }
                    break;
                case WindowMessage.SetText:
                    {
                        InterlockedHelper.Or(ref _updateFlags, (long)UpdateFlags.ChangeTitle);
                        Update();
                    }
                    goto Transfer;
                default:
                    goto Transfer;
            }

            return true;
        Transfer:
            return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
        }

        private bool TryProcessUIWindowMessage_Integrated(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            if (DwmApi.DwmDefWindowProc(hwnd, (uint)message, wParam, lParam, UnsafeHelper.AsPointerOut(out result)))
                return true;

            switch (message)
            {
                case WindowMessage.DwmCompositionChanged:
                    Margins margins = new Margins(-1);
                    DwmApi.DwmExtendFrameIntoClientArea(hwnd, &margins);
                    goto default;
                case WindowMessage.NCHitTest:
                    {
                        HitTestValue hitTest = DoHitTestForIntergratedUI(lParam);
                        if (hitTest == HitTestValue.NoWhere)
                            goto default;
                        result = (nint)hitTest;
                    }
                    break;
                default:
                    return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
            }
            return true;
        }

        protected override bool TryProcessCustomWindowMessage(IntPtr handle, uint message, nint wParam, nint lParam, out nint result)
        {
            if (message == CustomWindowMessages.ConcreteUpdateRefreshRate)
            {
                UpdateWindowFps(handle);
                result = 0;
                return true;
            }
            return base.TryProcessCustomWindowMessage(handle, message, wParam, lParam, out result);
        }

        private static void TransferMessageToChildren(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam)
        {
            for (IntPtr child = User32.GetWindow(hwnd, GetWindowCommand.Child);
                child != IntPtr.Zero; child = User32.GetWindow(child, GetWindowCommand.HwndNext))
                User32.PostMessageW(child, message, wParam, lParam);
        }
        #endregion

        #region HitTests
        private HitTestValue DoHitTestForIntergratedUI(IntPtr lParam)
        {
            PointF point = PointToClient(UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32());
            return CustomHitTest(point);
        }

        private HitTestValue DoHitTestConcreteUI(IntPtr lParam)
        {
            Rectangle bounds = RawBounds;
            Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
            point.X -= bounds.Left;
            point.Y -= bounds.Top;

            if (!_isMaximized) // Disable border hit testing to maximized window
            {
                if (HasSizableBorder)
                {
                    int borderWidth = _borderWidthInPixels;
                    int x = point.X;
                    int y = point.Y;
                    int topBorder = borderWidth;
                    int leftBorder = borderWidth;
                    int rightBorder = bounds.Width - borderWidth;
                    int bottomBorder = bounds.Height - borderWidth;
                    if (y < topBorder)
                    {
                        if (x < leftBorder) return HitTestValue.TopLeftBorder;
                        else if (x > rightBorder) return HitTestValue.TopRightBorder;
                        else return HitTestValue.TopBorder;
                    }
                    else if (y > bottomBorder)
                    {
                        if (x < leftBorder) return HitTestValue.BottomLeftBorder;
                        else if (x > rightBorder) return HitTestValue.BottomRightBorder;
                        else return HitTestValue.BottomBorder;
                    }
                    else
                    {
                        if (x < leftBorder) return HitTestValue.LeftBorder;
                        else if (x > rightBorder) return HitTestValue.RightBorder;
                    }
                }
            }
            return CustomHitTest(GraphicsUtils.ScalingPoint(point, _pixelsPerPoint));
        }
        #endregion

        #region Normal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsUsingBackdrop() => SystemConstants.VersionLevel switch
        {
            SystemVersionLevel.Windows_11_After => _windowMaterial > WindowMaterial.Gaussian,
            _ => false,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateWindowFps(IntPtr handle)
        {
            RenderingController? controller = _controller;
            if (controller is not null && controller.NeedUpdateFps)
                controller.SetFramesPerSecond(GetWindowFps(handle));
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Rational GetWindowFps(IntPtr handle)
        {
            DwmTimingInfo timingInfo = new DwmTimingInfo() { cbSize = UnsafeHelper.SizeOf<DwmTimingInfo>() };
            int hr = DwmApi.DwmGetCompositionTimingInfo(IntPtr.Zero, &timingInfo);
            if (hr >= 0)
            {
                Rational result = timingInfo.rateRefresh;
                if (result.Denominator > 0 && result.Numerator > 0)
                    return result;
            }
            return GetWindowFpsFallback(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rational GetWindowFpsFallback(IntPtr handle)
        {
            const int VREFRESH = 0x74;

            IntPtr hdc = User32.GetDC(handle);
            if (hdc == IntPtr.Zero)
                goto Fallback;
            try
            {
                int result = Gdi32.GetDeviceCaps(hdc, VREFRESH);
                if (result < 2)
                    goto Fallback;
                return new Rational((uint)result, 1);
            }
            finally
            {
                User32.ReleaseDC(handle, hdc);
            }

        Fallback:
            DeviceModeW mode;
            if (!User32.EnumDisplaySettingsW(null, iModeNum: 0, &mode))
                return new Rational(30, 1);
            return new Rational(mode.dmDisplayFrequency, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDpi(IntPtr handle)
        {
            if (!User32.TryGetDpiForWindow(handle, out uint dpiX, out uint dpiY))
            {
                dpiX = SystemConstants.DefaultDpiX;
                dpiY = SystemConstants.DefaultDpiY;
            }
            ChangeDpi(dpiX, dpiY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeDpi(uint dpiX, uint dpiY)
        {
            _borderWidthInPixels = User32.GetSystemMetrics(SystemMetric.SM_CXBORDER) + User32.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);

            dpiX = MathHelper.Min(dpiX, SystemConstants.Float32IntegerLimit);
            dpiY = MathHelper.Min(dpiY, SystemConstants.Float32IntegerLimit);
            CalculateDpi(dpiX, out float pointsPerPixelX, out float pixelsPerPointX);
            CalculateDpi(dpiY, out float pointsPerPixelY, out float pixelsPerPointY);

            Vector2 pointsPerPixel = new Vector2(pointsPerPixelX, pointsPerPixelY);
            Vector2 pixelsPerPoint = new Vector2(pixelsPerPointX, pixelsPerPointY);
            PointU dpi = new PointU(dpiX, dpiX);

            _pointsPerPixel = pointsPerPixel;
            _pixelsPerPoint = pixelsPerPoint;
            _dpi = dpi;

            ChangeDpi_RenderingPart(dpi, pointsPerPixel, pixelsPerPoint);
            OnDpiChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateDpi(uint dpi, out float pointsPerPixel, out float pixelsPerPoint)
        {
            switch (dpi)
            {
                case 0:
                    dpi = 96;
                    goto Normal;
                case 96:
                    goto Normal;
                default:
                    goto NeedAmplified;
            }

        Normal:
            pixelsPerPoint = 1.0f;
            pointsPerPixel = 1.0f;
            return;

        NeedAmplified:
            pointsPerPixel = dpi / 96.0f;
            pixelsPerPoint = 96.0f / dpi;
            return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMessageFilter(IWindowMessageFilter filter)
        {
            _filterList.Add(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveMessageFilter(IWindowMessageFilter filter)
        {
            _filterList.Remove(filter);
        }
        #endregion

        #region Hit Test
        [Inline(InlineBehavior.Remove)]
        private HitTestValue CustomHitTest_Default(PointF clientPoint)
        {
            BitVector64 titleBarStates = _titleBarStates;
            bool hasMinimum = titleBarStates[1];
            bool hasMaximum = titleBarStates[2];
            float clientX = clientPoint.X;
            float clientY = clientPoint.Y;
            float borderWidthInPointsX = _borderWidthInPointsX;
            float borderWidthInPointsY = _borderWidthInPointsY;
            float titleRightLoc;
            RectF minRect = _minRect;
            RectF maxRect = _maxRect;
            RectF closeRect = _closeRect;
            if (hasMinimum)
                titleRightLoc = minRect.X;
            else if (hasMaximum)
                titleRightLoc = maxRect.X;
            else
                titleRightLoc = closeRect.X;
            if (clientX < titleRightLoc && clientY <= _titleBarRect.Bottom && clientX >= borderWidthInPointsX && clientY >= borderWidthInPointsY)
            {
                return HitTestValue.Caption;
            }
            else if (hasMinimum && minRect.Contains(clientX, clientY))
            {
                return HitTestValue.MinimizeButton;
            }
            else if (hasMaximum && maxRect.Contains(clientX, clientY))
            {
                return HitTestValue.MaximizeButton;
            }
            else if (closeRect.Contains(clientX, clientY))
            {
                return HitTestValue.CloseButton;
            }
            return HitTestValue.NoWhere;
        }

        [Inline(InlineBehavior.Remove)]
        private HitTestValue CustomHitTest_Integrated(PointF clientPoint)
        {
            return HitTestValue.NoWhere;
        }
        #endregion

        #region Virtual Methods
        protected virtual HitTestValue CustomHitTest(PointF clientPoint)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
                return CustomHitTest_Integrated(clientPoint);
            else
                return CustomHitTest_Default(clientPoint);
        }
        #endregion

        #region Override Methods
        protected override CreateWindowInfo GetCreateWindowInfo()
        {
            CreateWindowInfo windowInfo = base.GetCreateWindowInfo();
            WindowMaterial material = _windowMaterial;
            if (material != WindowMaterial.Integrated)
                windowInfo.Styles &= ~WindowStyles.SystemMenu;
            SystemVersionLevel versionLevel = SystemConstants.VersionLevel;
            if (material == WindowMaterial.None && versionLevel == SystemVersionLevel.Windows_8)
                windowInfo.ExtendedStyles |= WindowExtendedStyles.NoRedirectionBitmap;
            else if (material >= WindowMaterial.Gaussian &&
                        versionLevel >= SystemVersionLevel.Windows_10 && versionLevel <= SystemVersionLevel.Windows_11_21H2)
            { }
            else
            {
                GraphicsDeviceProvider provider = _graphicsDeviceProviderLazy.Value;
                if (provider.IsSupportSwapChain1 && provider.IsSupportDComp)
                    windowInfo.ExtendedStyles |= WindowExtendedStyles.NoRedirectionBitmap;
            }

            const int CW_USEDEFAULT = unchecked((int)0x80000000);

            int x = windowInfo.X;
            int width = windowInfo.Width;
            _isCreateByDefaultX = x <= CW_USEDEFAULT & width <= CW_USEDEFAULT;
            int y = windowInfo.Y;
            int height = windowInfo.Height;
            _isCreateByDefaultY = y <= CW_USEDEFAULT & height <= CW_USEDEFAULT;
            return windowInfo;
        }

        protected override void OnHandleCreated(IntPtr handle)
        {
            base.OnHandleCreated(handle);

            if (SystemConstants.VersionLevel >= SystemVersionLevel.Windows_11_21H2)
                DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.CaptionColor, 0xFFFFFFFE);
            User32.SetWindowPos(handle, IntPtr.Zero,
                            WindowPositionFlags.SwapWithFrameChanged | WindowPositionFlags.SwapWithNoZOrder);

            UpdateDpi(handle);
            (float factorX, float factorY) = _pointsPerPixel;
            if (factorX == 1.0f && factorY == 1.0f)
                goto InitRenderObj;

            Rectangle bounds = RawBounds;
            Size size = bounds.Size;
            if (size.IsEmpty)
                goto InitRenderObj;

            Point location = bounds.Location;
            size.Width = MathI.Ceiling(size.Width * factorX);
            size.Height = MathI.Ceiling(size.Height * factorY);

            if (_isCreateByDefaultX)
                location.X -= (bounds.Width - size.Width) / 2;
            if (_isCreateByDefaultY)
                location.Y -= (bounds.Height - size.Height) / 2;
            User32.SetWindowPos(handle, IntPtr.Zero, location, size,
                WindowPositionFlags.SwapWithNoActivate | WindowPositionFlags.SwapWithNoZOrder);

        InitRenderObj:
            InitRenderObjects(handle);

            _associatedMonitor = User32.MonitorFromWindow(handle, MonitorFromWindowFlags.DefaultToNearest);
        }

        #region Thread-Safe Function Overwrite
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToClient(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return GraphicsUtils.ScalingPoint(PointToClientCore(handle, point), _pixelsPerPoint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToClientRaw(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return PointToClientCore(handle, point);
        }

        [LocalsInit(false)]
        private static Point PointToClientCore(IntPtr handle, Point point)
        {
            if (!User32.ScreenToClient(handle, &point))
                return Point.Empty;

            return point;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToScreen(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return GraphicsUtils.ScalingPoint(PointToClientCore(handle, point), _pointsPerPixel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToScreenRaw(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return PointToScreenCore(handle, point);
        }

        [LocalsInit(false)]
        private static Point PointToScreenCore(IntPtr handle, Point point)
        {
            if (!User32.ClientToScreen(handle, &point))
                return Point.Empty;

            return point;
        }

        #endregion
        #endregion
    }
}
