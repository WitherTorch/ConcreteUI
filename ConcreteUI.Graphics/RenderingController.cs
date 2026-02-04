using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics
{
    public sealed partial class RenderingController : IDisposable
    {
        private static readonly IWaitingEventManager _eventManager = WaitingEventManager.Create();

        private readonly IRenderingControl _control;
        private readonly IFrameWaiter _frameWaiter;
        private readonly RenderingThread _thread;
        private readonly ManualResetEventSlim _waitForRenderingTrigger;
        private readonly bool _needUpdateFps;

        private bool _disposed;
        private long _state, _locked;

        public bool NeedUpdateFps => _needUpdateFps;

        public RenderingController(IRenderingControl control, uint framesPerSecond)
        {
            _control = control;
            _state = (long)RenderingFlags._FlagAllTrue;
            _frameWaiter = CreateFrameWaiter(control, framesPerSecond, out _needUpdateFps);
            _thread = new RenderingThread(this, _eventManager, _frameWaiter);
            _waitForRenderingTrigger = new ManualResetEventSlim(true);
        }

        private static IFrameWaiter CreateFrameWaiter(IRenderingControl control, uint framesPerSecond, out bool needUpdateFps)
        {
            DXGISwapChain swapChain = control.GetSwapChain();
            if (!swapChain.TryQueryInterface(DXGISwapChain2.IID_IDXGISwapChain2, out DXGISwapChain2? swapChain2))
                goto Fallback;
            try
            {
                IntPtr handle = swapChain2.GetFrameLatencyWaitableObject();
                if (handle == IntPtr.Zero)
                    goto Fallback;

                needUpdateFps = false;
                return FrameWaiter.CreateWithWaitHandle(handle);
            }
            finally
            {
                swapChain2.Dispose();
            }
        Fallback:
            needUpdateFps = true;
            return FrameWaiter.CreateWithFramesPerSecond(framesPerSecond);
        }

        public void RequestUpdate(bool force)
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            if (force)
                InterlockedHelper.Or(ref _state, (long)RenderingFlags.RedrawAll);
            _thread.DoRender();
        }

        public void RequestResize(bool temporarily)
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            InterlockedHelper.Or(ref _state, temporarily ? (long)RenderingFlags.ResizeTemporarilyAndRedrawAll : (long)RenderingFlags.ResizeAndRedrawAll);
            _thread.DoRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderCore()
        {
            if (InterlockedHelper.Read(ref _locked) != 0L)
                return;
            ManualResetEventSlim trigger = _waitForRenderingTrigger;
            trigger.Reset();
            _control.Render((RenderingFlags)Interlocked.Exchange(ref _state, 0L));
            trigger.Set();
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
            _waitForRenderingTrigger.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFramesPerSecond(uint value) => _frameWaiter.FramesPerSecond = value;

        public bool WaitForExit(int millisecondsTimeout) => _thread.WaitForExit(millisecondsTimeout);

        private void DisposeCore()
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            _waitForRenderingTrigger.Dispose();
            _frameWaiter.Dispose();
            _thread.Dispose();
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
