using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Internals;
using ConcreteUI.Native;

using InlineIL;

using LocalsInit;

using WitherTorch.Common.Helpers;

using GdiColor = System.Drawing.Color;
using GdiGraphics = System.Drawing.Graphics;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace ConcreteUI.Window
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
            Dictionary<uint, nint> result = new Dictionary<uint, nint>(1);

            IL.EnsureLocal(result);

            IL.Push(result);
            IL.Push(CustomWindowMessages.ConcreteDestroyWindowAsync);
            IL.Emit.Ldftn(new MethodRef(typeof(NativeWindow), nameof(HandleConcreteDestroyWindowAsync)));
            IL.Emit.Call(new MethodRef(typeof(Dictionary<uint, nint>), nameof(Dictionary<uint, nint>.Add)));

            return result;
        }

        bool IWindowMessageFilter.TryProcessWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
            => TryProcessWindowMessage(handle, message, wParam, lParam, out result);

        protected virtual bool TryProcessWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            if (message < WindowMessage.CustomClassMessageStart)
                return TryProcessSystemWindowMessage(handle, message, wParam, lParam, out result);

            if (message >= WindowMessage.RegisterWindowMessageStart && message <= WindowMessage.RegisterWindowMessageEnd)
                return TryProcessCustomWindowMessage(handle, (uint)message, wParam, lParam, out result);

            return TryProcessOtherWindowMessage(handle, (uint)message, wParam, lParam, out result);
        }

        protected virtual bool TryProcessSystemWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
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
                WindowMessage.ShowWindow => HandleShowWindow(wParam: wParam, lParam: lParam),
                _ => false,
            };
        }

        [LocalsInit(false)]
        protected virtual bool TryProcessCustomWindowMessage(IntPtr handle, uint message, nint wParam, nint lParam, out nint result)
        {
            const string Label = "JumpLabel";

            if (_customWindowMessageProcessorDict.TryGetValue(message, out nint functionPointer))
            {
                IL.Emit.Ldarg_0();
                IL.Emit.Ldarg_2();
                IL.Emit.Ldarg_3();
                IL.Emit.Ldarg(4);
                IL.PushOutRef(out result);
                IL.Push(functionPointer);
                IL.Emit.Calli(new StandAloneMethodSig(CallingConventions.HasThis, typeof(bool),
                    typeof(IntPtr), typeof(nint), typeof(nint), TypeRef.Type<nint>().MakeByRefType()));

                IL.Emit.Brfalse(Label);
                return true;
            }

            IL.MarkLabel(Label);
            result = 0;
            return false;
        }

        [LocalsInit(false)]
        protected virtual bool TryProcessOtherWindowMessage(IntPtr handle, uint message, nint wParam, nint lParam, out nint result)
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
                    if ((InterlockedHelper.And(ref _windowFlags, ~(nuint)0b100) & 0b100) == 0b100)
                        OnFocusedChanged(EventArgs.Empty);
                    break;
                case 1: // WA_ACTIVE
                case 2: // WA_CLICKACTIVE
                    if ((InterlockedHelper.Or(ref _windowFlags, 0b100) & 0b100) != 0b100)
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
            if (InterlockedHelper.Exchange(ref _windowFlags, UnsafeHelper.GetMaxValue<nuint>()) != UnsafeHelper.GetMaxValue<nuint>())
            {
                IntPtr handle = _handleLazy.Value;
                if (handle == IntPtr.Zero)
                    return true;
                if (!WindowClassImpl.Instance.UnregisterWindowUnsafe(handle, this))
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
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (uint)WindowState.Maximized);
                        if (oldState != WindowState.Maximized)
                            OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Maximized));
                    }
                    break;
                case 1: // SIZE_MINIMIZED
                    {
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (uint)WindowState.Minimized);
                        if (oldState != WindowState.Minimized)
                            OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Minimized));
                    }
                    break;
                case 0: // SIZE_RESTORED
                    {
                        WindowState oldState = (WindowState)InterlockedHelper.Exchange(ref _windowState, (uint)WindowState.Normal);
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

        private bool HandleShowWindow(nint wParam, nint lParam)
        {
            if (wParam != 0 && lParam == 0 && (InterlockedHelper.Or(ref _windowFlags, 0b10) & 0b10) != 0b10)
                WindowMessageLoop.InvokeAsync(() => OnShown(EventArgs.Empty));
            return false;
        }

        private static bool HandleEraseBackground(out nint result)
        {
            result = 1;
            return true;
        }

        private bool HandleConcreteDestroyWindowAsync(IntPtr hwnd, nint wParam, nint lParam, out nint result)
        {
            DestroyHandle();
            result = 0;
            return true;
        }
    }
}
