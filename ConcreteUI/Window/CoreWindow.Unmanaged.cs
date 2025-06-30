using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

using ConcreteUI.Internals;
using ConcreteUI.Native;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

#if !DEBUG
using InlineIL;
#endif

namespace ConcreteUI.Window
{
    public unsafe partial class CoreWindow
    {
#if DEBUG
        protected delegate void WndProcDelegate(ref Message m);
        protected delegate HitTestValue HitTestDelegate(in PointF clientPoint);
#endif

        #region Static Fields
        private static readonly ThreadLocal<int> _threadId = new ThreadLocal<int>(Kernel32.GetCurrentThreadId, false);
#if DEBUG
        private WndProcDelegate? UIDependentWndProc;
        private HitTestDelegate? UIDependentCustomHitTest;
#else
        private void* UIDependentWndProc;
        private void* UIDependentCustomHitTest;
#endif
        #endregion

        #region Fields
        private bool _isMaximized;
        private int _borderWidth;
        private FormWindowState _windowState = FormWindowState.Normal;
        private IntPtr _handle, _beforeHitTest;
        private SizeF _minimumSize, _maximumSize;
        private List<IMessageFilter> _filterList = new List<IMessageFilter>(1);
        private string _text = string.Empty;
        #endregion

        #region Special Fields
        private object? _fixLagObject;
        #endregion

        #region Properties
        protected override CreateParams CreateParams
        {
            get
            {
                bool useFlipModelSwapChain = _windowMaterial == WindowMaterial.None && SystemConstants.VersionLevel == SystemVersionLevel.Windows_8;
                CreateParams result = base.CreateParams;
                if (useFlipModelSwapChain)
                    result.ExStyle |= 0x00200000;
                return result;
            }
        }

