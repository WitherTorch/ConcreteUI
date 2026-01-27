using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

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

        public static bool TryGetDpiForWindow(IntPtr hWnd, out uint dpiX, out uint dpiY)
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

            IntPtr hMonitor = MonitorFromWindow(hWnd, GetMonitorFlags.DefaultToNearest);
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
        public static extern bool GetWindowRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(LibraryName)]
        public static extern bool GetClientRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(LibraryName)]
        public static extern bool ScreenToClient(IntPtr hWnd, Point* lpPoint);

        [DllImport(LibraryName)]
        public static extern bool SetWindowTextW(IntPtr hWnd, char* lpString);

        [DllImport(LibraryName)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);

        [DllImport(LibraryName)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

        [DllImport(LibraryName)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);

        [DllImport(LibraryName)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetDoubleClickTime();

        [DllImport(LibraryName)]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport(LibraryName)]
        public static extern bool DestroyCaret();

        [DllImport(LibraryName)]
        public static extern bool SetCaretPos(int x, int y);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int GetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int SetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [DllImport(LibraryName)]
        public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);

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
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(LibraryName)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport(LibraryName)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);

        [DllImport(LibraryName)]
        public static extern IntPtr GetActiveWindow();

        [DllImport(LibraryName)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern uint RegisterWindowMessageW(char* lpString);

        [DllImport(LibraryName)]
        public static extern nint SendMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern nint SendMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern bool PostMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern bool PostMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport(LibraryName)]
        public static extern SysBool GetMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(LibraryName)]
        public static extern bool PeekMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PeekMessageOptions wRemoveMsg);

        [DllImport(LibraryName)]
        public static extern SysBool TranslateMessage(PumpingMessage* lpMsg);

        [DllImport(LibraryName)]
        public static extern nint DispatchMessageW(PumpingMessage* lpMsg);

        [DllImport(LibraryName)]
        public static extern IntPtr LoadImageW(IntPtr hInstance, char* name, Win32ImageType type, int cx, int cy, LoadOrCopyImageOptions fuLoad);

        [DllImport(LibraryName)]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport(LibraryName)]
        public static extern bool DestroyCursor(IntPtr hCursor);

        [DllImport(LibraryName)]
        public static extern bool DestroyIcon(IntPtr hIcon);

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
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(LibraryName)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport(LibraryName)]
        public static extern bool EmptyClipboard();

        [DllImport(LibraryName)]
        public static extern bool CloseClipboard();

        [DllImport(LibraryName)]
        public static extern IntPtr GetClipboardData(ClipboardFormat uFormat);

        [DllImport(LibraryName)]
        public static extern bool IsClipboardFormatAvailable(ClipboardFormat format);

        [DllImport(LibraryName)]
        public static extern IntPtr SetClipboardData(ClipboardFormat format, IntPtr hMem);

        [DllImport(LibraryName)]
        public static extern short GetAsyncKeyState(VirtualKey vKey);

        [DllImport(LibraryName)]
        public static extern short GetKeyState(VirtualKey vKey);

        [DllImport(LibraryName)]
        public static extern bool GetCursorPos(Point* lpPoint);

        [DllImport(LibraryName)]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, GetMonitorFlags dwFlags);

        [DllImport(LibraryName)]
        public static extern bool GetMonitorInfoW(IntPtr hMonitor, MonitorInfo* lpmi);

        [DllImport(LibraryName)]
        public static extern bool GetUpdateRect(IntPtr hWnd, Rect* lpRect, bool bErase);

        [DllImport(LibraryName)]
        public static extern IntPtr BeginPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(LibraryName)]
        public static extern bool EndPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(LibraryName)]
        public static extern DialogResult MessageBoxW(IntPtr hWnd, char* lpText, char* lpCaption, MessageBoxFlags uType);

        [DllImport(LibraryName)]
        public static extern bool PostThreadMessageW(uint idThread, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern uint MsgWaitForMultipleObjects(uint nCount, IntPtr* pHandles, bool fWaitAll, uint dwMilliseconds, QueueStatusFlags dwWakeMask);

        [DllImport(LibraryName)]
        public static extern bool PostThreadMessageW(uint idThread, uint msg, nint wParam, nint lParam);

        [DllImport(LibraryName)]
        public static extern bool InSendMessage();

        [DllImport(LibraryName)]
        public static extern bool ReplyMessage(nint lResult);

        [DllImport(LibraryName)]
        public static extern bool SetCapture(IntPtr hWnd);

        [DllImport(LibraryName)]
        public static extern bool ReleaseCapture();

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SetWindowText(IntPtr handle, string text)
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
