using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Native;

using LocalsInit;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private sealed class RenderingThread : IDisposable
        {
            private static readonly long NativeTicksPerSecond;

            private static long _serialNumber = -1;

            private readonly RenderingController _controller;
            private readonly Thread _thread;
            private readonly AutoResetEvent _trigger;
            private readonly ManualResetEvent _exitTrigger;

            private bool _disposed;
            private uint _framesPerSecond;
            private long _nativeTicksPerFrameCycle;

            [LocalsInit(false)]
            unsafe static RenderingThread()
            {
                long frequency;
                if (!Kernel32.QueryPerformanceFrequency(&frequency))
                    throw new NotSupportedException("Cannot query QPC frequency!");
                NativeTicksPerSecond = frequency;
            }

            public RenderingThread(RenderingController controller, uint framesPerSecond)
            {
                _controller = controller;
                _thread = new Thread(ThreadLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _trigger = new AutoResetEvent(false);
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
                AutoResetEvent trigger = _trigger;
                if (trigger.SafeWaitHandle.IsClosed)
                    return;
                try
                {
                    trigger.Set();
                }
                catch (Exception)
                {
                }
            }

            public void Stop()
            {
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, -1);
                AutoResetEvent trigger = _trigger;
                if (trigger.SafeWaitHandle.IsClosed)
                    return;
                try
                {
                    trigger.Set();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            private unsafe void ThreadLoop()
            {
                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + Interlocked.Increment(ref _serialNumber).ToString("D"));
                RenderingController controller = _controller;
                AutoResetEvent trigger = _trigger;
                ManualResetEvent exitTrigger = _exitTrigger;
                IntPtr sleepTimer = Kernel32.CreateWaitableTimerW(null, false, null);
                do
                {
                    trigger.WaitOne(Timeout.Infinite, exitContext: true);
                    long frameCycle = Interlocked.Read(ref _nativeTicksPerFrameCycle);
                    if (frameCycle < 0L)
                        break;
                    frameCycle += GetSystemTimeInNativeTicks();
                    Thread.MemoryBarrier();
                    controller.RenderCore();
                    Kernel32.SetWaitableTimer(sleepTimer, &frameCycle, 0, null, null, false);
                    Kernel32.WaitForSingleObject(sleepTimer, Timeout.Infinite);
                } while (true);
                Kernel32.CloseHandle(sleepTimer);
                trigger.Dispose();
                exitTrigger.Set();
                exitTrigger.Dispose();
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
