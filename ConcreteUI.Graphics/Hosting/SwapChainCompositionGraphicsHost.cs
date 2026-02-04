using System;
using System.Drawing;

using ConcreteUI.Graphics.Internals.Native;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DirectComposition;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

using static ConcreteUI.Graphics.Constants;

namespace ConcreteUI.Graphics.Hosting
{
    public sealed class SwapChainCompositionGraphicsHost : SwapChainGraphicsHost1
    {
        private readonly DCompositionTarget _target;
        private readonly DCompositionVisual _visual;

        public SwapChainCompositionGraphicsHost(GraphicsDeviceProvider deviceProvider, IntPtr handle,
            D2D1TextAntialiasMode textAntialiasMode) : base(deviceProvider, handle, textAntialiasMode, true)
        {
            DCompositionDevice device = NullSafetyHelper.ThrowIfNull(deviceProvider.DCompDevice);
            Initialize(device, (DXGISwapChain1)_swapChain, handle, out _target, out _visual);
        }

        public SwapChainCompositionGraphicsHost(SwapChainGraphicsHost another, IntPtr handle) : base(another, handle)
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

        protected override unsafe DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, bool isFlipModel)
        {
            if (!isFlipModel || provider.DXGIFactory is not DXGIFactory2 factory)
                return base.CreateSwapChain(provider, isFlipModel);

            DXGISwapChainDescription1 swapChainDesc = new DXGISwapChainDescription1
            {
                BufferUsage = DXGIUsage.RenderTargetOutput,
                Format = Format,
                SampleDesc = new DXGISampleDescription(1, 0),
                Stereo = false,
                Width = 1,
                Height = 1,
                AlphaMode = DXGIAlphaMode.Premultiplied,
                Flags = DXGISwapChainFlags.None,
                BufferCount = 2,
                Scaling = DXGIScaling.Stretch,
                SwapEffect = DXGISwapEffect.FlipSequential
            };
            return GetLatestSwapChain(factory.CreateSwapChainForComposition(provider.D3DDevice, swapChainDesc));
        }

        protected override unsafe DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, DXGISwapChain original)
        {
            if (original is not DXGISwapChain1 originalSwapChain || provider.DXGIFactory is not DXGIFactory2 factory)
                return base.CreateSwapChain(provider, original);
            DXGISwapChainDescription1 swapChainDesc = originalSwapChain.Description1;
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

        public override void ResizeTemporarily(in Size size)
        {
            if (_swapChain is not DXGISwapChain2 swapChain)
                goto Fallback;

            DXGISwapChainDescription1 description = swapChain.Description1;
            uint width, height;
            if ((width = MathHelper.MakeUnsigned(size.Width)) > description.Width ||
                (height = MathHelper.MakeUnsigned(size.Height)) > description.Height)
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
