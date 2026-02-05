using System;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals
{
    internal static partial class FrameWaiter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFrameWaiter CreateWithWaitHandle(IntPtr handle) => new ModernImpl(handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFrameWaiter CreateWithFramesPerSecond(Rational framesPerSecond) => new LegacyImpl(framesPerSecond);
    }

    internal interface IFrameWaiter : IDisposable
    {
        Rational FramesPerSecond { get; set; }

        bool TryEnterFrame();

        void LeaveFrameAndWait();
    }
}
