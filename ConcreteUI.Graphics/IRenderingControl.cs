using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

namespace ConcreteUI.Graphics
{
    public interface IRenderingControl
    {
        DXGISwapChain GetSwapChain();

        D2D1DeviceContext GetDeviceContext();

        RenderingController? GetRenderingController();

        void Render(RenderingFlags flags);
    }
}