        /// <inheritdoc cref="Control.Handle"/>
        public new IntPtr Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = _handle;
                if (handle == IntPtr.Zero)
                {
                    Thread.MemoryBarrier();
                    return _handle;
                }
                return handle;
            }
        }

        /// <inheritdoc cref="Control.IsHandleCreated"/>
        public new bool IsHandleCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle != IntPtr.Zero;
        }

        /// <inheritdoc cref="Form.ClientSize"/>
        public new SizeF ClientSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ScalingSizeF(base.ClientSize, windowScaleFactor);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => base.ClientSize = ScalingSize(value, dpiScaleFactor);
        }

        /// <inheritdoc cref="Form.MinimumSize"/>
        public new SizeF MinimumSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _minimumSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_handle != IntPtr.Zero)
                    base.MinimumSize = ScalingSize(value, dpiScaleFactor);
                _minimumSize = value;
            }
        }

        /// <inheritdoc cref="Form.MaximumSize"/>
        public new SizeF MaximumSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maximumSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_handle != IntPtr.Zero)
                {
                    base.MaximumSize = ScalingSize(value, dpiScaleFactor);
                }
                _maximumSize = value;
            }
        }

        protected int FormBorderWidth => _borderWidth;

        #region Thread-Safe Property Overwrite
        private Func<bool>? _focusedFunc;

        /// <inheritdoc/>
        public new bool Focused
        {
            get
            {
                if (InvokeRequired)
                {
                    _focusedFunc ??= () => base.Focused;
#if NET8_0_OR_GREATER
                    return Invoke(_focusedFunc);
#else
                    return (bool)Invoke(_focusedFunc);
#endif
                }
                return base.Focused;
            }
        }

        /// <inheritdoc cref="Form.Text"/>
        public new string Text
        {
            get => _text;
            set
            {
                if (ReferenceEquals(_text, value))
                    return;
                _text = value;
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                {
                    base.Text = value;
                    return;
                }
                User32.SetWindowText(handle, value);
                InterlockedHelper.Or(ref _updateFlags, (long)UpdateFlags.ChangeTitle);
                Update();
            }
        }
        #endregion
        #endregion

        #region Initialize
        [Inline(InlineBehavior.RemovePrivate)]
        private void InitUnmanagedPart()
        {
#if !DEBUG
            UIDependentWndProc = default;
            UIDependentCustomHitTest = default;
#endif
        }
        #endregion

        #region WndProcs

        protected override void WndProc(ref Message m)
        {
            List<IMessageFilter> filterList = _filterList;
            for (int i = 0, count = filterList.Count; i < count; i++)
            {
                if (filterList[i].PreFilterMessage(ref m))
                    return;
            }
            switch ((WindowMessage)m.Msg)
            {
                case WindowMessage.Create:
                    _handle = m.HWnd;
                    goto default;
                case WindowMessage.Destroy:
                    _handle = IntPtr.Zero;
                    goto default;
                case WindowMessage.Char:
                    OnCharacterInputForElements((char)m.WParam.ToInt32());
                    break;
                case WindowMessage.DpiChanged:
                    {
                        ChangeDpi(m.WParam.GetWords().highWord);
                        Rect* ptr = (Rect*)m.LParam.ToPointer();
                        if (ptr != default)
                        {
                            ref Rect clientRect = ref *ptr;
                            User32.SetWindowPos(Handle,
                                IntPtr.Zero,
                                clientRect.Left,
                                clientRect.Top,
                                clientRect.Right - clientRect.Left,
                                clientRect.Bottom - clientRect.Top
                                , WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
                        }
                        m.Result = IntPtr.Zero;
                        break;
                    }
                case WindowMessage.Size:
                    FormWindowState state;
                    switch (m.WParam.ToInt64())
                    {
                        case 2: //SIZE_MAXIMIZED
                            state = FormWindowState.Maximized;
                            _isMaximized = true;
                            if (_windowState != FormWindowState.Maximized)
                            {
                                OnWindowStateChanging(state);
                                _windowState = FormWindowState.Maximized;
                                OnWindowStateChanged();
                            }
                            break;
                        case 1: //SIZE_MINIMIZED
                            state = FormWindowState.Minimized;
                            _isMaximized = false;
                            if (_windowState != FormWindowState.Minimized)
                            {
                                OnWindowStateChanging(state);
                                _windowState = FormWindowState.Minimized;
                                OnWindowStateChanged();
                            }
                            break;
                        case 0: //SIZE_RESTORED
                            state = FormWindowState.Normal;
                            _isMaximized = false;
                            if (_windowState != FormWindowState.Normal)
                            {
                                OnWindowStateChanging(state);
                                _windowState = FormWindowState.Normal;
                                OnWindowStateChanged();
                            }
                            break;
                        default:
                            break;
                    }
                    goto default;
                case WindowMessage.WindowPositionChanging:
                    getWindowRect_Temp = default;
                    goto default;
                #region WndProc for Rendering
                case WindowMessage.NCMouseMove:
                    IntPtr hitTest = m.WParam;
                    if (_beforeHitTest != hitTest)
                    {
                        switch ((HitTestValue)_beforeHitTest.ToInt32())
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
                        switch ((HitTestValue)hitTest.ToInt32())
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
                        if (_beforeHitTest != (nint)HitTestValue.Client)
                        {
                            switch ((HitTestValue)_beforeHitTest.ToInt32())
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
                    ChangeDpi(User32.GetDpiForWindow(m.HWnd));
                    _controller?.UpdateMonitorFpsStatus();
                    Update();
                    goto default;
                #endregion
                default:
                    InvokeUIDependentWndProc(ref m);
                    break;
            }
        }

        private void WndProcForConcreteUI(ref Message m)
        {
            switch ((WindowMessage)m.Msg)
            {
                case WindowMessage.Create:
                    {
                        base.WndProc(ref m);
                        User32.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                            WindowPositionFlags.SwapWithFrameChanged | WindowPositionFlags.SwapWithNoSize | WindowPositionFlags.SwapWithNoMove);
                    }
                    break;
                case WindowMessage.Activate:
                    {
                        base.WndProc(ref m);
                        Margins margins;
                        var useBackdrop = SystemConstants.VersionLevel switch
                        {
                            SystemVersionLevel.Windows_11_After => _windowMaterial > WindowMaterial.Gaussian,
                            _ => false,
                        };
                        if (useBackdrop)
                            margins = new Margins(-1);
                        else
                            margins = default;
                        DwmApi.DwmExtendFrameIntoClientArea(Handle, &margins);
                    }
                    break;
                case WindowMessage.NCCalcSize:
                    {
                        if (m.WParam == default)
                        {
                            Rect* ptr = (Rect*)m.LParam.ToPointer();
                            if (ptr != default)
                            {
                                ref Rect clientRect = ref *ptr;
                                clientRect.Right = clientRect.Left + base.ClientSize.Width;
                                clientRect.Bottom = clientRect.Top + base.ClientSize.Height;
                            }
                        }
                        else
                        {
                            AppBarData data = new AppBarData() { cbSize = sizeof(AppBarData) };
                            if (((Shell32.SHAppBarMessage(0x00000004, &data).ToInt64() & 0x1) == 0x1) && FormBorderStyle == FormBorderStyle.Sizable)
                            {
                                WindowPlacement windowPlacement = new WindowPlacement() { Length = sizeof(WindowPlacement) };
                                User32.GetWindowPlacement(Handle, &windowPlacement);
                                if (windowPlacement.ShowCmd == ShowWindowCommands.ShowMaximized)
                                {
                                    NCCalcSizeParameters* lpParams = (NCCalcSizeParameters*)m.LParam.ToPointer();
                                    Rect clientRect = lpParams->rcNewWindow;
                                    int metrics_paddedBorder = User32.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
                                    int yBorder = User32.GetSystemMetrics(SystemMetric.SM_CYFRAME) + metrics_paddedBorder;
                                    float dpiScaleFactor = this.dpiScaleFactor;
                                    clientRect.Bottom -= yBorder + (dpiScaleFactor == 1.0f ? 1 : MathI.Ceiling(1 * dpiScaleFactor));
                                    lpParams->rcNewWindow = clientRect;
                                }
                            }
                        }
                        m.Result = IntPtr.Zero;
                    }
                    break;
                case WindowMessage.NCHitTest:
                    {
                        HitTestValue hitTest = DoHitTestConcreteUI(m.LParam);
                        m.Result = hitTest == HitTestValue.NoWhere ? (nint)HitTestValue.Client : (nint)hitTest;
                    }
                    break;
                case WindowMessage.NCLeftButtonDown:
                    {
                        HitTestValue state = (HitTestValue)m.WParam.ToInt32();
                        switch (state)
                        {
                            case HitTestValue.MinimizeButton:
                            case HitTestValue.MaximizeButton:
                            case HitTestValue.CloseButton:
                                m.Result = IntPtr.Zero;
                                break;
                            default:
                                base.WndProc(ref m);
                                break;
                        }
                    }
                    break;
                case WindowMessage.NCLeftButtonUp:
                    {
                        HitTestValue state = (HitTestValue)m.WParam.ToInt32();
                        switch (state)
                        {
                            case HitTestValue.MinimizeButton:
                                WindowState = FormWindowState.Minimized;
                                m.Result = IntPtr.Zero;
                                break;
                            case HitTestValue.MaximizeButton:
                                if (WindowState == FormWindowState.Maximized)
                                    WindowState = FormWindowState.Normal;
                                else
                                    WindowState = FormWindowState.Maximized;
                                m.Result = IntPtr.Zero;
                                break;
                            case HitTestValue.CloseButton:
                                Close();
                                m.Result = IntPtr.Zero;
                                break;
                            default:
                                base.WndProc(ref m);
                                break;
                        }
                    }
                    break;
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }

        private void WndProcForIntergratedUI(ref Message m)
        {
            IntPtr result = m.Result;
            if (DwmApi.DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, &result))
            {
                m.Result = result;
            }
            else
            {
                switch ((WindowMessage)m.Msg)
                {
                    case WindowMessage.Create:
                        {
                            base.WndProc(ref m);
                            User32.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                                WindowPositionFlags.SwapWithFrameChanged | WindowPositionFlags.SwapWithNoSize | WindowPositionFlags.SwapWithNoMove);
                        }
                        break;
                    case WindowMessage.Activate:
                    case WindowMessage.DwmCompositionChanged:
                        Margins margins = new Margins(-1);
                        DwmApi.DwmExtendFrameIntoClientArea(Handle, &margins);
                        goto default;
                    case WindowMessage.NCHitTest:
                        {
                            HitTestValue hitTest = DoHitTestForIntergratedUI(m.LParam);
                            if (hitTest == HitTestValue.NoWhere)
                            {
                                goto default;
                            }
                            else
                            {
                                m.Result = (IntPtr)hitTest;
                            }
                        }
                        break;
                    default:
                        {
                            base.WndProc(ref m);
                            break;
                        }
                }
            }
        }

#if DEBUG
        private void InvokeUIDependentWndProc(ref Message msg)
        {
            WndProcDelegate? wndProc = UIDependentWndProc;
            if (wndProc is null)
            {
                if (WindowMaterial == WindowMaterial.Integrated)
                    wndProc = WndProcForIntergratedUI;
                else
                    wndProc = WndProcForConcreteUI;
                UIDependentWndProc = wndProc;
            }
            wndProc.Invoke(ref msg);
        }
#else
        [Inline(InlineBehavior.Remove)]
        private void InvokeUIDependentWndProc(ref Message m)
        {
            void* wndProc = UIDependentWndProc;
            if (wndProc == null)
            {
                if (_windowMaterial == WindowMaterial.Integrated)
                {
                    IL.Emit.Ldarg_0();
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(WndProcForIntergratedUI)));
                    IL.Emit.Dup();
                    IL.Pop(out wndProc);
                    IL.Emit.Stfld(FieldRef.Field(typeof(CoreWindow), nameof(UIDependentWndProc)));
                }
                else
                {
                    IL.Emit.Ldarg_0();
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(WndProcForConcreteUI)));
                    IL.Emit.Dup();
                    IL.Pop(out wndProc);
                    IL.Emit.Stfld(FieldRef.Field(typeof(CoreWindow), nameof(UIDependentWndProc)));
                }
            }
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Push(wndProc);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(System.Reflection.CallingConventions.HasThis,
                typeof(void), TypeRef.Type<Message>().MakeByRefType()));
        }
