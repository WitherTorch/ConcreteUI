using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Windows.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class User32
    {
        private const string USER32_DLL = "user32.dll";
        private static readonly void*[] _pointers = MethodImportHelper.GetImportedMethodPointers(USER32_DLL,
            nameof(GetDpiForWindow));

        public static uint GetDpiForWindow(IntPtr hWnd)
        {
            void* pointer = _pointers[0];
            if (pointer == null)
            {
                IntPtr hMonitor = MonitorFromWindow(hWnd, GetMonitorFlags.DefaultToNearest);
                if (hMonitor == IntPtr.Zero)
                    return 0;
                int hr = ShCore.GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint dpi, out _); // Fallback 1 : GetDpiForMonitor (Windows 8.1 or greater)
                if (hr >= 0)
                    return dpi;
                if (hr == Constants.E_NOTIMPL)
                {
                    IntPtr hdc = Gdi32.GetDC(hWnd);
                    if (hdc == IntPtr.Zero)
                        return 0;
                    try
                    {
                        const int LOGPIXELSX = 88;
                        return (uint)Gdi32.GetDeviceCaps(hdc, LOGPIXELSX); // Fallback 2 : GetDeviceCaps (Vista or greater)
                    }
                    finally
                    {
                        Gdi32.ReleaseDC(hWnd, hdc);
                    }
                }
                return 0;
            }
            return ((delegate* unmanaged[Stdcall]<IntPtr, uint>)pointer)(hWnd);
        }

        [DllImport(USER32_DLL)]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport(USER32_DLL)]
        public static extern bool GetWindowRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(USER32_DLL)]
        public static extern bool GetClientRect(IntPtr hWnd, Rect* lpRect);

        [DllImport(USER32_DLL)]
        public static extern bool ScreenToClient(IntPtr hWnd, Point* lpPoint);  

        [DllImport(USER32_DLL)]
        public static extern bool SetWindowTextW(IntPtr hWnd, char* lpString);

        [DllImport(USER32_DLL)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

        [DllImport(USER32_DLL)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);

        [DllImport(USER32_DLL)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport(USER32_DLL)]
        public static extern uint GetDoubleClickTime();

        [DllImport(USER32_DLL)]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport(USER32_DLL)]
        public static extern bool DestroyCaret();

        [DllImport(USER32_DLL)]
        public static extern bool SetCaretPos(int x, int y);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [DllImport(USER32_DLL)]
        public static extern int SetWindowCompositionAttribute(IntPtr hWnd, WindowCompositionAttributeData* data);

        [DllImport(USER32_DLL)]
        public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);

        [DllImport(USER32_DLL)]
        public static extern IntPtr CreateWindowExW(WindowExtendedStyles dwExStyle, char* lpClassName, char* lpWindowName,
            WindowStyles dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, void* lpParam);

        [DllImport(USER32_DLL)]
        public static extern ushort RegisterClassExW(WindowClassEx* windowClass);

        [DllImport(USER32_DLL)]
        public static extern nint DefWindowProcW(IntPtr hWnd, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern nint DefWindowProcW(IntPtr hWnd, uint msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(USER32_DLL)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetActiveWindow();

        [DllImport(USER32_DLL)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern uint RegisterWindowMessageW(char* lpString);

        [DllImport(USER32_DLL)]
        public static extern nint SendMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern nint SendMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern bool PostMessageW(IntPtr hWnd, WindowMessage message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern bool PostMessageW(IntPtr hWnd, uint message, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport(USER32_DLL)]
        public static extern SysBool GetMessageW(PumpingMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(USER32_DLL)]
        public static extern SysBool TranslateMessage(PumpingMessage* lpMsg);

        [DllImport(USER32_DLL)]
        public static extern nint DispatchMessageW(PumpingMessage* lpMsg);

        [DllImport(USER32_DLL)]
        public static extern IntPtr LoadImageW(IntPtr hInstance, char* name, Win32ImageType type, int cx, int cy, LoadOrCopyImageOptions fuLoad);

        [DllImport(USER32_DLL)]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport(USER32_DLL)]
        public static extern bool DestroyCursor(IntPtr hCursor);

        [DllImport(USER32_DLL)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport(USER32_DLL)]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowTextLengthW(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowTextW(IntPtr hWnd, char* lpString, int maxCount);

        [DllImport(USER32_DLL)]
        public static extern nint GetWindowLongPtrW(IntPtr hWnd, int nIndex);

        [DllImport(USER32_DLL)]
        public static extern nint SetWindowLongPtrW(IntPtr hWnd, int nIndex, nint dwNewLong);

        [DllImport(USER32_DLL)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport(USER32_DLL)]
        public static extern bool EmptyClipboard();

        [DllImport(USER32_DLL)]
        public static extern bool CloseClipboard();

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetClipboardData(ClipboardFormat uFormat);

        [DllImport(USER32_DLL)]
        public static extern bool IsClipboardFormatAvailable(ClipboardFormat format);

        [DllImport(USER32_DLL)]
        public static extern IntPtr SetClipboardData(ClipboardFormat format, IntPtr hMem);

        [DllImport(USER32_DLL)]
        public static extern short GetAsyncKeyState(VirtualKey vKey);

        [DllImport(USER32_DLL)]
        public static extern short GetKeyState(VirtualKey vKey);

        [DllImport(USER32_DLL)]
        public static extern bool GetCursorPos(Point* lpPoint);

        [DllImport(USER32_DLL)]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, GetMonitorFlags dwFlags);

        [DllImport(USER32_DLL)]
        public static extern bool GetMonitorInfoW(IntPtr hMonitor, MonitorInfo* lpmi);

        [DllImport(USER32_DLL)]
        public static extern bool GetUpdateRect(IntPtr hWnd, Rect* lpRect, bool bErase);

        [DllImport(USER32_DLL)]
        public static extern IntPtr BeginPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(USER32_DLL)]
        public static extern bool EndPaint(IntPtr hWnd, PaintStruct* lpPaint);

        [DllImport(USER32_DLL)]
        public static extern DialogResult MessageBoxW(IntPtr hWnd, char* lpText, char* lpCaption, MessageBoxFlags uType);

        [DllImport(USER32_DLL)]
        public static extern bool PostThreadMessageW(uint idThread, WindowMessage msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern bool PostThreadMessageW(uint idThread, uint msg, nint wParam, nint lParam);

        [DllImport(USER32_DLL)]
        public static extern bool InSendMessage();

        [DllImport(USER32_DLL)]
        public static extern bool ReplyMessage(nint lResult);

        [DllImport(USER32_DLL)]
        public static extern int GetLastError();

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
