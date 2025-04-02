using System;
using System.Drawing;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.CrossNative.Windows.Structures;

using GDIColor = System.Drawing.Color;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// This specifies the options while simultaneously creating the device, factory,
    /// and device context.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1CreationProperties
    {
        /// <summary>
        /// Describes locking behavior of D2D resources
        /// </summary>
        public D2D1ThreadingMode ThreadingMode;
        public D2D1DebugLevel DebugLevel;
        public D2D1DeviceContextOptions Options;
    }

    /// <summary>
    /// Describes the pixel format and dpi of a bitmap.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1BitmapProperties
    {
        public D2D1PixelFormat PixelFormat;
        public float DpiX;
        public float DpiY;

        public D2D1BitmapProperties(D2D1PixelFormat pixelFormat)
        {
            PixelFormat = pixelFormat;
            DpiX = default;
            DpiY = default;
        }

        public D2D1BitmapProperties(D2D1PixelFormat pixelFormat, float dpiX, float dpiY)
        {
            PixelFormat = pixelFormat;
            DpiX = dpiX;
            DpiY = dpiY;
        }
    }

    /// <summary>
    /// Extended bitmap properties.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct D2D1BitmapProperties1
    {
        public D2D1PixelFormat PixelFormat;
        public float DpiX;
        public float DpiY;
        /// <summary>
        /// Specifies how the bitmap can be used.
        /// </summary>
        public D2D1BitmapOptions BitmapOptions;
        public void* ColorContext;

        public D2D1BitmapProperties1(D2D1PixelFormat pixelFormat, D2D1BitmapOptions bitmapOptions)
        {
            PixelFormat = pixelFormat;
            DpiX = default;
            DpiY = default;
            BitmapOptions = bitmapOptions;
            ColorContext = null;
        }

        public D2D1BitmapProperties1(D2D1PixelFormat pixelFormat, float dpiX, float dpiY, D2D1BitmapOptions bitmapOptions)
        {
            PixelFormat = pixelFormat;
            DpiX = dpiX;
            DpiY = dpiY;
            BitmapOptions = bitmapOptions;
            ColorContext = null;
        }
    }

    /// <summary>
    /// Description of a pixel format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1PixelFormat
    {
        public DXGIFormat Format;
        public D2D1AlphaMode AlphaMode;

        public D2D1PixelFormat(DXGIFormat format, D2D1AlphaMode alphaMode)
        {
            Format = format;
            AlphaMode = alphaMode;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1ColorF
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public D2D1ColorF(float r, float g, float b, float a = 1.0f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public D2D1ColorF(int r, int g, int b, int a = 255)
        {
            R = r / 255.0f;
            G = g / 255.0f;
            B = b / 255.0f;
            A = a / 255.0f;
        }

        public static implicit operator D2D1ColorF(GDIColor color)
        {
            return new D2D1ColorF(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        public static explicit operator GDIColor(D2D1ColorF color)
        {
            return GDIColor.FromArgb((int)Math.Floor(color.A * 255.0f), (int)Math.Floor(color.R * 255.0f), (int)Math.Floor(color.G * 255.0f), (int)Math.Floor(color.B * 255.0f));
        }

        public static bool operator ==(D2D1ColorF a, D2D1ColorF b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(D2D1ColorF a, D2D1ColorF b)
        {
            return a.R != b.R || a.G != b.G || a.B != b.B || a.A != b.A;
        }

        public override bool Equals(object obj)
        {
            if (obj is D2D1ColorF color)
                return this == color;
            return base.Equals(obj);
        }

        public override unsafe readonly int GetHashCode()
        {
            float a = A, r = R, g = G, b = B;
            return *(int*)&a ^ *(int*)&r ^ *(int*)&g ^ *(int*)&b;
        }
    }

    /// <summary>
    /// Contains the dimensions and corner radii of a rounded rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1RoundedRectangle
    {
        public RectF Rect;
        public float RadiusX;
        public float RadiusY;

        public D2D1RoundedRectangle(RectF rect, float radiusX, float radiusY)
        {
            Rect = rect;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }
    }

    /// <summary>
    /// Contains the center point, x-radius, and y-radius of an ellipse.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1Ellipse
    {
        public PointF Point;
        public float RadiusX;
        public float RadiusY;

        public D2D1Ellipse(PointF point, float radiusX, float radiusY)
        {
            Point = point;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }
    }

    /// <summary>
    /// Describes a triangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1Triangle
    {
        public PointF Point1;
        public PointF Point2;
        public PointF Point3;

        public D2D1Triangle(PointF point1, PointF point2, PointF point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
    }

    /// <summary>
    /// Properties, aside from the width, that allow geometric penning to be specified.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1StrokeStyleProperties
    {
        public D2D1CapStyle StartCap;
        public D2D1CapStyle EndCap;
        public D2D1CapStyle DashCap;
        public D2D1LineJoin LineJoin;
        public float MiterLimit;
        public D2D1DashStyle DashStyle;
        public float DashOffset;
    }

    /// <summary>
    /// Describes mapped memory from the <see cref="D2D1Bitmap1.Map(D2D1MapOptions)"/> API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct D2D1MappedRect
    {
        public uint Pitch;
        public byte* Bits;
    }

    /// <summary>
    /// Contains rendering options (hardware or software), pixel format, DPI
    /// information, remoting options, and Direct3D support requirements for a render
    /// target.
    /// </summary>    
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1RenderTargetProperties
    {
        public D2D1RenderTargetType Type;
        public D2D1PixelFormat PixelFormat;
        public float DpiX;
        public float DpiY;
        public D2D1RenderTargetUsages Usages;
        public D2D1FeatureLevel MinLevel;
    }
}
