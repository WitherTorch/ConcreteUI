using System.Drawing;
using System.Numerics;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Theme;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI;

public interface IRenderWindow : IElementContainer
{
    D2D1DeviceContext GetDeviceContext();

    Vector2 GetPixelsPerPoint();

    Vector2 GetPointsPerPixel();

    IThemeResourceProvider? GetDefaultThemeResourceProvider();

    IThemeResourceProvider CreateThemeResourceProvider(IThemeContext context);

    Point InnerPageToPage(Point point);

    PointF InnerPageToPage(PointF point);

    Point PageToInnerPage(Point point);

    PointF PageToInnerPage(PointF point);

    RenderResult RenderPage(in RegionalRenderingContext context, in RenderInformation information);

    void Refresh();

    void Update();
}
