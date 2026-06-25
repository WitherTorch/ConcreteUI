using System.Drawing;
using System.Numerics;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;

namespace ConcreteUI;

public interface IRenderer : IRenderable
{
    Vector2 GetPixelsPerPoint();

    Vector2 GetPointsPerPixel();

    PointF GetMousePosition();

    IThemeResourceProvider? GetThemeResourceProvider();

    void Refresh();

    void Update();
}
