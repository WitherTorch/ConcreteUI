using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Controls;
using ConcreteUI.Internals;
using ConcreteUI.Native;

using InlineIL;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window
{
    public unsafe partial class CoreWindow
    {
        #region Static Fields
        private void* UIDependentWndProc;
        private void* UIDependentCustomHitTest;
        #endregion

        #region Fields
        private bool _isMaximized, _isCreateByDefaultX, _isCreateByDefaultY;
        private int _borderWidth;
        private nint _beforeHitTest;
        private SizeF _minimumSize, _maximumSize;
        private UnwrappableList<IWindowMessageFilter> _filterList = new UnwrappableList<IWindowMessageFilter>(1);
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
                _maximumSize = new SizeF(MathHelper.Max(value.Width, maximumSize.Width),
                    MathHelper.Max(value.Height, maximumSize.Height));

                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                    WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoSize |
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
                SizeF minimumSize = _minimumSize;
                _maximumSize = new SizeF(MathHelper.Max(value.Width, minimumSize.Width),
                    MathHelper.Max(value.Height, minimumSize.Height));

                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                    WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoSize |
                    WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
            }
        }

        protected int FormBorderWidth => _borderWidth;
        #endregion

        #region Initialize
        [Inline(InlineBehavior.RemovePrivate)]
        private void InitUnmanagedPart() { }
        #endregion

        #region WndProcs
        protected override bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            int count = filterList.Count;
            if (count <= 0)
                goto Default;
            ref IWindowMessageFilter filterRef = ref filterList.Unwrap()[0];
            for (nuint i = 0, limit = unchecked((nuint)count); i < limit; i++)
            {
                if (UnsafeHelper.AddByteOffset(ref filterRef, i * UnsafeHelper.SizeOf<IWindowMessageFilter>()).TryProcessWindowMessage(hwnd, message, wParam, lParam, out result))
                    return true;
            }

        Default:
            return base.TryProcessWindowMessage(hwnd, message, wParam, lParam, out result);
        }

        protected override bool TryProcessSystemWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            switch (message)
            {
                case WindowMessage.DpiChanged:
                    {
                        ChangeDpi(wParam.GetWords().HighWord);
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

                        float dpiScaleFactor = _dpiScaleFactor;
                        Rect rect = UnsafeHelper.ReadUnaligned<Rect>((void*)lParam);
                        Size oldSize = rect.Size;
                        Size newSize = Clamp(oldSize, ScalingSize(minimumSize, dpiScaleFactor), ScalingSize(maximumSize, dpiScaleFactor));
                        if (newSize == oldSize)
                            goto default;

#if false
                        switch (wParam)
                        {
                            case 1: // WMSZ_LEFT
                            case 4: // WMSZ_TOPLEFT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Right - newSize.Width, rect.Top, rect.Right, rect.Top + newSize.Height));
                                break;
                            case 2: // WMSZ_RIGHT
                            case 3: // WMSZ_TOP
                            case 5: // WMSZ_TOPRIGHT
                                UnsafeHelper.WriteUnaligned((void*)lParam, Rect.FromXYWH(rect.Location, newSize));
                                break;
                            case 6: // WMSZ_BOTTOM
                            case 8: // WMSZ_BOTTOMRIGHT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Left, rect.Bottom - newSize.Height, rect.Left + newSize.Width, rect.Bottom));
                                break;
                            case 7: // WMSZ_BOTTOMLEFT
                                UnsafeHelper.WriteUnaligned((void*)lParam, new Rect(rect.Right - newSize.Width, rect.Bottom - newSize.Height, rect.Right, rect.Bottom));
                                break;
                        }
#endif
                    }
                    goto default;
                case WindowMessage.Size:
                    {
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
                        float dpiScaleFactor = _dpiScaleFactor;
                        Size oldSize = new Size(width, height);
                        Size newSize = Clamp(oldSize, ScalingSize(minimumSize, dpiScaleFactor), ScalingSize(maximumSize, dpiScaleFactor));
                        if (oldSize == newSize)
                            goto default;

#if false
                        User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, newSize.Width, newSize.Height,
                            WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
#endif
                        goto default;
                    }
                #region Normal input events
                case WindowMessage.Char:
                    OnCharacterInputForElements((char)wParam);
                    break;
                case WindowMessage.KeyDown:
                    OnKeyDown(new KeyInteractEventArgs((VirtualKey)wParam, (ushort)lParam));
                    break;
                case WindowMessage.KeyUp:
                    OnKeyUp(new KeyInteractEventArgs((VirtualKey)wParam, (ushort)lParam));
                    break;
                case WindowMessage.MouseWheel:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        (ushort keys, ushort delta) = wParam.GetWords();
                        OnMouseScroll(new MouseInteractEventArgs(
                            point: PointToClient(point),
                            keys: (MouseKeys)keys,
                            delta: UnsafeHelper.As<ushort, short>(delta)));
                    }
                    break;
                case WindowMessage.LeftButtonDown:
                case WindowMessage.MiddleButtonDown:
                case WindowMessage.RightButtonDown:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        OnMouseDown(new MouseInteractEventArgs(
                            point: ScalingPointF(point, _windowScaleFactor),
                            keys: (MouseKeys)wParam));
                    }
                    break;
                case WindowMessage.LeftButtonUp:
                case WindowMessage.MiddleButtonUp:
                case WindowMessage.RightButtonUp:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        OnMouseUp(new MouseInteractEventArgs(
                            point: ScalingPointF(point, _windowScaleFactor),
                            keys: (MouseKeys)wParam));
                    }
                    break;
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
                        _controller?.RequestUpdate(false);
                    }
                    goto default;
                case WindowMessage.MouseMove:
                    {
                        Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                        OnMouseMove(new MouseInteractEventArgs(
                            point: ScalingPointF(point, _windowScaleFactor),
                            keys: (MouseKeys)wParam));
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
                    _beforeHitTest = (nint)HitTestValue.NoWhere;
                    _titleBarButtonStatus.Reset();
                    _titleBarButtonChangedStatus.Set();
                    _controller?.RequestUpdate(false);
                    goto default;
                case WindowMessage.Paint:
                    Update();
                    goto default;
                case WindowMessage.DisplayChange:
                    ChangeDpi(User32.GetDpiForWindow(hwnd));
                    _controller?.UpdateMonitorFpsStatus();
                    Update();
                    goto default;
                #endregion
                default:
                    return TryProcessUIWindowMessage(hwnd, message, wParam, lParam, out result);
            }
            result = 0;
            return true;
        }

        private bool TryProcessUIWindowMessage_Default(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            switch (message)
            {
                case WindowMessage.Activate:
                    {
                        Margins margins;
                        bool useBackdrop = SystemConstants.VersionLevel switch
                        {
                            SystemVersionLevel.Windows_11_After => _windowMaterial > WindowMaterial.Gaussian,
                            _ => false,
                        };
                        if (useBackdrop)
                            margins = new Margins(-1);
                        else
                            margins = default;
                        DwmApi.DwmExtendFrameIntoClientArea(hwnd, &margins);
                        result = 0;
                    }
                    break;
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
                                    float dpiScaleFactor = _dpiScaleFactor;
                                    clientRect.Bottom -= yBorder + (dpiScaleFactor == 1.0f ? 1 : MathI.Ceiling(1 * dpiScaleFactor));
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
                default:
                    goto Transfer;
            }

            return true;
        Transfer:
            return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
        }

        private bool TryProcessUIWindowMessage_Integrated(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            if (DwmApi.DwmDefWindowProc(hwnd, (int)message, wParam, lParam, UnsafeHelper.AsPointerOut(out result)))
                return true;

            switch (message)
            {
                case WindowMessage.Activate:
                case WindowMessage.DwmCompositionChanged:
                    Margins margins = new Margins(-1);
                    DwmApi.DwmExtendFrameIntoClientArea(hwnd, &margins);
                    goto default;
                case WindowMessage.NCHitTest:
                    {
                        HitTestValue hitTest = DoHitTestForIntergratedUI(lParam);
                        if (hitTest == HitTestValue.NoWhere)
                            goto Transfer;
                        result = (nint)hitTest;
                    }
                    break;
                default:
                    goto Transfer;
            }

            return true;
        Transfer:
            return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
        }

        private bool TryProcessUIWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            void* wndProc = UIDependentWndProc;
            if (wndProc == null)
            {
                if (_windowMaterial == WindowMaterial.Integrated)
                {
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(TryProcessUIWindowMessage_Integrated)));
                    IL.Pop(out wndProc);
                    UIDependentWndProc = wndProc;
                }
                else
                {
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(TryProcessUIWindowMessage_Default)));
                    IL.Pop(out wndProc);
                    UIDependentWndProc = wndProc;
                }
            }
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Emit.Ldarg_2();
            IL.Emit.Ldarg_3();
            IL.Emit.Ldarg(4);
            IL.PushOutRef(out result);
            IL.Push(wndProc);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(System.Reflection.CallingConventions.HasThis,
                typeof(bool), typeof(IntPtr), typeof(WindowMessage), typeof(nint), typeof(nint), TypeRef.Type<nint>().MakeByRefType()));
            return IL.Return<bool>();
        }
        #endregion

        #region HitTests
        private HitTestValue DoHitTestForIntergratedUI(IntPtr lParam)
        {
            return CustomHitTest(ScalingPointF(PointToClientRaw(UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32()), _windowScaleFactor));
        }

        private HitTestValue DoHitTestConcreteUI(IntPtr lParam)
        {
            Rectangle bounds = base.Bounds;
            Point point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
            if (!_isMaximized) // Disable border hit testing to maximized window
            {
                if (HasSizableBorder)
                {
                    int borderWidth = _borderWidth;
                    int x = point.X;
                    int y = point.Y;
                    int topBorder = bounds.Top + borderWidth;
                    int leftBorder = bounds.Left + borderWidth;
                    int rightBorder = bounds.Right - borderWidth;
                    int bottomBorder = bounds.Bottom - borderWidth;
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
            return CustomHitTest(ScalingPointFInRect(point, bounds, _windowScaleFactor));
        }

        [Inline(InlineBehavior.Remove)]
        private HitTestValue InvokeUIDependentCustomHitTest(in PointF point)
        {
            void* hitTest = UIDependentCustomHitTest;
            if (hitTest == null)
            {
                if (_windowMaterial == WindowMaterial.Integrated)
                {
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(CustomHitTestForIntegratedUI)));
                    IL.Pop(out hitTest);
                    UIDependentCustomHitTest = hitTest;
                }
                else
                {
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(CustomHitTestForConcreteUI)));
                    IL.Pop(out hitTest);
                    UIDependentCustomHitTest = hitTest;
                }
            }
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Push(hitTest);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(System.Reflection.CallingConventions.HasThis,
                typeof(HitTestValue), TypeRef.Type<PointF>().MakeByRefType()));
            return IL.Return<HitTestValue>();
        }
        #endregion

        #region Normal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeDpi(int newDpi)
        {
            _borderWidth = User32.GetSystemMetrics(SystemMetric.SM_CXBORDER) + User32.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
            SizeF minimumSize = _minimumSize, maximumSize = _maximumSize;
            if (newDpi == 96)
            {
                baseLineWidth = _windowScaleFactor = _dpiScaleFactor = 1.0f;
            }
            else
            {
                float dpiScaleFactor = newDpi / 96.0f;
                float windowScaleFactor = 96.0f / newDpi;
                _dpiScaleFactor = dpiScaleFactor;
                _windowScaleFactor = windowScaleFactor;
                if (dpiScaleFactor > 1f)
                {
                    float factor = MathF.Round(dpiScaleFactor - float.Epsilon);
                    baseLineWidth = windowScaleFactor * factor;
                }
                else
                    baseLineWidth = windowScaleFactor;
            }
            dpi = newDpi;
            OnDpiChanged();
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
        private HitTestValue CustomHitTestForConcreteUI(in PointF clientPoint)
        {
            BitVector64 titleBarStates = this.titleBarStates;
            bool hasMinimum = titleBarStates[1];
            bool hasMaximum = titleBarStates[2];
            float clientX = clientPoint.X;
            float clientY = clientPoint.Y;
            float drawingBorderWidth = _drawingBorderWidth;
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
            if (clientX < titleRightLoc && clientY <= _titleBarRect.Bottom && clientY >= drawingBorderWidth && clientX >= drawingBorderWidth)
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

        private HitTestValue CustomHitTestForIntegratedUI(in PointF clientPoint)
        {
            return HitTestValue.NoWhere;
        }
        #endregion

        #region Virtual Methods
        protected virtual HitTestValue CustomHitTest(in PointF clientPoint)
        {
            return InvokeUIDependentCustomHitTest(clientPoint);
        }
        #endregion

        #region Override Methods
        protected override CreateWindowInfo GetCreateWindowInfo()
        {
            CreateWindowInfo windowInfo = base.GetCreateWindowInfo();
            WindowMaterial material = _windowMaterial;
            if (material != WindowMaterial.Integrated)
                windowInfo.Styles &= ~WindowStyles.SystemMenu;
            if (material == WindowMaterial.None && SystemConstants.VersionLevel == SystemVersionLevel.Windows_8)
                windowInfo.ExtendedStyles |= WindowExtendedStyles.NoRedirectionBitmap;

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

            User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                            WindowPositionFlags.SwapWithFrameChanged | WindowPositionFlags.SwapWithNoSize |
                            WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoZOrder);

            SystemVersionLevel versionLevel = SystemConstants.VersionLevel;
            if (versionLevel > SystemVersionLevel.Windows_10 || (versionLevel == SystemVersionLevel.Windows_10 && Environment.OSVersion.Version.Build >= 14393))
                ChangeDpi(User32.GetDpiForWindow(handle));
            float dpiScaleFactor = _dpiScaleFactor;
            if (dpiScaleFactor != 1.0f)
            {
                Rect rect;
                if (!User32.GetWindowRect(handle, &rect))
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                bool isDefaultX = _isCreateByDefaultX, isDefaultY = _isCreateByDefaultY;
                if ((isDefaultX | isDefaultY) && !rect.IsEmptySize)
                {
                    if (!isDefaultX)
                    {
                        int newWidth = MathI.Ceiling(rect.Width * dpiScaleFactor);
                        rect.X -= (newWidth - rect.Width) / 2;
                        rect.Width = newWidth;
                    }
                    if (!isDefaultY)
                    {
                        int newHeight = MathI.Ceiling(rect.Width * dpiScaleFactor);
                        rect.Y -= (newHeight - rect.Height) / 2;
                        rect.Height = newHeight;
                    }
                    User32.SetWindowPos(handle, IntPtr.Zero, rect.X, rect.Y, rect.Width, rect.Height,
                        WindowPositionFlags.SwapWithNoRedraw | WindowPositionFlags.SwapWithNoActivate);
                }
            }
            InitRenderObjects(handle);
        }

        #region Thread-Safe Function Overwrite
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToLocal(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return ScalingPointF(PointToLocalCore(handle, point), _windowScaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToLocalRaw(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return PointToLocalCore(handle, point);
        }

        [LocalsInit(false)]
        private static Point PointToLocalCore(IntPtr handle, Point point)
        {
            Rect rect;
            if (!User32.GetWindowRect(handle, &rect))
                return Point.Empty;

            return new Point(point.X - rect.X, point.Y - rect.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToClient(Point point)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return Point.Empty;

            return ScalingPointF(PointToClientCore(handle, point), _windowScaleFactor);
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
            Rect windowRect, clientRect;
            if (!User32.GetWindowRect(handle, &windowRect) || !User32.GetClientRect(handle, &clientRect))
                return Point.Empty;

            return new Point(point.X - windowRect.X - clientRect.X, point.Y - windowRect.Y - clientRect.Y);
        }

        #endregion
        #endregion

        #region Inline Macros
        [Inline(InlineBehavior.Remove)]
        private static Size ScalingSize(SizeF original, float dpiScaleFactor)
        {
            if (dpiScaleFactor == 1.0f)
                return new Size(MathI.Floor(original.Width), MathI.Floor(original.Height));
            else
                return new Size(MathI.Floor(original.Width * dpiScaleFactor), MathI.Floor(original.Height * dpiScaleFactor));
        }

        [Inline(InlineBehavior.Remove)]
        private static SizeF ScalingSizeF(Size original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new SizeF(original.Width * windowScaleFactor, original.Height * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        private static PointF ScalingPointF(Point original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new PointF(original.X * windowScaleFactor, original.Y * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        private static PointF ScalingPointF(PointF original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new PointF(original.X * windowScaleFactor, original.Y * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        private static PointF ScalingPointFInRect(Point original, Rect rect, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return new PointF(original.X - rect.Left, original.Y - rect.Top);
            else
                return new PointF((original.X - rect.Left) * windowScaleFactor, (original.Y - rect.Top) * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        private static Rect ScalingRect(RectF original, float dpiScaleFactor)
        {
            if (dpiScaleFactor == 1.0f)
                return (Rect)original;
            return new Rect(top: MathI.FloorPositive(original.Top * dpiScaleFactor),
                left: MathI.FloorPositive(original.Left * dpiScaleFactor),
                right: MathI.Ceiling(original.Right * dpiScaleFactor),
                bottom: MathI.Ceiling(original.Bottom * dpiScaleFactor));
        }

        [Inline(InlineBehavior.Remove)]
        private static RectF ScalingRectF(Rect original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return (RectF)original;
            return new RectF(top: original.Top * windowScaleFactor,
                left: original.Left * windowScaleFactor,
                right: original.Right * windowScaleFactor,
                bottom: original.Bottom * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        private static SizeF Clamp(Size original, SizeF min, SizeF max)
            => new SizeF(width: MathHelper.Clamp(original.Width, min.Width, max.Width),
                height: MathHelper.Clamp(original.Height, min.Height, max.Height));

        [Inline(InlineBehavior.Remove)]
        private static Size Clamp(Size original, Size min, Size max)
            => new Size(width: MathHelper.Clamp(original.Width, min.Width, max.Width),
                height: MathHelper.Clamp(original.Height, min.Height, max.Height));
        #endregion
    }
}
