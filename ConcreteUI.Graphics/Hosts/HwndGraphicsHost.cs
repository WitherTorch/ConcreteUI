using System;
using System.Drawing;

using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Graphics.Hosts
{
    public sealed class HwndGraphicsHost : IGraphicsHost
    {
        private readonly D2D1HwndRenderTarget _renderTarget;

        public nint AssociatedWindowHandle => throw new NotImplementedException();

        public bool IsDisposed => throw new NotImplementedException();

        public event EventHandler? DeviceRemoved;

        public HwndGraphicsHost(GraphicsDeviceProvider provider, IntPtr handle, D2D1TextAntialiasMode textAntialiasMode, bool isFlipModel, bool isOpaque)
        {
            _renderTarget = provider.D2DFactory.CreateHwndRenderTarget(new D2D1RenderTargetProperties()
            {
                Type = D2D1RenderTargetType.Default,
                PixelFormat = new D2D1PixelFormat(Constants.Format, isOpaque ? D2D1AlphaMode.Ignore : D2D1AlphaMode.Premultiplied),
                DpiX = 96,
                DpiY = 96,
                Usage = D2D1RenderTargetUsage.None,
                MinLevel = D2D1FeatureLevel.Default
            }, new D2D1HwndRenderTargetProperties()
            {
                Hwnd = handle,
                PixelSize = new Size(1, 1),
                PresentOptions = isFlipModel ? D2D1PresentOptions.None : D2D1PresentOptions.Immediately
            });
        }

        public D2D1RenderTarget? BeginDraw()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EndDraw()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public GraphicsDeviceProvider GetDeviceProvider()
        {
            throw new NotImplementedException();
        }

        public D2D1RenderTarget GetRenderTarget()
        {
            throw new NotImplementedException();
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Resize(Size size)
        {
            throw new NotImplementedException();
        }

        public void ResizeTemporarily(Size size)
        {
            throw new NotImplementedException();
        }

        public bool TryPresent()
        {
            throw new NotImplementedException();
        }
    }
}
