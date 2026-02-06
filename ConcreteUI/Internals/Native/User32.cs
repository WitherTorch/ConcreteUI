using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class User32
    {
        private const string LibraryName = "user32.dll";
        private static readonly void*[] _pointers =
            MethodImportHelper.GetImportedMethodPointers(LibraryName, "GetDpiForWindow");

        public static SysBool TryGetDpiForWindow(IntPtr hWnd, out uint dpiX, out uint dpiY)
        {
            void* pointer = _pointers[0];
            if (pointer != null)
            {
                dpiX = ((delegate* unmanaged
#if NET8_0_OR_GREATER
                    [Stdcall, SuppressGCTransition]
#else
                    [Stdcall]
#endif
                    <IntPtr, uint>)pointer)(hWnd);
                goto ReturnSame;
            }

            IntPtr hMonitor = MonitorFromWindow(hWnd, MonitorFromWindowFlags.DefaultToNearest);
            if (hMonitor == IntPtr.Zero)
                goto Failed;
            int hr = ShCore.GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out dpiX, out dpiY); // Fallback 1 : GetDpiForMonitor (Windows 8.1 or greater)
            if (hr >= 0)
                goto Return;
            if (hr == Constants.E_NOTIMPL)
            {
                IntPtr hdc = GetDC(hWnd);
                if (hdc == IntPtr.Zero)
                    goto Failed;
                try
                {
                    // Fallback 2 : GetDeviceCaps (Vista or greater)
                    const int LOGPIXELSX = 88;
                    const int LOGPIXELSY = 90;
                    dpiX = (uint)Gdi32.GetDeviceCaps(hdc, LOGPIXELSX);
                    dpiY = (uint)Gdi32.GetDeviceCaps(hdc, LOGPIXELSY);
                    goto Return;
                }
                finally
                {
                    ReleaseDC(hWnd, hdc);
                }
            }

        ReturnSame:
            dpiY = dpiX;
            goto Return;

        Return:
            return true;

        Failed:
            dpiX = 0;
            dpiY = 0;
            return false;
        }

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport(LibraryName)]
        public static extern SysBool GetWindowRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(LibraryName)]
        public static extern SysBool GetClientRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(LibraryName)]
        public static extern SysBool ScreenToClient(IntPtr hWnd, Point* lpPoint);

        [DllImport(LibraryName)]
        public static extern SysBool ClientToScreen(IntPtr hWnd, Point* lpPoint);

        [DllImport(LibraryName)]
        public static extern SysBool SetWindowTextW(IntPtr hWnd, char* lpString);

        [DllImport(LibraryName)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);

        [DllImport(LibraryName)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

        [DllImport(LibraryName)]
        public static extern SysBool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);

        [DllImport(LibraryName)]
        public static extern SysBool IsIconic(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern SysBool IsZoomed(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetDoubleClickTime();

        [DllImport(LibraryName)]
        public static extern SysBool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport(LibraryName)]
        public static extern SysBool DestroyCaret();

        [DllImport(LibraryName)]
        public static extern SysBool SetCaretPos(int x, int y);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int GetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int SetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [DllImport(LibraryName)]
        public static extern SysBool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);

        [DllImport(LibraryName)]
        public static extern IntPtr CreateWindowExW(WindowExtendedStyles dwExStyle, char* lpClassName, char* lpWindowName,
            WindowStyles dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, void* lpParam);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern ushort RegisterClassExW(WindowClassEx* windowClass);

        [DllImport(LibraryName)]
        public static extern nint DefWindowProcW(IntPtr hWnd, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern nint DefWindowProcW(IntPtr hWnd, uint msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern SysBool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(LibraryName)]
        public static extern SysBool EnableWindow(IntPtr hWnd, SysBool bEnable);

        [DllImport(LibraryName)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);

        [DllImport(LibraryName)]
        public static extern IntPtr GetActiveWindow();

        [DllImport(LibraryName)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern SysBool IsWindowVisible(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern SysBool DestroyWindow(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern uint RegisterWindowMessageW(char* lpString);

        [DllImport(LibraryName)]
        public static extern nint SendMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern nint SendMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern SysBool PostMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern SysBool PostMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport(LibraryName)]
        public static extern SysBool GetMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(LibraryName)]
        public static extern SysBool PeekMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PeekMessageOptions wRemoveMsg);

        [DllImport(LibraryName)]
        public static extern SysBool TranslateMessage(PumpingMessage* lpMsg);

        [DllImport(LibraryName)]
        public static extern nint DispatchMessageW(PumpingMessage* lpMsg);

        [DllImport(LibraryName)]
        public static extern IntPtr LoadImageW(IntPtr hInstance, char* name, Win32ImageType type, int cx, int cy, LoadOrCopyImageOptions fuLoad);

        [DllImport(LibraryName)]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport(LibraryName)]
        public static extern SysBool DestroyCursor(IntPtr hCursor);

        [DllImport(LibraryName)]
        public static extern SysBool DestroyIcon(IntPtr hIcon);

        [DllImport(LibraryName)]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport(LibraryName)]
        public static extern int GetWindowTextLengthW(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern int GetWindowTextW(IntPtr hWnd, char* lpString, int maxCount);

        [DllImport(LibraryName)]
        public static extern nint GetWindowLongPtrW(IntPtr hWnd, int nIndex);

        [DllImport(LibraryName)]
        public static extern nint SetWindowLongPtrW(IntPtr hWnd, int nIndex, nint dwNewLong);

        [DllImport(LibraryName)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, SysBool bRevert);

        [DllImport(LibraryName)]
        public static extern SysBool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport(LibraryName)]
        public static extern SysBool EmptyClipboard();

        [DllImport(LibraryName)]
        public static extern SysBool CloseClipboard();

        [DllImport(LibraryName)]
        public static extern IntPtr GetClipboardData(ClipboardFormat uFormat);

        [DllImport(LibraryName)]
        public static extern SysBool IsClipboardFormatAvailable(ClipboardFormat format);

        [DllImport(LibraryName)]
        public static extern IntPtr SetClipboardData(ClipboardFormat format, IntPtr hMem);

        [DllImport(LibraryName)]
        public static extern short GetAsyncKeyState(VirtualKey vKey);

        [DllImport(LibraryName)]
        public static extern short GetKeyState(VirtualKey vKey);

        [DllImport(LibraryName)]
        public static extern SysBool GetCursorPos(Point* lpPoint);

        [DllImport(LibraryName)]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorFromWindowFlags dwFlags);

        [DllImport(LibraryName)]
        public static extern SysBool GetMonitorInfoW(IntPtr hMonitor, MonitorInfo* lpmi);

        [DllImport(LibraryName)]
        public static extern SysBool GetUpdateRect(IntPtr hWnd, Rect* lpRect, SysBool bErase);

        [DllImport(LibraryName)]
        public static extern IntPtr BeginPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(LibraryName)]
        public static extern SysBool EndPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(LibraryName)]
        public static extern DialogResult MessageBoxW(IntPtr hWnd, char* lpText, char* lpCaption, MessageBoxFlags uType);

        [DllImport(LibraryName)]
        public static extern SysBool PostThreadMessageW(uint idThread, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern uint MsgWaitForMultipleObjects(uint nCount, IntPtr* pHandles, SysBool fWaitAll, uint dwMilliseconds, QueueStatusFlags dwWakeMask);

        [DllImport(LibraryName)]
        public static extern SysBool PostThreadMessageW(uint idThread, uint msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern SysBool InSendMessage();

        [DllImport(LibraryName)]
        public static extern SysBool ReplyMessage(nint lResult);

        [DllImport(LibraryName)]
        public static extern SysBool SetCapture(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern SysBool ReleaseCapture();

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SysBool SetWindowText(IntPtr handle, string text)
        {
            fixed (char* ptr = text)
                return SetWindowTextW(handle, ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RegisterWindowMessage(string str)
        {
            fixed (char* ptr = str)
                return RegisterWindowMessageW(ptr);
        }

        [Inline(InlineBehavior.Remove)]
        public static int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, WindowPositionFlags flags)
            => SetWindowPos(hWnd, hWndInsertAfter, 0, 0, 0, 0, flags | WindowPositionFlags.SwapWithNoSize | WindowPositionFlags.SwapWithNoMove);

        [Inline(InlineBehavior.Remove)]
        public static int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, Point location, Size size, WindowPositionFlags flags)
            => SetWindowPos(hWnd, hWndInsertAfter, location.X, location.Y, size.Width, size.Height, flags);

        [Inline(InlineBehavior.Remove)]
        public static int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, in Rectangle bounds, WindowPositionFlags flags)
            => SetWindowPos(hWnd, hWndInsertAfter, bounds.X, bounds.Y, bounds.Width, bounds.Height, flags);
    }
}
