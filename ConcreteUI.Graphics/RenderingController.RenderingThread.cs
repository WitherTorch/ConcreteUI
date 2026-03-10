using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using ConcreteUI.Graphics.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private sealed class RenderingThread : CriticalFinalizerObject, IDisposable
        {
            private static ulong _idCounter = 0;

            private readonly RenderingController _controller;
            private readonly IFrameWaiter _frameWaiter;
            private readonly Thread _thread;

            private IntPtr _renderingWaitingHandle, _exitTriggerHandle;
            private bool _disposed;

            public RenderingThread(RenderingController controller, IFrameWaiter frameWaiter)
            {
                _controller = controller;
                _frameWaiter = frameWaiter;
                _thread = new Thread(ThreadLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _exitTriggerHandle = IntPtr.Zero;
                _thread.Start();
            }

            public void DoRender() => Resume();

            private void Resume()
            {
                IntPtr handle = InterlockedHelper.Read(ref _renderingWaitingHandle);
                if (handle == IntPtr.Zero)
                    return;
                NativeMethods.SetWaitingHandle(handle);
            }

            private void ThreadLoop()
            {
                const uint Infinite = unchecked((uint)Timeout.Infinite);

                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + InterlockedHelper.GetAndIncrement(ref _idCounter).ToString("D"));
                RenderingController controller = _controller;
                IFrameWaiter frameWaiter = _frameWaiter;

                IntPtr exitTriggerHandle = NativeMethods.CreateWaitingHandle(autoReset: false);
                try
                {
                    if (InterlockedHelper.CompareExchange(ref _exitTriggerHandle, exitTriggerHandle, IntPtr.Zero) != IntPtr.Zero)
                        return;
                    IntPtr renderingWaitingHandle = NativeMethods.CreateWaitingHandle(autoReset: true);
                    try
                    {
                        if (InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, renderingWaitingHandle, IntPtr.Zero) != IntPtr.Zero)
                            return;
                        NativeMethods.WaitForWaitingHandle(renderingWaitingHandle, timeout: Infinite);
                        do
                        {
                            if (!frameWaiter.TryEnterFrame())
                                break;
                            Thread.MemoryBarrier();
                            controller.RenderCore();
                            frameWaiter.LeaveFrameAndWait();
                            NativeMethods.WaitForWaitingHandle(renderingWaitingHandle, timeout: Infinite);
                        } while (true);
                    }
                    finally
                    {
                        InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, IntPtr.Zero, renderingWaitingHandle);
                        NativeMethods.DestroyWaitingHandle(renderingWaitingHandle);
                    }
                }
                finally
                {
                    InterlockedHelper.CompareExchange(ref _exitTriggerHandle, IntPtr.Zero, exitTriggerHandle);
                    NativeMethods.SetWaitingHandle(exitTriggerHandle);
                    NativeMethods.DestroyWaitingHandle(exitTriggerHandle);
                }
            }

            public bool WaitForExit(int millisecondsTimeout)
            {
                IntPtr handle = InterlockedHelper.Read(ref _exitTriggerHandle);
                if (handle == IntPtr.Zero || millisecondsTimeout < Timeout.Infinite)
                    return true;
                return NativeMethods.WaitForWaitingHandle(handle, (uint)millisecondsTimeout);
            }

            ~RenderingThread() => DisposeCore();

            private void DisposeCore()
            {
                if (ReferenceHelper.Exchange(ref _disposed, true))
                    return;
                _frameWaiter.Dispose();
                Resume();
                WaitForExit(50);
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }
        }
    }
}