#endif
        #endregion

        #region HitTests
        private HitTestValue DoHitTestForIntergratedUI(IntPtr LParam)
        {
            return CustomHitTest(ScalingPointF(PointToClientBase(GetPointFromIntPtr(LParam)), windowScaleFactor));
        }

        private HitTestValue DoHitTestConcreteUI(IntPtr LParam)
        {
            Rect windowRect = GetWindowRect();
            var point = GetPointFromIntPtr(LParam);
            if (!_isMaximized) // Disable border hit testing to maximized window
            {
                FormBorderStyle formBorderStyle = FormBorderStyle;
                if (formBorderStyle == FormBorderStyle.Sizable || formBorderStyle == FormBorderStyle.SizableToolWindow)
                {
                    int borderWidth = _borderWidth;
                    int x = point.X;
                    int y = point.Y;
                    int topBorder = windowRect.Top + borderWidth;
                    int leftBorder = windowRect.Left + borderWidth;
                    int rightBorder = windowRect.Right - borderWidth;
                    int bottomBorder = windowRect.Bottom - borderWidth;
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
            return CustomHitTest(ScalingPointFInRect(point, windowRect, windowScaleFactor));
        }

#if DEBUG
        private HitTestValue InvokeUIDependentCustomHitTest(in PointF point)
        {
            HitTestDelegate? hitTest = UIDependentCustomHitTest;
            if (hitTest is null)
            {
                if (_windowMaterial == WindowMaterial.Integrated)
                    hitTest = CustomHitTestForIntegratedUI;
                else
                    hitTest = CustomHitTestForConcreteUI;
                UIDependentCustomHitTest = hitTest;
            }
            return hitTest.Invoke(in point);
        }
#else
        [Inline(InlineBehavior.Remove)]
        private HitTestValue InvokeUIDependentCustomHitTest(in PointF point)
        {
            void* hitTest = UIDependentCustomHitTest;
            if (hitTest == null)
            {
                if (_windowMaterial == WindowMaterial.Integrated)
                {
                    IL.Emit.Ldarg_0();
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(CustomHitTestForIntegratedUI)));
                    IL.Emit.Dup();
                    IL.Pop(out hitTest);
                    IL.Emit.Stfld(FieldRef.Field(typeof(CoreWindow), nameof(UIDependentCustomHitTest)));
                }
                else
                {
                    IL.Emit.Ldarg_0();
                    IL.Emit.Ldftn(new MethodRef(typeof(CoreWindow), nameof(CustomHitTestForConcreteUI)));
                    IL.Emit.Dup();
                    IL.Pop(out hitTest);
                    IL.Emit.Stfld(FieldRef.Field(typeof(CoreWindow), nameof(UIDependentCustomHitTest)));
                }
            }
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Push(hitTest);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(System.Reflection.CallingConventions.HasThis,
                typeof(HitTestValue), TypeRef.Type<PointF>().MakeByRefType()));
            return IL.Return<HitTestValue>();
        }
