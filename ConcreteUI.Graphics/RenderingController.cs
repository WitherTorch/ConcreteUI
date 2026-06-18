using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics;

public sealed partial class RenderingController : CriticalFinalizerObject, IDisposable
{
    private readonly IRenderingControl _control;
    private readonly IFrameWaiter _frameWaiter;
    private readonly RenderingThread _thread;
    private readonly IntPtr _waitForRenderingTrigger;
    private readonly bool _needUpdateFps;

    private ulong _state;
    private nuint _lockedCount, _isSystemBoosting, _disposed;

    public bool NeedUpdateFps => _needUpdateFps;

    public RenderingController(IRenderingControl control, Rational framesPerSecond)
    {
        _control = control;
        _state = (ulong)RenderingFlags._FlagAllTrue;
        _frameWaiter = CreateFrameWaiter(control, framesPerSecond, out _needUpdateFps);
        _thread = new RenderingThread(this, _frameWaiter);
        _waitForRenderingTrigger = NativeMethods.CreateWaitingHandle(autoReset: false);
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
        if (force)
            InterlockedHelper.Or(ref _state, (ulong)RenderingFlags.RedrawAll);

        if (InterlockedHelper.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    public void RequestUpdateUnsafe(RenderingFlags flags)
    {
        InterlockedHelper.Or(ref _state, (ulong)flags); 

        if (InterlockedHelper.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    public void RequestUpdateAndResize(bool temporarily)
    {
        ulong state;
#if NET8_0_OR_GREATER
        state = temporarily ? (ulong)RenderingFlags.ResizeTemporarilyAndRedrawAll : (ulong)RenderingFlags.ResizeAndRedrawAll;
#else
        state = (ulong)RenderingFlags.ResizeAndRedrawAll | ((ulong)RenderingFlags._ResizeTemporarilyFlag & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(temporarily)));
#endif
        InterlockedHelper.Or(ref _state, state);
        if (InterlockedHelper.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    public void RequestUpdateAndResize(bool temporarily, bool redrawAll)
    {
        ulong state;
#if NET8_0_OR_GREATER
        state = (temporarily ? (ulong)RenderingFlags.ResizeTemporarily : (ulong)RenderingFlags.Resize) | (redrawAll ? (ulong)RenderingFlags.RedrawAll : default);
#else
        state = (ulong)RenderingFlags.Resize |
            ((ulong)RenderingFlags._ResizeTemporarilyFlag & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(temporarily))) |
            ((ulong)RenderingFlags.RedrawAll & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(redrawAll)));
#endif
        InterlockedHelper.Or(ref _state, state);
        if (InterlockedHelper.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderCore()
    {
        if (InterlockedHelper.Read(ref _lockedCount) != 0UL)
            return;
        IntPtr trigger = _waitForRenderingTrigger;
        NativeMethods.ResetWaitingHandle(trigger);
        try
        {
            _control.Render(this);
        }
        finally
        {
            NativeMethods.SetWaitingHandle(trigger);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Lock()
    {
        if (InterlockedHelper.Read(ref _disposed) != default ||
            InterlockedHelper.LimitedIncrement(ref _lockedCount, UnsafeHelper.GetMaxValue<nuint>()) > 1)
            return;
        _thread.StartNextWaiting();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unlock()
    {
        if (InterlockedHelper.Read(ref _disposed) != default || 
            InterlockedHelper.LimitedDecrement(ref _lockedCount, default) > 0 ||
            InterlockedHelper.Read(ref _state) == default)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unlock(bool resizeAll)
    {
        if (InterlockedHelper.Read(ref _disposed) != default || 
            InterlockedHelper.LimitedDecrement(ref _lockedCount, default) > 0)
            return;
        InterlockedHelper.Or(ref _state, (ulong)RenderingFlags.RedrawAll | 
            ((ulong)RenderingFlags.Resize & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(resizeAll))));
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RenderingFlags GetAndResetRenderingFlags()
    {
        _thread.StartNextWaiting();
        return (RenderingFlags)InterlockedHelper.Exchange(ref _state, default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WaitForRendering() => NativeMethods.WaitForWaitingHandle(_waitForRenderingTrigger);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFramesPerSecond(Rational value) => _frameWaiter.FramesPerSecond = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetSystemBoosting(bool boost)
    {
        nuint value = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(boost));
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
        if (InterlockedHelper.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != 0)
            return;
        InterlockedHelper.Write(ref _lockedCount, UnsafeHelper.GetMaxValue<nuint>());
        NativeMethods.DestroyWaitingHandle(_waitForRenderingTrigger);
        if (disposing)
        {
            _frameWaiter.Dispose();
            _thread.Dispose();
        }
        if (InterlockedHelper.Exchange(ref _isSystemBoosting, default) != default)
            DelayedSystemBooster.Instance.RemoveRef();
    }

    public void Dispose()
    {
        DisposeCore(disposing: true);
        GC.SuppressFinalize(this);
    }
}
