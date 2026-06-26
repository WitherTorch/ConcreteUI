using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.DXGI;

namespace ShioUI.Graphics;

public interface IRenderable
{
    DXGISwapChain GetSwapChain();

    D2D1DeviceContext GetDeviceContext();

    RenderingController? GetRenderingController();

    void Render(RenderingController controller);
}
