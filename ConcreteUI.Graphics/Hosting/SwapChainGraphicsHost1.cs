using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using static ConcreteUI.Graphics.Constants;

namespace ConcreteUI.Graphics.Hosting
{
    public class SwapChainGraphicsHost1 : SwapChainGraphicsHost
    {
        private bool _forcePresentAll = false;
        private bool _switchToNormalSwapChain = false;

        public SwapChainGraphicsHost1(GraphicsDeviceProvider deviceProvider, IntPtr handle,
            D2D1TextAntialiasMode textAntialiasMode, bool isFlipModel) : base(deviceProvider, handle, textAntialiasMode, isFlipModel) { }

        public SwapChainGraphicsHost1(SwapChainGraphicsHost another, IntPtr handle) : base(another, handle) { }

        protected override unsafe DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, bool isFlipModel)
        {
            if (provider.DXGIFactory is not DXGIFactory2 factory)
                return base.CreateSwapChain(provider, isFlipModel);
            DXGISwapChainDescription1 swapChainDesc = new DXGISwapChainDescription1()
            {
                BufferUsage = DXGIUsage.RenderTargetOutput | DXGIUsage.BackBuffer,
                Format = Format,
                SampleDesc = new DXGISampleDescription(1, 0),
                Stereo = false,
                Width = 0,
                Height = 0,
                AlphaMode = DXGIAlphaMode.Unspecified,
                Flags = DXGISwapChainFlags.None,
            };
            if (isFlipModel)
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.Scaling = DXGIScaling.None;
                swapChainDesc.SwapEffect = DXGISwapEffect.FlipSequential;
            }
            else
            {
                swapChainDesc.BufferCount = 1;
                swapChainDesc.Scaling = DXGIScaling.Stretch;
                swapChainDesc.SwapEffect = DXGISwapEffect.Sequential;
            }
            return factory.CreateSwapChainForHwnd(provider.D3DDevice, Handle, swapChainDesc);
        }

        protected override unsafe DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, DXGISwapChain original)
        {
            if (original is not DXGISwapChain1 originalSwapChain || provider.DXGIFactory is not DXGIFactory2 factory)
                return base.CreateSwapChain(provider, original);
            DXGISwapChainDescription1 swapChainDesc = originalSwapChain.Description1;
            DXGISwapChain1 swapChain = factory.CreateSwapChainForHwnd(provider.D3DDevice, Handle, swapChainDesc);
            return swapChain;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPresent(in DXGIPresentParameters parameters)
        {
            return PresentCore(parameters) is null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Present(in DXGIPresentParameters parameters)
        {
            Exception? exception = PresentCore(parameters);
            if (exception is null)
                return;
            throw exception;
        }

        private Exception? PresentCore(in DXGIPresentParameters parameters)
        {
            DXGISwapChain swapChain = _swapChain;
            if (swapChain is null || swapChain.IsDisposed)
                return null;
            if (BeforeNormalPresent(swapChain))
                return null;
            Exception? exception = GetExceptionHRForPresent(swapChain, PresentCore(swapChain, parameters));
            return exception;
        }

        [Inline(InlineBehavior.Remove)]
        private int PresentCore(DXGISwapChain swapChain, in DXGIPresentParameters parameters)
        {
            if (swapChain is not DXGISwapChain1 swapChain1 || _switchToNormalSwapChain)
                return PresentCore(swapChain);
            if (_forcePresentAll)
            {
                _forcePresentAll = false;
                return PresentCore(swapChain);
            }
            return swapChain1.TryPresent1(0, parameters);
        }

        protected override Exception? GetExceptionHRForPresent(DXGISwapChain swapChain, int hr)
        {
            if (hr == E_NOTIMPL)
            {
                _switchToNormalSwapChain = true;
                return base.GetExceptionHRForPresent(swapChain, PresentCore(swapChain));
            }
            return base.GetExceptionHRForPresent(swapChain, hr);
        }
    }
}
