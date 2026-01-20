using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl, IElementContainer
    {
        bool IsInitializingElements();

        float GetPointsPerPixel();

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider? GetThemeResourceProvider();
    }
}
