using System.Numerics;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls;

public readonly record struct DpiChangedEventArgs(PointU Dpi, Vector2 PointsPerPixel, Vector2 PixelsPerPoint);

public interface IDpiChangedHandler
{
    void OnDpiChanged(in DpiChangedEventArgs args);
}
