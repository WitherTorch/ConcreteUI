#pragma warning disable CS0618

using System;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Internals.Native;

using InlineMethod;

namespace ConcreteUI.Utils
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ClipboardScope : IDisposable
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly bool _needClose;

        private ClipboardScope(bool needClose) => _needClose = needClose;

        public static bool TryEnter(int millisecondsTimeout, out ClipboardScope result)
        {
            if (!_semaphore.Wait(millisecondsTimeout))
            {
                result = default;
                return false;
            }
            result = EnterCore(throwException: false);
            return true;
        }

        public static bool TryEnter(TimeSpan timeout, out ClipboardScope result)
        {
            if (!_semaphore.Wait(timeout))
            {
                result = default;
                return false;
            }
            result = EnterCore(throwException: false);
            return true;
        }

        public static ClipboardScope Enter()
        {
            _semaphore.Wait();
            return EnterCore(throwException: true);
        }

        [Inline(InlineBehavior.Remove)]
        private static ClipboardScope EnterCore([InlineParameter] bool throwException)
        {
            if (User32.OpenClipboard(IntPtr.Zero))
                return new ClipboardScope(needClose: true);
            try
            {
                if (throwException)
                    Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                return default;
            }
            finally
            {
                _semaphore.Release(); 
            }
        }

        public void Dispose()
        {
            if (!_needClose)
                return;
            User32.CloseClipboard();
            _semaphore.Release();
        }
    }
}
