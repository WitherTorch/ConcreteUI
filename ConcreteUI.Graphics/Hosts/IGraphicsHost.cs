using System;
using System.Drawing;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Hosts
{
    public interface IGraphicsHost : ICheckableDisposable
    {

        event EventHandler? DeviceRemoved;

        nint AssociatedWindowHandle { get; }

        D2D1RenderTarget? BeginDraw();
        void EndDraw();
        void Flush();
        D2D1RenderTarget GetRenderTarget();
        GraphicsDeviceProvider GetDeviceProvider();
        void Resize(Size size);
        void ResizeTemporarily(Size size);
        void Present();
        bool TryPresent();
    }

    public interface IOptimizedGraphicsHost : IGraphicsHost
    {
        unsafe void Present(Rect* dirtyRects, uint count);

        unsafe bool TryPresent(Rect* dirtyRects, uint count);
    }

    public interface ISwapChainGraphicsHost : IGraphicsHost
    {
        DXGISwapChain GetSwapChain();
    }
}