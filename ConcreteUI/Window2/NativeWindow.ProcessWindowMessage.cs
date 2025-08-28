using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Native;

using LocalsInit;

using WitherTorch.Common.Helpers;

using GdiGraphics = System.Drawing.Graphics;
using GdiColor = System.Drawing.Color;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace ConcreteUI.Window2
{
    unsafe partial class NativeWindow
    {
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<uint, nint> _customWindowMessageProcessorDict
            = CreateCustomWindowMessageProcessorDictionary().ToFrozenDictionary();
#else
        private static readonly Dictionary<uint, nint> _customWindowMessageProcessorDict
            = CreateCustomWindowMessageProcessorDictionary();
#endif

        private static Dictionary<uint, nint> CreateCustomWindowMessageProcessorDictionary()
        {
            return new Dictionary<uint, nint>()
            {
                {
                    CustomWindowMessages.ConcreteWindowInvoke,
                    (nint)(delegate* managed<NativeWindow, nint, nint, out nint, bool>)&HandleConcreteWindowInvoke
                },
                {
                    CustomWindowMessages.ConcreteDestroyWindowAsync,
                    (nint)(delegate* managed<NativeWindow, nint, nint, out nint, bool>)&HandleConcreteDestroyWindowAsync
                },
            };
        }

        bool IWindowMessageFilter.TryProcessWindowMessage(WindowMessage message, nint wParam, nint lParam, out nint result)
            => TryProcessWindowMessage(message, wParam, lParam, out result);

        protected virtual bool TryProcessWindowMessage(WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            if (message < WindowMessage.CustomClassMessageStart)
                return TryProcessSystemWindowMessage(message, wParam, lParam, out result);

            if (message >= WindowMessage.RegisterWindowMessageStart && message <= WindowMessage.RegisterWindowMessageEnd)
                return TryProcessCustomWindowMessage((uint)message, wParam, lParam, out result);

            return TryProcessOtherWindowMessage((uint)message, wParam, lParam, out result);
        }

        protected virtual bool TryProcessSystemWindowMessage(WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            result = 0;
            return message switch
            {
                WindowMessage.Activate => HandleActivate(wParam: wParam),
                WindowMessage.Close => HandleClose(),
                WindowMessage.Destroy => HandleDestroyed(),
                WindowMessage.NCLeftButtonDown => HandleNCLeftButtonDown(wParam: wParam),
                WindowMessage.NCLeftButtonUp => HandleNCLeftButtonUp(wParam: wParam),
                WindowMessage.SetText => HandleSetText(),
                WindowMessage.SetIcon => HandleSetIcon(),
                WindowMessage.SetCursor => HandleSetCursor(lParam: lParam),
                WindowMessage.WindowPositionChanging => HandleWindowPositionChanging(),
                WindowMessage.Sizing => HandleSizing(),
                WindowMessage.Size => HandleSize(wParam),
                WindowMessage.Paint => HandlePaint(),
                WindowMessage.EraseBackground => HandleEraseBackground(out result),
                _ => false,
            };
        }

        [LocalsInit(false)]
        protected virtual bool TryProcessCustomWindowMessage(uint message, nint wParam, nint lParam, out nint result)
        {
            if (_customWindowMessageProcessorDict.TryGetValue(message, out nint functionPointer) &&
                ((delegate* managed<NativeWindow, nint, nint, out nint, bool>)functionPointer)(this, wParam, lParam, out result))
                return true;

            result = 0;
            if (message == CustomWindowMessages.ConcreteWindowInvoke)
            {

                DestroyHandle();
                return true;
            }
            return false;
        }

        [LocalsInit(false)]
        protected virtual bool TryProcessOtherWindowMessage(uint message, nint wParam, nint lParam, out nint result)
        {
            result = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleActivate(nint wParam)
        {
            switch (wParam)
            {
                case 0: // WA_INACTIVE
                    if ((InterlockedHelper.And(ref _windowState, ~(nuint)0b01) & 0b01) == 0b01)
                        OnFocusedChanged(EventArgs.Empty);
                    break;
                case 1: // WA_ACTIVE
                case 2: // WA_CLICKACTIVE
                    if ((InterlockedHelper.Or(ref _windowState, 0b01) & 0b01) != 0b01)
                        OnFocusedChanged(EventArgs.Empty);
                    break;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleClose()
        {
            ClosingEventArgs args = new ClosingEventArgs((CloseReason)InterlockedHelper.Exchange(ref _closeReason, (uint)CloseReason.Unknown), cancelled: false);
            OnClosing(ref args);
            return args.Cancelled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleDestroyed()
        {
            if (InterlockedHelper.Exchange(ref _windowState, UnsafeHelper.GetMaxValue<nuint>()) != UnsafeHelper.GetMaxValue<nuint>())
            {
                if (!WindowClassImpl.Instance.UnregisterWindow(_handleLazy.Value, this))
                    DebugHelper.Throw();
                OnDestroyed(EventArgs.Empty);
            }
            return true;
        }

        private static bool HandleNCLeftButtonDown(nint wParam)
            => (HitTestValue)wParam switch
            {
                HitTestValue.MinimizeButton or HitTestValue.MaximizeButton or HitTestValue.CloseButton => true,
                _ => false,
            };

        private bool HandleNCLeftButtonUp(nint wParam)
        {
            HitTestValue state = (HitTestValue)wParam;
            switch (state)
            {
                case HitTestValue.MinimizeButton:
                    WindowState = WindowState.Minimized;
                    return true;
                case HitTestValue.MaximizeButton:
                    if (WindowState == WindowState.Maximized)
                        WindowState = WindowState.Normal;
                    else
                        WindowState = WindowState.Maximized;
                    return true;
                case HitTestValue.CloseButton:
                    Close(CloseReason.UserClicked);
                    return true;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleSetText()
        {
            InterlockedHelper.Exchange(ref _cachedText, null);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleSetIcon()
        {
            InterlockedHelper.Exchange(ref _cachedIcon, null);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleSetCursor(nint lParam)
        {
            switch ((HitTestValue)(ushort)lParam)
            {
                case HitTestValue.Client or HitTestValue.NoWhere:
                    IntPtr oldHandle = User32.SetCursor(_cursor.Handle);
                    if (oldHandle != IntPtr.Zero)
                        User32.DestroyCursor(oldHandle);
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleWindowPositionChanging()
        {
            _cachedBounds = default;
            Thread.MemoryBarrier();
            return false;
        }

        private bool HandleSizing()
        {
            OnResizing(EventArgs.Empty);
            return false;
        }

        private bool HandleSize(nint wParam)
        {
            switch (wParam)
            {
                case 2: // SIZE_MAXIMIZED
                    {
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (nuint)WindowState.Maximized);
                        if (oldState != WindowState.Maximized)
                            OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Maximized));
                    }
                    break;
                case 1: // SIZE_MINIMIZED
                    {
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (nuint)WindowState.Minimized);
                        if (oldState != WindowState.Minimized)
                            OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Minimized));
                    }
                    break;
                case 0: // SIZE_RESTORED
                    {
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (nuint)WindowState.Normal);
                        if (oldState != WindowState.Normal)
                            OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Normal));
                    }
                    break;
                default:
                    break;
            }
            OnResized(EventArgs.Empty);
            return false;
        }

        [LocalsInit(false)]
        private bool HandlePaint()
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return true;
            PaintStruct paintStruct;
            IntPtr hdc = User32.BeginPaint(handle, &paintStruct);
            if (hdc == IntPtr.Zero)
                return true;
            using GdiGraphics graphics = GdiGraphics.FromHdc(hdc);
            graphics.Clear(GdiColor.Black);
            User32.EndPaint(handle, &paintStruct);
            return true;
        }

        private static bool HandleEraseBackground(out nint result)
        {
            result = 1;
            return true;
        }

        private static bool HandleConcreteWindowInvoke(NativeWindow window, nint wParam, nint lParam, out nint result)
        {
            ConcurrentBag<InvokeClosure> invokeClosureBag = window._invokeClosureBag;
            while (invokeClosureBag.TryTake(out InvokeClosure? closure))
                closure.Invoke();
            result = 0;
            return true;
        }

        private static bool HandleConcreteDestroyWindowAsync(NativeWindow window, nint wParam, nint lParam, out nint result)
        {
            window.DestroyHandle();
            result = 0;
            return true;
        }
    }
}
