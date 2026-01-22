using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Internals;
using ConcreteUI.Native;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Window
{
    partial class NativeWindow
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShowWindow(IntPtr handle, WindowState showState)
            => User32.ShowWindow(handle, showState switch
            {
                WindowState.Normal => ShowWindowCommands.ShowNormal,
                WindowState.Minimized => ShowWindowCommands.ShowMinimized,
                WindowState.Maximized => ShowWindowCommands.ShowMaximized,
                _ => throw new ArgumentOutOfRangeException(nameof(showState))
            });

        private unsafe IntPtr CreateWindowHandle(IntPtr parent)
        {
            WindowClassImpl windowClass = WindowClassImpl.Instance;
            CreateWindowInfo windowInfo = GetCreateWindowInfo();

            IntPtr result = User32.CreateWindowExW(
                lpClassName: (char*)windowClass.Atom,
                lpWindowName: null,
                dwStyle: windowInfo.Styles,
                dwExStyle: windowInfo.ExtendedStyles,
                X: windowInfo.X, Y: windowInfo.Y,
                nWidth: windowInfo.Width, nHeight: windowInfo.Height,
                hWndParent: parent,
                hMenu: IntPtr.Zero,
                hInstance: windowClass.HInstance,
                lpParam: null);
            if (result == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
            return result;
        }

        protected virtual CreateWindowInfo GetCreateWindowInfo()
        {
            const int CW_USEDEFAULT = unchecked((int)0x80000000);
            return new CreateWindowInfo(
                styles: WindowStyles.OverlappedWindow,
                extendedStyles: WindowExtendedStyles.AppWindow | WindowExtendedStyles.WindowEdge,
                x: CW_USEDEFAULT,
                y: CW_USEDEFAULT,
                width: CW_USEDEFAULT,
                height: CW_USEDEFAULT);
        }

        protected virtual void OnHandleCreated(IntPtr handle)
        {
            string text = InterlockedHelper.CompareExchange(ref _cachedText, nameof(NativeWindow), null) ?? nameof(NativeWindow);
            User32.SetWindowText(handle, text);
            Icon? icon = InterlockedHelper.Read(ref _cachedIcon);
            IntPtr iconHandle = icon is null ? IntPtr.Zero : User32.CopyIcon(icon.Handle);
            SetIconCore(handle, iconHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsWindowDestroyed()
            => InterlockedHelper.Read(ref _windowFlags) == UnsafeHelper.GetMaxValue<nuint>();


        protected virtual void DisposeCore(bool disposing)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return;
            User32.PostMessageW(handle, CustomWindowMessages.ConcreteDestroyWindowAsync, 0, 0);
        }

        private void DestroyHandle()
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return;
            User32.DestroyWindow(handle);
        }

        private void Dispose(bool disposing)
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            if (disposing)
                Thread.MemoryBarrier();
            DisposeCore(disposing);
        }

        ~NativeWindow() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
