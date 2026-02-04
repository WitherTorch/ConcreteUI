using System;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Graphics.Internals
{
    internal static partial class FrameWaiter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFrameWaiter CreateWithWaitHandle(IntPtr handle) => new ModernImpl(handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFrameWaiter CreateWithFramesPerSecond(uint framesPerSecond) => new LegacyImpl(framesPerSecond);
    }

    internal interface IFrameWaiter : IDisposable
    {
        uint FramesPerSecond { get; set; }

        bool TryEnterFrame();

        void LeaveFrameAndWait();
    }
}
