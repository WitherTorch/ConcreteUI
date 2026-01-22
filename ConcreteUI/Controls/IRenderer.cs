using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl, IElementContainer
    {
        float GetPointsPerPixel();

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider? GetThemeResourceProvider();
    }
}
