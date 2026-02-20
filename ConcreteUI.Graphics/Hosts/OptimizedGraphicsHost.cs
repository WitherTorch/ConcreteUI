using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

namespace ConcreteUI.Graphics.Hosts
{
    public class OptimizedGraphicsHost : SimpleGraphicsHost
    {
        private bool _forcePresentAll = false;
        private bool _switchToNormalSwapChain = false;

        public OptimizedGraphicsHost(GraphicsDeviceProvider deviceProvider, IntPtr handle,
            D2D1TextAntialiasMode textAntialiasMode, bool isFlipModel, bool isOpaque) : base(deviceProvider, handle, textAntialiasMode, isFlipModel, isOpaque) { }

        public OptimizedGraphicsHost(SimpleGraphicsHost another, IntPtr handle, bool isOpaque) : base(another, handle, isOpaque) { }

        protected override DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, bool isFlipModel, bool isOpaque)
        {
            if (provider.DXGIFactory is not DXGIFactory2 factory)
                throw new InvalidOperationException();

            DXGISwapChainDescription1 swapChainDesc = new DXGISwapChainDescription1()
            {
                BufferUsage = DXGIUsage.RenderTargetOutput | DXGIUsage.BackBuffer,
                Format = Constants.Format,
                SampleDesc = new DXGISampleDescription(1, 0),
                Stereo = false,
                Width = 0,
                Height = 0,
                AlphaMode = isOpaque ? DXGIAlphaMode.Ignore : DXGIAlphaMode.Premultiplied,
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
            return factory.CreateSwapChainForHwnd(provider.D3DDevice, AssociatedWindowHandle, swapChainDesc);
        }

        protected override DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, DXGISwapChain original, bool isOpaque)
        {
            if (original is not DXGISwapChain1 originalSwapChain || provider.DXGIFactory is not DXGIFactory2 factory)
                throw new InvalidOperationException();

            DXGISwapChainDescription1 swapChainDesc = originalSwapChain.Description1;
            swapChainDesc.AlphaMode = isOpaque ? DXGIAlphaMode.Ignore : DXGIAlphaMode.Premultiplied;
            DXGISwapChain1 swapChain = factory.CreateSwapChainForHwnd(provider.D3DDevice, AssociatedWindowHandle, swapChainDesc);
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
            if (hr == Constants.E_NOTIMPL)
            {
                _switchToNormalSwapChain = true;
                return base.GetExceptionHRForPresent(swapChain, PresentCore(swapChain));
            }
            return base.GetExceptionHRForPresent(swapChain, hr);
        }
    }
}
