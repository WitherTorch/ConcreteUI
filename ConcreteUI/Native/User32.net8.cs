#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class User32
    {
        private const string USER32_DLL = "user32.dll";

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial int GetSystemMetrics(SystemMetric smIndex);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetWindowRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetClientRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(USER32_DLL)]
        public static extern partial bool ScreenToClient(IntPtr hWnd, Point* lpPoint);

        [DllImport(USER32_DLL)]
        public static extern partial bool SetWindowTextW(IntPtr hWnd, char* lpString);

        [DllImport(USER32_DLL)]
        public static extern partial int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);

        [DllImport(USER32_DLL)]
        public static extern partial int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);

        [DllImport(USER32_DLL)]
        public static extern partial bool IsIconic(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial bool IsZoomed(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetKeyboardLayout(uint idThread);

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial uint GetDoubleClickTime();

        [DllImport(USER32_DLL)]
        public static extern partial bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport(USER32_DLL)]
        public static extern partial bool DestroyCaret();

        [DllImport(USER32_DLL)]
        public static extern partial bool SetCaretPos(int x, int y);

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial int GetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial int SetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [DllImport(USER32_DLL)]
        public static extern partial bool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr CreateWindowExW(WindowExtendedStyles dwExStyle, char* lpClassName, char* lpWindowName,
            WindowStyles dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, void* lpParam);

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial ushort RegisterClassExW(WindowClassEx* windowClass);

        [DllImport(USER32_DLL)]
        public static extern partial nint DefWindowProcW(IntPtr hWnd, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial nint DefWindowProcW(IntPtr hWnd, uint msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(USER32_DLL)]
        public static extern partial bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetActiveWindow();

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial bool IsWindowVisible(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial bool DestroyWindow(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial uint RegisterWindowMessageW(char* lpString);

        [DllImport(USER32_DLL)]
        public static extern partial nint SendMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial nint SendMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial bool PostMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial bool PostMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial void PostQuitMessage(int nExitCode);

        [DllImport(USER32_DLL)]
        public static extern partial SysBool GetMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(USER32_DLL)]
        public static extern partial bool PeekMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PeekMessageOptions wRemoveMsg);

        [DllImport(USER32_DLL)]
        public static extern partial SysBool TranslateMessage(PumpingMessage* lpMsg);

        [DllImport(USER32_DLL)]
        public static extern partial nint DispatchMessageW(PumpingMessage* lpMsg);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr LoadImageW(IntPtr hInstance, char* name, Win32ImageType type, int cx, int cy, LoadOrCopyImageOptions fuLoad);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr CopyIcon(IntPtr hIcon);

        [DllImport(USER32_DLL)]
        public static extern partial bool DestroyCursor(IntPtr hCursor);

        [DllImport(USER32_DLL)]
        public static extern partial bool DestroyIcon(IntPtr hIcon);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr SetCursor(IntPtr hCursor);

        [DllImport(USER32_DLL)]
        public static extern partial int GetWindowTextLengthW(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial int GetWindowTextW(IntPtr hWnd, char* lpString, int maxCount);

        [DllImport(USER32_DLL)]
        public static extern partial nint GetWindowLongPtrW(IntPtr hWnd, int nIndex);

        [DllImport(USER32_DLL)]
        public static extern partial nint SetWindowLongPtrW(IntPtr hWnd, int nIndex, nint dwNewLong);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(USER32_DLL)]
        public static extern partial bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport(USER32_DLL)]
        public static extern partial bool EmptyClipboard();

        [DllImport(USER32_DLL)]
        public static extern partial bool CloseClipboard();

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetClipboardData(ClipboardFormat uFormat);

        [DllImport(USER32_DLL)]
        public static extern partial bool IsClipboardFormatAvailable(ClipboardFormat format);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr SetClipboardData(ClipboardFormat format, IntPtr hMem);

        [DllImport(USER32_DLL)]
        public static extern partial short GetAsyncKeyState(VirtualKey vKey);

        [DllImport(USER32_DLL)]
        public static extern partial short GetKeyState(VirtualKey vKey);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetCursorPos(Point* lpPoint);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr MonitorFromWindow(IntPtr hWnd, GetMonitorFlags dwFlags);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetMonitorInfoW(IntPtr hMonitor, MonitorInfo* lpmi);

        [DllImport(USER32_DLL)]
        public static extern partial bool GetUpdateRect(IntPtr hWnd, Rect* lpRect, bool bErase);

        [DllImport(USER32_DLL)]
        public static extern partial IntPtr BeginPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(USER32_DLL)]
        public static extern partial bool EndPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(USER32_DLL)]
        public static extern partial DialogResult MessageBoxW(IntPtr hWnd, char* lpText, char* lpCaption, MessageBoxFlags uType);

        [DllImport(USER32_DLL)]
        public static extern partial bool PostThreadMessageW(uint idThread, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial uint MsgWaitForMultipleObjects(uint nCount, IntPtr* pHandles, bool fWaitAll, uint dwMilliseconds, QueueStatusFlags dwWakeMask);

        [DllImport(USER32_DLL)]
        public static extern partial bool PostThreadMessageW(uint idThread, uint msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern partial bool InSendMessage();

        [DllImport(USER32_DLL)]
        public static extern partial bool ReplyMessage(nint lResult);

        [DllImport(USER32_DLL)]
        public static extern partial bool SetCapture(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern partial bool ReleaseCapture();

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial IntPtr GetDC(IntPtr hWnd);

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }
}
#endif