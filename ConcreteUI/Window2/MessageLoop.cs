using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Native;

using LocalsInit;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window2
{
    public static class MessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int StartMessageLoop(NativeWindow window, bool disposeAfterDestroyed = true)
        {
            window.Destroyed += OnWindowDestroyed;
            window.Show();
            int result = StartMessageLoop();
            if (disposeAfterDestroyed)
                window.Dispose();
            return result;
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int StartMessageLoop()
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
        public static void StopMessageLoop(int exitCode = 0) 
            => User32.PostQuitMessage(exitCode);

        private static void OnWindowDestroyed(object? sender, EventArgs e)
            => StopMessageLoop();
    }
}
