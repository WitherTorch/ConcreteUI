using System.Drawing;
using System.Numerics;

using ShioUI.Graphics;
using ShioUI.Theme;

namespace ShioUI;

public interface IRenderer : IRenderable
{
    Vector2 GetPixelsPerPoint();

    Vector2 GetPointsPerPixel();

    PointF GetMousePosition();

    IThemeResourceProvider? GetThemeResourceProvider();

    void Refresh();

    void Update();
}
