﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Native;

using LocalsInit;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics
{
    public sealed partial class RenderingController : IDisposable
    {
        public const int DefaultFramesPerSecond = 30; //30 fps

        private readonly IRenderingControl _control;
        private readonly RenderingThread _thread;
        private readonly ManualResetEvent _waitForRenderingTrigger;

        private bool _disposed;
        private long _state, _locked;

        public RenderingController(IRenderingControl control)
        {
            _control = control;
            _state = (long)RenderingFlags._FlagAllTrue;
            _thread = new RenderingThread(this, GetMonitorFpsStatus());
            _waitForRenderingTrigger = new ManualResetEvent(true);
        }

        public void RequestUpdate(bool force)
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            if (force)
                InterlockedHelper.Or(ref _state, (long)RenderingFlags.RedrawAll);
            _thread.DoRender();
        }

        public void RequestResize()
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            InterlockedHelper.Or(ref _state, (long)RenderingFlags.ResizeAndRedrawAll);
            _thread.DoRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderCore()
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            _waitForRenderingTrigger.Reset();
            _control.Render((RenderingFlags)Interlocked.Exchange(ref _state, 0L));
            _waitForRenderingTrigger.Set();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() => Interlocked.Exchange(ref _locked, Booleans.TrueLong);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            if (Interlocked.CompareExchange(ref _locked, Booleans.FalseLong, Booleans.TrueLong) != Booleans.TrueLong)
                return;
            InterlockedHelper.Or(ref _state, (long)RenderingFlags.ResizeAndRedrawAll);
            _thread.DoRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForRendering()
        {
            _waitForRenderingTrigger.WaitOne();
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
