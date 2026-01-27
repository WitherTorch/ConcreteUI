using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using ConcreteUI.Graphics.Native;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private abstract class RenderingThreadBase : CriticalFinalizerObject, IDisposable
        {
            private static readonly long NativeTicksPerSecond;

            private static ulong _idCounter = 0;

            private readonly RenderingController _controller;
            private readonly Thread _thread;

            private IntPtr _renderingWaitingHandle, _exitTriggerHandle;
            private long _nativeTicksPerFrameCycle;
            private uint _framesPerSecond;
            private bool _disposed;

            [LocalsInit(false)]
            unsafe static RenderingThreadBase()
            {
                long frequency;
                if (!Kernel32.QueryPerformanceFrequency(&frequency))
                    throw new NotSupportedException("Cannot query QPC frequency!");
                NativeTicksPerSecond = frequency;
            }

            public unsafe RenderingThreadBase(RenderingController controller, uint framesPerSecond)
            {
                _controller = controller;
                _thread = new Thread(ThreadLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _framesPerSecond = framesPerSecond;
                _nativeTicksPerFrameCycle = NativeTicksPerSecond / framesPerSecond;
                _exitTriggerHandle = IntPtr.Zero;
                _thread.Start();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetFramesPerSecond(uint value)
            {
                if (_framesPerSecond == value)
                    return;
                _framesPerSecond = value;
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, NativeTicksPerSecond / value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long GetFramesPerSecond() => _framesPerSecond;

            public void DoRender() => Resume();

            public void Stop()
            {
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, -1);
                Resume();
            }

            private void Resume()
            {
                IntPtr handle = InterlockedHelper.Read(ref _renderingWaitingHandle);
                if (handle == IntPtr.Zero)
                    return;
                WakeUp(handle);
            }

            protected abstract void WakeUp(IntPtr waitingHandle);

            protected abstract IntPtr CreateWaitingHandle(bool manualReset);

            protected abstract bool Wait(IntPtr waitingHandle, uint timeout);

            protected abstract void DestroyWaitingHandle(IntPtr waitingHandle);

            private unsafe void ThreadLoop()
            {
                const uint Infinite = unchecked((uint)Timeout.Infinite);

                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + InterlockedHelper.GetAndIncrement(ref _idCounter).ToString("D"));
                RenderingController controller = _controller;
                IntPtr exitTriggerHandle = CreateWaitingHandle(manualReset: true);
                try
                {
                    if (InterlockedHelper.CompareExchange(ref _exitTriggerHandle, exitTriggerHandle, IntPtr.Zero) != IntPtr.Zero)
                        return;
                    IntPtr renderingWaitingHandle = CreateWaitingHandle(manualReset: false);
                    try
                    {
                        if (InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, renderingWaitingHandle, IntPtr.Zero) != IntPtr.Zero)
                            return;
                        Wait(renderingWaitingHandle, timeout: Infinite);
                        IntPtr sleepTimer = Kernel32.CreateWaitableTimerW(null, false, null);
                        try
                        {
                            do
                            {
                                long frameCycle = Interlocked.Read(ref _nativeTicksPerFrameCycle);
                                if (frameCycle < 0L)
                                    break;
                                frameCycle += GetSystemTimeInNativeTicks();
                                Thread.MemoryBarrier();
                                controller.RenderCore();
                                Kernel32.SetWaitableTimer(sleepTimer, &frameCycle, 0, null, null, false);
                                Wait(renderingWaitingHandle, timeout: Infinite);
                                Kernel32.WaitForSingleObject(sleepTimer, dwMilliseconds: Infinite);
                            } while (true);
                        }
                        finally
                        {
                            Kernel32.CloseHandle(sleepTimer);
                        }
                    }
                    finally
                    {
                        InterlockedHelper.CompareExchange(ref _renderingWaitingHandle, IntPtr.Zero, renderingWaitingHandle);
                        DestroyWaitingHandle(renderingWaitingHandle);
                    }
                }
                finally
                {
                    InterlockedHelper.CompareExchange(ref _exitTriggerHandle, IntPtr.Zero, exitTriggerHandle);
                    WakeUp(exitTriggerHandle);
                    DestroyWaitingHandle(exitTriggerHandle);
                }
            }

            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe long GetSystemTimeInNativeTicks()
            {
                long result;
                Kernel32.GetSystemTimeAsFileTime(&result);
                return result;
            }

            public bool WaitForExit(int millisecondsTimeout)
            {
                IntPtr handle = InterlockedHelper.Read(ref _exitTriggerHandle);
                if (handle == IntPtr.Zero || millisecondsTimeout < Timeout.Infinite)
                    return true;
                return Wait(handle, (uint)millisecondsTimeout);
            }

            ~RenderingThreadBase() => DisposeCore();

            private void DisposeCore()
            {
                if (ReferenceHelper.Exchange(ref _disposed, true))
                    return;
                Stop();
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
