using System.Threading;

using ConcreteUI.Graphics.Internals.Native;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals
{
    partial class FrameWaiter
    {
        private sealed class LegacyImpl : IFrameWaiter
        {
            private const ulong NativeTicksPerSecond = 10000000;

            private ulong _nativeTicksPerFrameCycle, _nextTime;
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
                    InterlockedHelper.Exchange(ref _nativeTicksPerFrameCycle, NativeTicksPerSecond / value);
                }
            }

            public bool TryEnterFrame()
            {
                ulong frameCycle = InterlockedHelper.Read(ref _nativeTicksPerFrameCycle);
                if (frameCycle <= 0L)
                {
                    _nextTime = 0;
                    return false;
                }
                _nextTime = NativeMethods.GetTicksForSystem() + frameCycle;
                return true;
            }

            public void LeaveFrameAndWait()
            {
                ulong nextTime = _nextTime;
                if (nextTime > 0 && !NativeMethods.SleepInAbsoluteTicks(nextTime))
                    Thread.Yield();
            }

            public void Dispose()
            {
                InterlockedHelper.Exchange(ref _nativeTicksPerFrameCycle, 0);
                InterlockedHelper.Exchange(ref _nextTime, 0);
            }
        }
    }
}
