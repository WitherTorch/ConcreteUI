using System;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;

namespace ConcreteUI.Window2
{
    public readonly ref struct ClipboardToken : IDisposable
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 請勿直接呼叫此建構函數，而是呼叫 <see cref="TryAcquire(int)"/> 或 <see cref="Acquire()"/> 靜態方法
        /// </summary>
        public ClipboardToken()
        {
            if (!User32.OpenClipboard(IntPtr.Zero))
                Marshal.ThrowExceptionForHR(User32.GetLastError());
        }

        public static bool TryAcquire(int millisecondsTimeout, out ClipboardToken result)
        {
            if (!_semaphore.Wait(millisecondsTimeout))
            {
                result = default;
                return false;
            }
            result = new ClipboardToken();
            return true;
        }

        public static bool TryAcquire(TimeSpan timeout, out ClipboardToken result)
        {
            if (!_semaphore.Wait(timeout))
            {
                result = default;
                return false;
            }
            result = new ClipboardToken();
            return true;
        }

        public static ClipboardToken Acquire()
        {
            _semaphore.Wait();
            return new ClipboardToken();
        }

        public void Dispose()
        {
            User32.CloseClipboard();
            _semaphore.Release();
        }
    }
}
