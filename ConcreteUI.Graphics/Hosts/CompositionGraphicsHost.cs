using System;
using System.Drawing;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DirectComposition;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common.Helpers;

using static ConcreteUI.Graphics.Constants;

namespace ConcreteUI.Graphics.Hosts
{
    public sealed class CompositionGraphicsHost : OptimizedGraphicsHost
    {
        private readonly DCompositionTarget _target;
        private readonly DCompositionVisual _visual;

        public CompositionGraphicsHost(GraphicsDeviceProvider deviceProvider, IntPtr handle,
            D2D1TextAntialiasMode textAntialiasMode, bool isOpaque) : base(deviceProvider, handle, textAntialiasMode, true, isOpaque)
        {
            DCompositionDevice device = NullSafetyHelper.ThrowIfNull(deviceProvider.DCompDevice);
            Initialize(device, (DXGISwapChain1)_swapChain, handle, out _target, out _visual);
        }

        public CompositionGraphicsHost(SimpleGraphicsHost another, IntPtr handle, bool isOpaque) : base(another, handle, isOpaque)
        {
            DCompositionDevice device = NullSafetyHelper.ThrowIfNull(another.GetDeviceProvider().DCompDevice);
            Initialize(device, (DXGISwapChain1)_swapChain, handle, out _target, out _visual);
        }

        private static void Initialize(DCompositionDevice device, DXGISwapChain1 swapChain, IntPtr handle,
            out DCompositionTarget target, out DCompositionVisual visual)
        {
            target = device.CreateTargetForHwnd(handle, topMost: true);
            visual = device.CreateVisual();
            visual.SetContent(swapChain);
            target.SetRoot(visual);
            device.Commit();
        }

        protected override DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, bool isFlipModel, bool isOpaque)
        {
            if (!isFlipModel || provider.DXGIFactory is not DXGIFactory2 factory)
                throw new InvalidOperationException();

            DXGISwapChainDescription1 swapChainDesc = new DXGISwapChainDescription1
            {
                BufferUsage = DXGIUsage.RenderTargetOutput,
                Format = Format,
                SampleDesc = new DXGISampleDescription(1, 0),
                Stereo = false,
                Width = 1,
                Height = 1,
                AlphaMode = isOpaque ? DXGIAlphaMode.Ignore : DXGIAlphaMode.Premultiplied,
                Flags = DXGISwapChainFlags.None,
                BufferCount = 2,
                Scaling = DXGIScaling.Stretch,
                SwapEffect = DXGISwapEffect.FlipSequential
            };
            return GetLatestSwapChain(factory.CreateSwapChainForComposition(provider.D3DDevice, swapChainDesc));
        }

        protected override DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, DXGISwapChain original, bool isOpaque)
        {
            if (original is not DXGISwapChain1 originalSwapChain || provider.DXGIFactory is not DXGIFactory2 factory)
                throw new InvalidOperationException();

            DXGISwapChainDescription1 swapChainDesc = originalSwapChain.Description1;
            swapChainDesc.AlphaMode = isOpaque ? DXGIAlphaMode.Ignore : DXGIAlphaMode.Premultiplied;
            DXGISwapChain1 swapChain = factory.CreateSwapChainForComposition(provider.D3DDevice, swapChainDesc);
            return GetLatestSwapChain(swapChain);
        }

        private static DXGISwapChain GetLatestSwapChain(DXGISwapChain swapChain)
        {
            if (swapChain is DXGISwapChain2)
                goto NotFound;

            DXGISwapChain? result;

            if ((result = swapChain.QueryInterface<DXGISwapChain2>(DXGISwapChain2.IID_IDXGISwapChain2, throwWhenQueryFailed: false)) is not null)
                goto Found;

            goto NotFound;

        NotFound:
            return swapChain;

        Found:
            swapChain.Dispose();
            return result;
        }

        public override void ResizeTemporarily(Size size)
        {
            Size oldSize = _size;
            if (size.Width > oldSize.Width || size.Height > oldSize.Height)
                goto Fallback;

            //swapChain.SourceSize = new SizeU(width, height);
            return;

        Fallback:
            base.ResizeTemporarily(size);
            return;
        }

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            _target?.Dispose();
            _visual?.Dispose();
        }
    }
}
