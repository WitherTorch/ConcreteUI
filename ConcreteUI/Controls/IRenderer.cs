using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl
    {
        bool IsInitializingElements();

        float GetBaseLineWidth();

        void RenderElementBackground(UIElement element, D2D1DeviceContext context);

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider GetThemeResourceProvider();
    }
}
