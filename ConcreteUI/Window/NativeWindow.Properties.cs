using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;
using ConcreteUI.Utils;

using LocalsInit;

using WitherTorch.Common;
using WitherTorch.Common.Buffers;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window
{
    partial class NativeWindow
    {
        public DialogResult DialogResult
        {
            get => (DialogResult)InterlockedHelper.Read(ref _dialogResult);
            set => InterlockedHelper.Exchange(ref _dialogResult, (uint)value);
        }

        public unsafe Rectangle Bounds
        {
            get
            {
                Thread.MemoryBarrier();
                Rectangle cachedBounds = _cachedBounds;
                if (cachedBounds.IsEmpty)
                {
                    IntPtr handle = Handle;
                    if (handle == IntPtr.Zero)
                        return Rectangle.Empty;
                    Rect rect;
                    if (!User32.GetWindowRect(handle, &rect))
                        return Rectangle.Empty;
                    _cachedBounds = cachedBounds = (Rectangle)rect;
                    Thread.MemoryBarrier();
                }
                return cachedBounds;
            }
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                _cachedBounds = value;
                Thread.MemoryBarrier();
                User32.SetWindowPos(handle, IntPtr.Zero, value,
                    WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithNoActivate);
            }
        }

        public Point Location => Bounds.Location;

        public Size Size => Bounds.Size;

        public int X => Bounds.X;

        public int Y => Bounds.Y;

        public int Width => Bounds.Width;

        public int Height => Bounds.Height;

        public unsafe Rectangle ClientBounds
        {
            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return Rectangle.Empty;
                Rect clientBounds;
                if (!User32.GetClientRect(handle, &clientBounds))
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                return (Rectangle)clientBounds;
            }
        }

        public unsafe Size ClientSize
        {
            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return Size.Empty;
                Rect clientBounds;
                if (!User32.GetClientRect(handle, &clientBounds))
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                return clientBounds.Size;
            }
        }

        public bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _disposed;
        }

        public IntPtr Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsWindowDestroyed())
                    return IntPtr.Zero;
                Lazy<IntPtr> handleLazy = _handleLazy;
                if (!handleLazy.IsValueCreated)
                    return IntPtr.Zero;
                return handleLazy.Value;
            }
        }

        public Icon? Icon
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Icon? cachedIcon = InterlockedHelper.Read(ref _cachedIcon);
                if (cachedIcon is null)
                {
                    IntPtr handle = Handle;
                    if (handle == IntPtr.Zero)
                        return null;
                    cachedIcon = GetIconCore(handle);
                    Interlocked.CompareExchange(ref _cachedIcon, cachedIcon, null);
                }
                return cachedIcon;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                {
                    Interlocked.Exchange(ref _cachedIcon, value);
                    return;
                }
                IntPtr iconHandle = value is null ? IntPtr.Zero : User32.CopyIcon(value.Handle);
                WindowMessageLoop.InvokeAsync(() => SetIconCore(handle, iconHandle));
            }
        }

        public Win32ImageHandle Cursor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr oldHandle = User32.SetCursor(value.Handle);
                if (oldHandle == IntPtr.Zero)
                    return;
                _cursor = value;
                User32.DestroyCursor(oldHandle);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                string? text = InterlockedHelper.Read(ref _cachedText);
                if (text is not null)
                    return text;
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return string.Empty;
                text = GetTextCore(handle);
                string? newCachedText = InterlockedHelper.CompareExchange(ref _cachedText, text, null);
                return newCachedText ?? text;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                {
                    InterlockedHelper.Exchange(ref _cachedText, value);
                    return;
                }
                User32.SetWindowText(handle, value);
            }
        }

        public unsafe WindowState WindowState
        {
            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    goto Failed;

                if (User32.IsIconic(handle))
                    return WindowState.Minimized;
                if (User32.IsZoomed(handle))
                    return WindowState.Maximized;

                Failed:
                return WindowState.Normal;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                ShowWindow(handle, value);
            }
        }

        public bool Focused
        {
            get => (InterlockedHelper.Read(ref _windowFlags) & 0b100) == 0b100;
        }

        public WindowStyles Styles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return WindowStyles.None;

                const int GWL_STYLE = -16;
                return (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                const int GWL_STYLE = -16;
                User32.SetWindowLongPtrW(handle, GWL_STYLE, (nint)value);
            }
        }

        public WindowExtendedStyles ExtendedStyles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return WindowExtendedStyles.None;

                const int GWL_EXSTYLE = -20;
                return (WindowExtendedStyles)User32.GetWindowLongPtrW(handle, GWL_EXSTYLE);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                const int GWL_EXSTYLE = -20;
                User32.SetWindowLongPtrW(handle, GWL_EXSTYLE, (nint)value);
            }
        }

        public bool HasSizableBorder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ExtendedStyles & WindowExtendedStyles.WindowEdge) == WindowExtendedStyles.WindowEdge;
        }

        private static unsafe Icon? GetIconCore(IntPtr handle)
        {
            const nint ICON_BIG = 1;
            IntPtr iconHandle = User32.SendMessageW(handle, WindowMessage.GetIcon, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                return null;
            iconHandle = User32.CopyIcon(iconHandle);
            if (iconHandle == IntPtr.Zero)
                return null;
            return Icon.FromHandle(iconHandle);
        }

        private static unsafe void SetIconCore(IntPtr handle, IntPtr iconHandle)
        {
            const nint ICON_BIG = 1;
            iconHandle = User32.SendMessageW(handle, WindowMessage.SetIcon, ICON_BIG, iconHandle);
            if (iconHandle == IntPtr.Zero)
                return;
            User32.DestroyIcon(iconHandle);
        }

        private static unsafe string GetTextCore(IntPtr handle)
        {
            int stringLength = User32.GetWindowTextLengthW(handle);
            if (stringLength <= 0)
                return string.Empty;
            string result = StringHelper.AllocateRawString(stringLength);
            fixed (char* ptr = result)
            {
                int charsWritten = User32.GetWindowTextW(handle, ptr, stringLength + 1);
                if (charsWritten == stringLength)
                    return result;
                if (charsWritten < stringLength)
                    return new string(ptr, 0, charsWritten);
                return GetTextCoreSlow(handle, charsWritten);
            }
        }

        private static unsafe string GetTextCoreSlow(IntPtr handle, int predictedLength)
        {
            ArrayPool<char> pool = ArrayPool<char>.Shared;
            char[] buffer;
            do
            {
                buffer = pool.Rent(predictedLength);
                fixed (char* ptr = buffer)
                {
                    int charsWritten = User32.GetWindowTextW(handle, ptr, buffer.Length);
                    if (charsWritten < buffer.Length)
                    {
                        try
                        {
                            return new string(ptr, 0, charsWritten);
                        }
                        finally
                        {
                            pool.Return(buffer);
                        }
                    }
                }
                predictedLength = buffer.Length + 1;
            } while (predictedLength <= Limits.MaxStringLength);
            try
            {
                return new string(buffer, 0, buffer.Length);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