#endif
        #endregion

        #region Normal Methods
        Rect getWindowRect_Temp;

        [Inline(InlineBehavior.RemovePrivate)]
        private Rect GetWindowRect()
        {
            Rect rect = getWindowRect_Temp;
            if (rect.IsEmptySize)
            {
                if (User32.GetWindowRect(_handle, &rect))
                {
                    getWindowRect_Temp = rect;
                    return rect;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return rect;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeDpi(int newDpi)
        {
            _borderWidth = User32.GetSystemMetrics(SystemMetric.SM_CXBORDER) + User32.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
            SizeF minimumSize = _minimumSize, maximumSize = _maximumSize;
            if (newDpi == 96)
            {
                baseLineWidth = windowScaleFactor = dpiScaleFactor = 1.0f;
                if (!minimumSize.IsEmpty)
                    base.MinimumSize = ScalingSize(minimumSize, 1f);
                if (!maximumSize.IsEmpty)
                    base.MaximumSize = ScalingSize(maximumSize, 1f);
            }
            else
            {
                float dpiScaleFactor = newDpi / 96.0f;
                float windowScaleFactor = 96.0f / newDpi;
                this.dpiScaleFactor = dpiScaleFactor;
                this.windowScaleFactor = windowScaleFactor;
                if (dpiScaleFactor > 1f)
                {
                    float factor = MathF.Round(dpiScaleFactor - float.Epsilon);
                    baseLineWidth = windowScaleFactor * factor;
                }
                else
                    baseLineWidth = windowScaleFactor;
                if (!minimumSize.IsEmpty)
                {
                    base.MinimumSize = ScalingSize(_minimumSize, dpiScaleFactor);
                }
                if (!maximumSize.IsEmpty)
                {
                    base.MaximumSize = ScalingSize(_maximumSize, dpiScaleFactor);
                }
            }
            dpi = newDpi;
            OnDpiChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMessageFilter(IMessageFilter filter)
        {
            _filterList.Add(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveMessageFilter(IMessageFilter filter)
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
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_windowMaterial != WindowMaterial.Integrated)
                ControlBox = false;
            SystemVersionLevel versionLevel = SystemConstants.VersionLevel;
            if (versionLevel > SystemVersionLevel.Windows_10 || (versionLevel == SystemVersionLevel.Windows_10 && Environment.OSVersion.Version.Build >= 14393))
                ChangeDpi(User32.GetDpiForWindow(Handle));
            Size Size = base.Size;
            float dpiScaleFactor = this.dpiScaleFactor;
            if (dpiScaleFactor != 1.0f && !Size.IsEmpty)
            {
                Size Size2 = Size;
                Size.Width = (int)(Size.Width * dpiScaleFactor);
                Size.Height = (int)(Size.Height * dpiScaleFactor);
                var startPos = StartPosition;
                if (startPos != FormStartPosition.WindowsDefaultLocation && startPos != FormStartPosition.WindowsDefaultBounds)
                {
                    Point loc = Location;
                    loc.X -= (Size.Width - Size2.Width) / 2;
                    loc.Y -= (Size.Height - Size2.Height) / 2;
                    if (loc.X < 0) loc.X = 0;
                    if (loc.Y < 0) loc.Y = 0;
                    if (User32.SetWindowPos(Handle, IntPtr.Zero, loc.X, loc.Y, Size.Width, Size.Height,
                        WindowPositionFlags.SwapWithNoRedraw | WindowPositionFlags.SwapWithNoActivate) != 0)
                    {
                        Location = loc;
                        base.Size = Size;
                    }
                }
            }
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);
            InitRenderObjects();
        }

        #region Thread-Safe Function Overwrite
        private Func<Point, Point>? _pointToClientFunc;
        private Func<bool>? _clipboard_ContainsTextFunc;

        public new bool InvokeRequired => _handle != default && User32.GetWindowThreadProcessId(_handle, null) != _threadId.Value;

        public new PointF PointToClient(Point point)
        {
            if (InvokeRequired)
            {
                if (IsDisposed)
                    return PointF.Empty;
                return ScalingPointF((Point)Invoke((_pointToClientFunc ??= base.PointToClient), point), windowScaleFactor);
            }
            else
            {
                return ScalingPointF(base.PointToClient(point), windowScaleFactor);
            }
        }

        public Point PointToClientBase(Point point)
        {
            if (InvokeRequired)
            {
                if (IsDisposed)
                    return Point.Empty;
                return (Point)Invoke((_pointToClientFunc ??= base.PointToClient), point);
            }
            else
            {
                return base.PointToClient(point);
            }
        }

        public bool Clipboard_ContainsText()
        {
            if (InvokeRequired)
            {
                _clipboard_ContainsTextFunc ??= Clipboard.ContainsText;
#if NET8_0_OR_GREATER
                return Invoke(_clipboard_ContainsTextFunc);
#else
                return (bool)Invoke(_clipboard_ContainsTextFunc);
#endif
            }
            else
            {
                return Clipboard.ContainsText();
            }
        }


#endregion
#endregion

        #region Inline Macros
        [Inline(InlineBehavior.Remove)]
        protected static Point GetPointFromIntPtr(IntPtr pointer)
            => (*(Point16*)&pointer).ToPoint32();

        [Inline(InlineBehavior.Remove)]
        public static Size ScalingSize(SizeF original, float dpiScaleFactor)
        {
            if (dpiScaleFactor == 1.0f)
                return new Size(MathI.Floor(original.Width), MathI.Floor(original.Height));
            else
                return new Size(MathI.Floor(original.Width * dpiScaleFactor), MathI.Floor(original.Height * dpiScaleFactor));
        }

        [Inline(InlineBehavior.Remove)]
        public static SizeF ScalingSizeF(Size original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new SizeF(original.Width * windowScaleFactor, original.Height * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        public static PointF ScalingPointF(Point original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new PointF(original.X * windowScaleFactor, original.Y * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        public static PointF ScalingPointF(PointF original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            else
                return new PointF(original.X * windowScaleFactor, original.Y * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        public static PointF ScalingPointFInRect(Point original, Rect rect, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return new PointF(original.X - rect.Left, original.Y - rect.Top);
            else
                return new PointF((original.X - rect.Left) * windowScaleFactor, (original.Y - rect.Top) * windowScaleFactor);
        }

        [Inline(InlineBehavior.Remove)]
        public static Rect ScalingRect(Rect original, float windowScaleFactor)
        {
            if (windowScaleFactor == 1.0f)
                return original;
            return new Rect(top: MathI.FloorPositive(original.Top * windowScaleFactor),
                left: MathI.FloorPositive(original.Left * windowScaleFactor),
                right: MathI.Ceiling(original.Right * windowScaleFactor),
               bottom: MathI.Ceiling(original.Bottom * windowScaleFactor));
        }
        #endregion
    }
}
