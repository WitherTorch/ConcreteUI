using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Native;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private sealed class RenderingThread : IDisposable
        {
            private static readonly long NativeTicksPerSecond;

            private static ulong _idCounter = 0;

            private readonly RenderingController _controller;
            private readonly Thread _thread;
            private readonly ManualResetEvent _exitTrigger;

            private IntPtr _triggerEventHandle;
            private long _nativeTicksPerFrameCycle;
            private uint _framesPerSecond;
            private bool _disposed;

            [LocalsInit(false)]
            unsafe static RenderingThread()
            {
                long frequency;
                if (!Kernel32.QueryPerformanceFrequency(&frequency))
                    throw new NotSupportedException("Cannot query QPC frequency!");
                NativeTicksPerSecond = frequency;
            }

            public unsafe RenderingThread(RenderingController controller, uint framesPerSecond)
            {
                _controller = controller;
                _thread = new Thread(ThreadLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _exitTrigger = new ManualResetEvent(false);
                _framesPerSecond = framesPerSecond;
                _nativeTicksPerFrameCycle = NativeTicksPerSecond / framesPerSecond;
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

            public void DoRender()
            {
                IntPtr handle = InterlockedHelper.Read(ref _triggerEventHandle);
                if (handle == IntPtr.Zero)
                    return;
                Kernel32.SetEvent(handle);
            }

            public void Stop()
            {
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, -1);
                IntPtr handle = InterlockedHelper.Read(ref _triggerEventHandle);
                if (handle == IntPtr.Zero)
                    return;
                Kernel32.SetEvent(handle);
            }

            private unsafe void ThreadLoop()
            {
                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + InterlockedHelper.GetAndIncrement(ref _idCounter).ToString("D"));
                RenderingController controller = _controller;
                ManualResetEvent exitTrigger = _exitTrigger;
                IntPtr triggerEventHandle = Kernel32.CreateEventW(null, bManualReset: false, bInitialState: false, null);
                InterlockedHelper.CompareExchange(ref _triggerEventHandle, triggerEventHandle, IntPtr.Zero);
                try
                {
                    Kernel32.WaitForSingleObject(triggerEventHandle, dwMilliseconds: Timeout.Infinite);

                    IntPtr sleepTimer = Kernel32.CreateWaitableTimerW(null, false, null);
                    IntPtr* waitHandles = stackalloc IntPtr[2] { triggerEventHandle, sleepTimer };
                    do
                    {
                        long frameCycle = Interlocked.Read(ref _nativeTicksPerFrameCycle);
                        if (frameCycle < 0L)
                            break;
                        frameCycle += GetSystemTimeInNativeTicks();
                        Thread.MemoryBarrier();
                        controller.RenderCore();
                        Kernel32.SetWaitableTimer(sleepTimer, &frameCycle, 0, null, null, false);
                        Kernel32.WaitForMultipleObjects(2u, waitHandles, bWaitAll: true, dwMilliseconds: Timeout.Infinite);
                    } while (true);
                    Kernel32.CloseHandle(sleepTimer);
                }
                finally
                {
                    InterlockedHelper.CompareExchange(ref _triggerEventHandle, IntPtr.Zero, triggerEventHandle);
                    Kernel32.CloseHandle(triggerEventHandle);
                    exitTrigger.Set();
                    exitTrigger.Dispose();
                }
            }

            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe long GetCurrentNativeTicks()
            {
                long result;
                if (!Kernel32.QueryPerformanceCounter(&result))
                    throw new NotSupportedException("Cannot query QPC ticks!");
                return result;
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
                ManualResetEvent exitTrigger = _exitTrigger;
                if (exitTrigger.SafeWaitHandle.IsClosed)
                    return true;
                try
                {
                    return exitTrigger.WaitOne(millisecondsTimeout);
                }
                catch (ObjectDisposedException)
                {
                    return true;
                }
            }

            private void DisposeCore()
            {
                if (_disposed)
                    return;
                _disposed = true;
                Stop();
                WaitForExit(200);
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }
        }
    }
}
