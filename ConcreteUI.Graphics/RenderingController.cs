using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

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

        private ulong _state, _locked, _isSystemBoosting;
        private bool _disposed;

        public bool NeedUpdateFps => _needUpdateFps;

        public RenderingController(IRenderingControl control, Rational framesPerSecond)
        {
            _control = control;
            _state = (ulong)RenderingFlags._FlagAllTrue;
            _frameWaiter = CreateFrameWaiter(control, framesPerSecond, out _needUpdateFps);
            _thread = new RenderingThread(this, _eventManager, _frameWaiter);
            _waitForRenderingTrigger = new ManualResetEventSlim(true);
            _isSystemBoosting = 0;
        }

        private static IFrameWaiter CreateFrameWaiter(IRenderingControl control, Rational framesPerSecond, out bool needUpdateFps)
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
            if (InterlockedHelper.Read(ref _locked) != 0UL)
                return;
            if (force)
                InterlockedHelper.Or(ref _state, (long)RenderingFlags.RedrawAll);
            _thread.DoRender();
        }

        public void RequestResize(bool temporarily)
        {
            if (InterlockedHelper.Read(ref _locked) != 0UL)
                return;
            InterlockedHelper.Or(ref _state, temporarily ? (ulong)RenderingFlags.ResizeTemporarilyAndRedrawAll : (ulong)RenderingFlags.ResizeAndRedrawAll);
            _thread.DoRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderCore()
        {
            if (InterlockedHelper.Read(ref _locked) != 0UL)
                return;
            ManualResetEventSlim trigger = _waitForRenderingTrigger;
            trigger.Reset();
            _control.Render((RenderingFlags)InterlockedHelper.Exchange(ref _state, 0UL));
            trigger.Set();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() => InterlockedHelper.Exchange(ref _locked, Booleans.TrueLong);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            if (InterlockedHelper.CompareExchange(ref _locked, Booleans.FalseLong, Booleans.TrueLong) != Booleans.TrueLong)
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
        public void SetFramesPerSecond(Rational value) => _frameWaiter.FramesPerSecond = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSystemBoosting(bool boost)
        {
            ulong value = UnsafeHelper.Negate(MathHelper.BooleanToUInt64(boost));
            if (InterlockedHelper.Exchange(ref _isSystemBoosting, value) == value)
                return;
            DelayedSystemBooster booster = DelayedSystemBooster.Instance;
            if (boost)
                booster.AddRef();
            else
                booster.RemoveRef();
        }

        public bool WaitForExit(int millisecondsTimeout) => _thread.WaitForExit(millisecondsTimeout);

        ~RenderingController() => DisposeCore(disposing: false);

        private void DisposeCore(bool disposing)
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            if (disposing)
            {
                _waitForRenderingTrigger.Dispose();
                _frameWaiter.Dispose();
                _thread.Dispose();
            }
            if (InterlockedHelper.Exchange(ref _isSystemBoosting, 0UL) != 0UL)
                DelayedSystemBooster.Instance.RemoveRef();
        }

        public void Dispose()
        {
            DisposeCore(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
