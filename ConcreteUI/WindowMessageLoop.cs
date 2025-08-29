using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Native;
using ConcreteUI.Window;

using LocalsInit;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI
{
    public static class WindowMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int Start(NativeWindow window, bool disposeAfterDestroyed = true)
        {
            window.Destroyed += OnWindowDestroyed;
            window.Show();
            int result = Start();
            if (disposeAfterDestroyed)
                window.Dispose();
            return result;
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int Start()
        {
            SysBool success;
            PumpingMessage msg;

            while (success = User32.GetMessageW(&msg, IntPtr.Zero, 0u, 0u))
            {
                if (success.IsFailed)
                {
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                    return -1;
                }
                User32.TranslateMessage(&msg);
                User32.DispatchMessageW(&msg);
            }
            return unchecked((int)msg.wParam);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Stop(int exitCode = 0) 
            => User32.PostQuitMessage(exitCode);

        private static void OnWindowDestroyed(object? sender, EventArgs e)
            => Stop();
    }
}
