using System.Drawing;
using System.Numerics;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl, IElementContainer
    {
        Vector2 GetPointsPerPixel();

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider? GetThemeResourceProvider();
    }
}
