using System.Drawing;
using System.Runtime.InteropServices;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Describes a cubic bezier in a path.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1BezierSegment
    {
        public PointF Point1;
        public PointF Point2;
        public PointF Point3;

        public D2D1BezierSegment(PointF point1, PointF point2, PointF point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
    }

    /// <summary>
    /// Contains the control point and end point for a quadratic Bezier segment.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1QuadraticBezierSegment
    {
        public PointF Point1;
        public PointF Point2;

        public D2D1QuadraticBezierSegment(PointF point1, PointF point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
    }

    /// <summary>
    /// Describes an arc that is defined as part of a path.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1ArcSegment
    {
        public PointF Point;
        public SizeF Size;
        public float RotationAngle;
        public D2D1SweepDirection SweepDirection;
        public D2D1ArcSize ArcSize;

        public D2D1ArcSegment(PointF point, SizeF size, float rotationAngle,
            D2D1SweepDirection sweepDirection = D2D1SweepDirection.Clockwise, D2D1ArcSize arcSize = D2D1ArcSize.Small)
        {
            Point = point;
            Size = size;
            RotationAngle = rotationAngle;
            SweepDirection = sweepDirection;
            ArcSize = arcSize;
        }
    }
}