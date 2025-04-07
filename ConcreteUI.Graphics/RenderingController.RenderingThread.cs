using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Native;

using InlineMethod;

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
            private static readonly long NativeTicksPerMillisecond;
            private static readonly long NativeTicksForLargeSleepGap;
            private static readonly long NativeTicksForSmallSleepGap;

            private static long _serialNumber = -1;

            private readonly RenderingController _controller;
            private readonly Thread _thread;

            private bool _disposed;
            private uint _framesPerSecond;
            private long _nativeTicksPerFrameCycle;
            private AutoResetEvent _trigger;
            private ManualResetEvent _exitTrigger;

            [LocalsInit(false)]
            unsafe static RenderingThread()
            {
                long frequency;
                if (!Kernel32.QueryPerformanceFrequency(&frequency))
                    throw new NotSupportedException("Cannot query QPC frequency!");
                NativeTicksPerSecond = frequency;
                NativeTicksPerMillisecond = frequency / 1000;

                NativeTicksForLargeSleepGap = frequency / 1000 * 15;
                NativeTicksForSmallSleepGap = frequency / 1000 / 5;
            }

            public RenderingThread(RenderingController controller, uint framesPerSecond)
            {
                _controller = controller;
                _thread = new Thread(ThreadLoop)
                {
                    IsBackground = true
                };
                _trigger = new AutoResetEvent(true);
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
                _trigger?.Set();
            }

            public void Stop()
            {
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, -1);
                _trigger?.Set();
            }

            private void ThreadLoop()
            {
                ThreadHelper.SetCurrentThreadName("Concrete UI Rendering Thread #" + Interlocked.Increment(ref _serialNumber).ToString("D"));
                RenderingController controller = _controller;
                AutoResetEvent trigger = _trigger;
                ManualResetEvent exitTrigger = _exitTrigger;
                long lastRenderTime = GetCurrentNativeTicks();
                do
                {
                    trigger.WaitOne(Timeout.Infinite, exitContext: true);
                    long frameCycle = Interlocked.Read(ref _nativeTicksPerFrameCycle);
                    if (frameCycle < 0L)
                        break;
                    lastRenderTime = GetCurrentNativeTicks() - lastRenderTime;
                    if (frameCycle > lastRenderTime)
                        SleepInTicks(frameCycle - lastRenderTime, removeNonPositiveCheck: true);
                    lastRenderTime = GetCurrentNativeTicks();
                    controller.RenderCore();
                } while (true);
                exitTrigger.Set();
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

#pragma warning disable CS0162
            [Inline(InlineBehavior.Remove)]
            private static void SleepInTicks(long ticks, [InlineParameter] bool removeNonPositiveCheck)
            {
                if (!removeNonPositiveCheck && ticks <= 0)
                    return;
                if (Constants.UsePreciseSleepFunction)
                {
                    SleepInNativeTicksPrecise(ticks);
                    return;
                }
                SleepInNativeTicksImprecise(ticks);
            }
#pragma warning restore CS0162

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SleepInNativeTicksPrecise(long ticks)
            {
                if (ticks >= NativeTicksForLargeSleepGap)
                {
                    SleepInNativeTicksPreciseLarge(ticks);
                    return;
                }
                SleepInNativeTicksPreciseSmall(ticks);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SleepInNativeTicksImprecise(long ticks)
            {
                Thread.Sleep(unchecked((int)(ticks / NativeTicksPerMillisecond)));
            }

            private static void SleepInNativeTicksPreciseLarge(long ticks)
            {
                long now = GetCurrentNativeTicks();
                Thread.Sleep(unchecked((int)(ticks / NativeTicksPerMillisecond)));
                if ((now - GetCurrentNativeTicks()) >= ticks)
                    return;
                SleepInNativeTicksPreciseSmall(ticks);
            }

            private static void SleepInNativeTicksPreciseSmall(long ticks)
            {
                long now = GetCurrentNativeTicks();
                long gap = NativeTicksForSmallSleepGap;
                do
                {
                    SleepInNativeTicksPreciseTiny();
                    long oldNow = now;
                    now = GetCurrentNativeTicks();
                    ticks -= now - oldNow;
                } while (ticks >= gap);
                if (ticks > 0)
                    SleepInNativeTicksPreciseTiny();
            }

            [Inline(InlineBehavior.Remove)]
            private static void SleepInNativeTicksPreciseTiny()
            {
                Thread.Sleep(0);
                Thread.Yield();
            }

            public bool WaitForExit(int millisecondsTimeout)
                => _exitTrigger?.WaitOne(millisecondsTimeout) != false;

            private void DisposeCore()
            {
                if (_disposed)
                    return;
                _disposed = true;
                Stop();
                WaitForExit(200);
                DisposeHelper.SwapDispose(ref _trigger);
                DisposeHelper.SwapDispose(ref _exitTrigger);
            }

            ~RenderingThread()
            {
                DisposeCore();
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }
        }
    }
}
