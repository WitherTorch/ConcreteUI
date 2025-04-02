using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native;

using LocalsInit;

using WitherTorch.CrossNative.Helpers;

namespace WitherTorch.Windows.Graphics
{
    public sealed partial class RenderingController : IDisposable
    {
        public const int DefaultFramesPerSecond = 30; //30 fps

        private readonly IRenderingControl _control;
        private readonly RenderingThread _thread;

        private bool _disposed;
        private long _state;

        public RenderingController(IRenderingControl control)
        {
            _control = control;
            _state = (long)RenderingFlags._FlagAllTrue;
            _thread = new RenderingThread(this, GetMonitorFpsStatus());
        }

        public void RequestUpdate(bool force)
        {
            long oldState = InterlockedHelper.Or(ref _state, force ? (long)RenderingFlags.RedrawAll : (long)RenderingFlags.None);
            if ((oldState & (long)RenderingFlags.Locked) == (long)RenderingFlags.Locked)
                return;
            _thread.DoRender();
        }

        public void RequestResize()
        {
            long oldState = InterlockedHelper.Or(ref _state, (long)RenderingFlags.ResizeAndRedrawAll);
            if ((oldState & (long)RenderingFlags.Locked) == (long)RenderingFlags.Locked)
                return;
            _thread.DoRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderCore()
        {
            _control.Render((RenderingFlags)Interlocked.Exchange(ref _state, 0L));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() => InterlockedHelper.Or(ref _state, (long)RenderingFlags.Locked);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            long oldState = InterlockedHelper.And(ref _state, ~(long)RenderingFlags.Locked);
            if ((oldState & (long)RenderingFlags.Locked) == (long)RenderingFlags.Locked)
                _thread.DoRender();
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint GetMonitorFpsStatus()
        {
            DeviceModeW devMode;
            if (User32.EnumDisplaySettingsW(null, -1, &devMode))
                return devMode.dmDisplayFrequency;
            return DefaultFramesPerSecond;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void UpdateMonitorFpsStatus()
            => _thread.SetFramesPerSecond(GetMonitorFpsStatus());

        public void Stop() => _thread.Stop();

        public bool WaitForExit(int millisecondsTimeout) => _thread.WaitForExit(millisecondsTimeout);

        private void DisposeCore()
        {
            if (_disposed)
                return;
            _disposed = true;
            _thread.Dispose();
        }

        ~RenderingController()
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
