using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Internals.Native;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private sealed class RenderingThread : CriticalFinalizerObject, IDisposable
        {
            private static ulong _idCounter = 0;

            private readonly RenderingController _controller;
            private readonly IWaitingEventManager _eventManager;
            private readonly IFrameWaiter _frameWaiter;
            private readonly Thread _thread;

            private IntPtr _renderingWaitingHandle, _exitTriggerHandle;
            private bool _disposed;

            public unsafe RenderingThread(RenderingController controller, IWaitingEventManager eventManager, IFrameWaiter frameWaiter)
            {
                _controller = controller;
                _eventManager = eventManager;
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
                _eventManager.WakeUp(handle);
            }

            private unsafe void ThreadLoop()
            {
                const uint Infinite = unchecked((uint)Timeout.Infinite);

                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + InterlockedHelper.GetAndIncrement(ref _idCounter).ToString("D"));
                RenderingController controller = _controller;
                IWaitingEventManager eventManager = _eventManager;
                IFrameWaiter frameWaiter = _frameWaiter;


                IntPtr exitTriggerHandle = eventManager.CreateWaitingHandle(manualReset: true);
                try
                {
                    if (InterlockedHelper.CompareExchange(ref _exitTriggerHandle, exitTriggerHandle, IntPtr.Zero) != IntPtr.Zero)
                        return;
                    IntPtr renderingWaitingHandle = eventManager.CreateWaitingHandle(manualReset: false);
                    try
                    {
                        if (InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, renderingWaitingHandle, IntPtr.Zero) != IntPtr.Zero)
                            return;
                        eventManager.Wait(renderingWaitingHandle, timeout: Infinite);
                        do
                        {
                            if (!frameWaiter.TryEnterFrame())
                                break;
                            Thread.MemoryBarrier();
                            controller.RenderCore();
                            frameWaiter.LeaveFrameAndWait();
                            eventManager.Wait(renderingWaitingHandle, timeout: Infinite);
                        } while (true);
                    }
                    finally
                    {
                        InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, IntPtr.Zero, renderingWaitingHandle);
                        eventManager.DestroyWaitingHandle(renderingWaitingHandle);
                    }
                }
                finally
                {
                    InterlockedHelper.CompareExchange(ref _exitTriggerHandle, IntPtr.Zero, exitTriggerHandle);
                    eventManager.WakeUp(exitTriggerHandle);
                    eventManager.DestroyWaitingHandle(exitTriggerHandle);
                }
            }

            public bool WaitForExit(int millisecondsTimeout)
            {
                IntPtr handle = InterlockedHelper.Read(ref _exitTriggerHandle);
                if (handle == IntPtr.Zero || millisecondsTimeout < Timeout.Infinite)
                    return true;
                return _eventManager.Wait(handle, (uint)millisecondsTimeout);
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
