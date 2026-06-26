using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using ShioUI.Graphics.Internals.Native;

using RiceTea.Core.Helpers;
using RiceTea.Core.Windows.Structures;

namespace ShioUI.Graphics.Internals;

partial class FrameWaiter
{
    private sealed class ModernImpl : CriticalFinalizerObject, IFrameWaiter
    {
        private readonly IntPtr _handle;

        private ulong _disposed;

        public Rational FramesPerSecond
        {
            get => Rational.Zero;
            set { }
        }

        public ModernImpl(IntPtr handle)
        {
            _handle = handle;
        }

        public bool TryEnterFrame() => InterlockedHelper.Read(ref _disposed) == 0UL;

        public void LeaveFrameAndWait()
        {
            if (InterlockedHelper.Read(ref _disposed) != 0UL)
                return;
            uint hr = Kernel32.WaitForSingleObject(_handle, dwMilliseconds: unchecked((uint)Timeout.Infinite));
            if (hr == 0xFFFFFFFFu)
                throw new Win32Exception((int)Kernel32.GetLastError());
        }

        ~ModernImpl() => DisposeCore();

        private void DisposeCore()
        {
            if (InterlockedHelper.Exchange(ref _disposed, ulong.MaxValue) != 0UL)
                return;
            Kernel32.CloseHandle(_handle);
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
