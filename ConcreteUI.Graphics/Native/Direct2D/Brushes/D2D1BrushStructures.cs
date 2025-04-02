using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ConcreteUI.Graphics.Native.Direct2D.Brushes
{
    /// <summary>
    /// Describes the opacity and transformation of a brush.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1BrushProperties
    {
        public float Opacity;
        public Matrix3x2 Transform;

        public D2D1BrushProperties(float opacity)
        {
            Opacity = opacity;
            Transform = default;
        }
    }

    /// <summary>
    /// Contains the position and color of a gradient stop.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1GradientStop
    {
        public float Position;
        public D2D1ColorF Color;

        public D2D1GradientStop(float position, D2D1ColorF color)
        {
            Position = position;
            Color = color;
        }
    }

    /// <summary>
    /// Contains the starting point and endpoint of the gradient axis for an
    /// <see cref="D2D1LinearGradientBrush"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1LinearGradientBrushProperties
    {
        public PointF StartPoint;
        public PointF EndPoint;

        public D2D1LinearGradientBrushProperties(PointF startPoint, PointF endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

    }

    /// <summary>
    /// Describes the extend modes and the interpolation mode of an <see cref="D2D1BitmapBrush"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1BitmapBrushProperties
    {
        public D2D1ExtendMode ExtendModeX;
        public D2D1ExtendMode ExtendModeY;
        public D2D1BitmapInterpolationMode InterpolationMode;

        public D2D1BitmapBrushProperties(D2D1ExtendMode extendMode, D2D1BitmapInterpolationMode interpolationMode)
        {
            ExtendModeX = extendMode;
            ExtendModeY = extendMode;
            InterpolationMode = interpolationMode;
        }

        public D2D1BitmapBrushProperties(D2D1ExtendMode extendModeX, D2D1ExtendMode extendModeY, D2D1BitmapInterpolationMode interpolationMode)
        {
            ExtendModeX = extendModeX;
            ExtendModeY = extendModeY;
            InterpolationMode = interpolationMode;
        }
    }

    /// <summary>
    /// Contains the gradient origin offset and the size and position of the gradient
    /// ellipse for an <see cref="D2D1RadialGradientBrush"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1RadialGradientBrushProperties
    {
        public PointF Center;
        public PointF GradientOriginOffset;
        public float RadiusX;
        public float RadiusY;

        public D2D1RadialGradientBrushProperties(PointF center, PointF gradientOriginOffset, float radiusX, float radiusY)
        {
            Center = center;
            GradientOriginOffset = gradientOriginOffset;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }
    }
}
