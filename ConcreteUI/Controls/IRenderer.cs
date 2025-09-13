using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl
    {
        bool IsInitializingElements();

        float GetPointsPerPixel();

        float GetBaseLineWidth();

        void RenderElementBackground(UIElement element, in RegionalRenderingContext context);

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider? GetThemeResourceProvider();
    }
}
