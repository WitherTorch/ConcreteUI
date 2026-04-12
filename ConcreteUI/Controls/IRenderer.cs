using System.Drawing;
using System.Numerics;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI.Controls
{
    public interface IRenderer : IRenderingControl
    {
        Vector2 GetPixelsPerPoint();

        Vector2 GetPointsPerPixel();

        ToolTip? GetToolTip();

        PointF GetMousePosition();

        IThemeResourceProvider? GetThemeResourceProvider();

        void Refresh();
    }
}
