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

namespace ConcreteUI.Native
{
    internal static unsafe partial class User32
    {
        private static readonly void*[] _pointers =
            MethodImportHelper.GetImportedMethodPointers(USER32_DLL, "GetDpiForWindow");

#if NET8_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [SuppressUnmanagedCodeSecurity]
        public static bool TryGetDpiForWindow(IntPtr hWnd, out uint dpiX, out uint dpiY)
        {
            void* pointer = _pointers[0];
            if (pointer != null)
            {
                dpiX = ((delegate* unmanaged[Stdcall]<IntPtr, uint>)pointer)(hWnd);
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

        public static partial int GetSystemMetrics(SystemMetric smIndex);
        public static partial bool GetWindowRect(IntPtr hWnd, Rect* lpRect);
        public static partial bool GetClientRect(IntPtr hWnd, Rect* lpRect);
        public static partial bool ScreenToClient(IntPtr hWnd, Point* lpPoint);
        public static partial bool SetWindowTextW(IntPtr hWnd, char* lpString);
        public static partial int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);
        public static partial int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);
        public static partial bool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);
        public static partial bool IsIconic(IntPtr hWnd);
        public static partial bool IsZoomed(IntPtr hWnd);
        public static partial IntPtr GetKeyboardLayout(uint idThread);
        public static partial uint GetDoubleClickTime();
        public static partial bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        public static partial bool DestroyCaret();
        public static partial bool SetCaretPos(int x, int y);
        public static partial int GetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);
        public static partial int SetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);
        public static partial bool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);
        public static partial IntPtr CreateWindowExW(WindowExtendedStyles dwExStyle, char* lpClassName, char* lpWindowName,
            WindowStyles dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, void* lpParam);
        public static partial ushort RegisterClassExW(WindowClassEx* windowClass);
        public static partial nint DefWindowProcW(IntPtr hWnd, WindowMessage msg, nint wParam, nint lParam);
        public static partial nint DefWindowProcW(IntPtr hWnd, uint msg, nint wParam, nint lParam);
        public static partial bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);
        public static partial bool EnableWindow(IntPtr hWnd, bool bEnable);
        public static partial IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);
        public static partial IntPtr GetActiveWindow();
        public static partial IntPtr SetActiveWindow(IntPtr hWnd);
        public static partial bool IsWindowVisible(IntPtr hWnd);
        public static partial bool DestroyWindow(IntPtr hWnd);
        public static partial uint RegisterWindowMessageW(char* lpString);
        public static partial nint SendMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);
        public static partial nint SendMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);
        public static partial bool PostMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);
        public static partial bool PostMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);
        public static partial void PostQuitMessage(int nExitCode);
        public static partial SysBool GetMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        public static partial bool PeekMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PeekMessageOptions wRemoveMsg);
        public static partial SysBool TranslateMessage(PumpingMessage* lpMsg);
        public static partial nint DispatchMessageW(PumpingMessage* lpMsg);
        public static partial IntPtr LoadImageW(IntPtr hInstance, char* name, Win32ImageType type, int cx, int cy, LoadOrCopyImageOptions fuLoad);
        public static partial IntPtr CopyIcon(IntPtr hIcon);
        public static partial bool DestroyCursor(IntPtr hCursor);
        public static partial bool DestroyIcon(IntPtr hIcon);
        public static partial IntPtr SetCursor(IntPtr hCursor);
        public static partial int GetWindowTextLengthW(IntPtr hWnd);
        public static partial int GetWindowTextW(IntPtr hWnd, char* lpString, int maxCount);
        public static partial nint GetWindowLongPtrW(IntPtr hWnd, int nIndex);
        public static partial nint SetWindowLongPtrW(IntPtr hWnd, int nIndex, nint dwNewLong);
        public static partial IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        public static partial bool OpenClipboard(IntPtr hWndNewOwner);
        public static partial bool EmptyClipboard();
        public static partial bool CloseClipboard();
        public static partial IntPtr GetClipboardData(ClipboardFormat uFormat);
        public static partial bool IsClipboardFormatAvailable(ClipboardFormat format);
        public static partial IntPtr SetClipboardData(ClipboardFormat format, IntPtr hMem);
        public static partial short GetAsyncKeyState(VirtualKey vKey);
        public static partial short GetKeyState(VirtualKey vKey);
        public static partial bool GetCursorPos(Point* lpPoint);
        public static partial IntPtr MonitorFromWindow(IntPtr hWnd, GetMonitorFlags dwFlags);
        public static partial bool GetMonitorInfoW(IntPtr hMonitor, MonitorInfo* lpmi);
        public static partial bool GetUpdateRect(IntPtr hWnd, Rect* lpRect, bool bErase);
        public static partial IntPtr BeginPaint(IntPtr hWnd, PaintStruct* lpPaint);
        public static partial bool EndPaint(IntPtr hWnd, PaintStruct* lpPaint);
        public static partial DialogResult MessageBoxW(IntPtr hWnd, char* lpText, char* lpCaption, MessageBoxFlags uType);
        public static partial bool PostThreadMessageW(uint idThread, WindowMessage msg, nint wParam, nint lParam);
        public static partial uint MsgWaitForMultipleObjects(uint nCount, IntPtr* pHandles, bool fWaitAll, uint dwMilliseconds, QueueStatusFlags dwWakeMask);
        public static partial bool PostThreadMessageW(uint idThread, uint msg, nint wParam, nint lParam);
        public static partial bool InSendMessage();
        public static partial bool ReplyMessage(nint lResult);
        public static partial bool SetCapture(IntPtr hWnd);
        public static partial bool ReleaseCapture();
        public static partial IntPtr GetDC(IntPtr hWnd);
        public static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        public static partial bool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);

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
