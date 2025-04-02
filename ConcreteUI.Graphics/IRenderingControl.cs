using ConcreteUI.Graphics.Native.Direct2D;

using WitherTorch.Windows.Graphics;

namespace ConcreteUI.Graphics
{
    public interface IRenderingControl
    {
        D2D1DeviceContext GetDeviceContext();

        RenderingController GetRenderingController();

        void Render(RenderingFlags flags);
    }
}
