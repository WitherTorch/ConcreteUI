using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Internals.Native;

using LocalsInit;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals
{
    partial class FrameWaiter
    {
        private sealed class LegacyImpl : IFrameWaiter
        {
            private const long NativeTicksPerSecond = 10000000;

            private long _nativeTicksPerFrameCycle, _nextTime;
            private Rational _framesPerSecond;

            public LegacyImpl(Rational framesPerSecond)
            {
                _framesPerSecond = framesPerSecond;
                _nativeTicksPerFrameCycle = NativeTicksPerSecond / framesPerSecond;
                _nextTime = 0;
            }

            public Rational FramesPerSecond
            {
                get => _framesPerSecond;
                set
                {
                    if (_framesPerSecond == value)
                        return;
                    _framesPerSecond = value;
                    Interlocked.Exchange(ref _nativeTicksPerFrameCycle, NativeTicksPerSecond / value);
                }
            }

            public bool TryEnterFrame()
            {
                long frameCycle = Interlocked.Read(ref _nativeTicksPerFrameCycle);
                if (frameCycle < 0L)
                {
                    _nextTime = 0;
                    return false;
                }
                _nextTime = frameCycle + GetSystemTimeInNativeTicks();
                return true;
            }

            public unsafe void LeaveFrameAndWait()
            {
                long nextTime = _nextTime;
                if (nextTime <= 0)
                    return;
                NtDll.NtDelayExecution(alertable: false, &nextTime);
            }

            [LocalsInit(false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe long GetSystemTimeInNativeTicks()
            {
                long result;
                Kernel32.GetSystemTimeAsFileTime(&result);
                return result;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _nativeTicksPerFrameCycle, -1);
                Interlocked.Exchange(ref _nextTime, -1);
            }
        }
    }
}
