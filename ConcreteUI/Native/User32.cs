using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class User32
    {
        private const string USER32_DLL = "user32.dll";

        [DllImport(USER32_DLL)]
        public static extern int GetDpiForWindow(IntPtr hwnd);

        [DllImport(USER32_DLL)]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport(USER32_DLL)]
        public static extern bool GetWindowRect(IntPtr hwnd, Rect* lpRect);

        [DllImport(USER32_DLL)]
        public static extern bool SetWindowTextW(IntPtr hWnd, char* lpString);

        [DllImport(USER32_DLL)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, WindowPositionFlags flags);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, int* lpdwProcessId);

        [DllImport(USER32_DLL)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, WindowPlacement* lpwndpl);

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
        public static extern int GetWindowCompositionAttribute(IntPtr hwnd, WindowCompositionAttributeData* data);

        [DllImport(USER32_DLL)]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, WindowCompositionAttributeData* data);

        [DllImport(USER32_DLL)]
        public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, void* pvParam, uint fWinIni); 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SetWindowText(IntPtr handle, string text)
        {
            fixed (char* ptr = text)
                return SetWindowTextW(handle, ptr);
        }
    }
}
