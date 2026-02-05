using System;

using ConcreteUI.Graphics.Native.Direct3D;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// This specifies options that apply to the device context for its lifetime.
    /// </summary>
    public enum D2D1DeviceContextOptions : uint
    {
        None = 0,
        /// <summary>
        /// Geometry rendering will be performed on many threads in parallel, a single
        /// thread is the default.
        /// </summary>
        EnableMultithreadedOptimizations = 1
    }

    /// <summary>
    /// This specifies the threading mode used while simultaneously creating the device,
    /// factory, and device context.
    /// </summary>
    public enum D2D1ThreadingMode : uint
    {
        /// <summary>
        /// Resources may only be invoked serially.  Reference counts on resources are
        /// interlocked, however, resource and render target state is not protected from
        /// multi-threaded access
        /// </summary>
        SingleThreaded,
        /// <summary>
        /// Resources may be invoked from multiple threads. Resources use interlocked
        /// reference counting and their state is protected.
        /// </summary>
        MultiThreaded
    }

    /// <summary>
    /// Indicates the debug level to be output by the debug layer.
    /// </summary>
    public enum D2D1DebugLevel : uint
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3
    }

    /// <summary>
    /// Qualifies how alpha is to be treated in a bitmap or render target containing
    /// alpha.
    /// </summary>
    public enum D2D1AlphaMode : uint
    {
        /// <summary>
        /// Alpha mode should be determined implicitly. Some target surfaces do not supply
        /// or imply this information in which case alpha must be specified.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Treat the alpha as premultipled.
        /// </summary>
        Premultiplied = 1,
        /// <summary>
        /// Opacity is in the 'A' component only.
        /// </summary>
        Straight = 2,
        /// <summary>
        /// Ignore any alpha channel information.
        /// </summary>
        Ignore = 3,
    }

    /// <summary>
    /// This determines what gamma is used for interpolation/blending.
    /// </summary>
    public enum D2D1Gamma : uint
    {    /// <summary>
         /// Colors are manipulated in 2.2 gamma color space.
         /// </summary>
        Gamma_2_2 = 0,
        /// <summary>
        /// Colors are manipulated in 1.0 gamma color space.
        /// </summary>
        Gamma_1_0 = 1
    }

    /// <summary>
    /// Enum which describes how to sample from a source outside its base tile.
    /// </summary>
    public enum D2D1ExtendMode : uint
    {
        /// <summary>
        /// Extend the edges of the source out by clamping sample points outside the source
        /// to the edges.
        /// </summary>
        Clamp = 0,
        /// <summary>
        /// The base tile is drawn untransformed and the remainder are filled by repeating
        /// the base tile.
        /// </summary>
        Wrap = 1,
        /// <summary>
        /// The same as wrap, but alternate tiles are flipped  The base tile is drawn
        /// untransformed.
        /// </summary>
        Mirror = 2
    }

    /// <summary>
    /// Specifies the algorithm that is used when images are scaled or rotated. 
    /// </summary>
    /// <remarks>
    /// Note: Starting in Windows 8, more interpolations modes are available. See
    /// <see cref="D2D1InterpolationMode"/> for more info.
    /// </remarks>
    public enum D2D1BitmapInterpolationMode : uint
    {
        /// <summary>
        /// Nearest Neighbor filtering. Also known as nearest pixel or nearest point
        /// sampling.
        /// </summary>
        NearestNeighbor = D2D1InterpolationMode.NearestNeighbor,
        /// <summary>
        /// Linear filtering.
        /// </summary>
        Linear = D2D1InterpolationMode.Linear
    }

    /// <summary>
    /// This defines the superset of interpolation mode supported by D2D APIs
    /// and built-in effects
    /// </summary>
    public enum D2D1InterpolationMode : uint
    {
        /// <summary>
        /// Samples the nearest single point and uses that exact color. This mode uses less processing time, but outputs the lowest quality image.
        /// </summary>
        NearestNeighbor,
        /// <summary>
        /// Uses a four point sample and linear interpolation. This mode uses more processing time than the nearest neighbor mode, but outputs a higher quality image.
        /// </summary>
        Linear,
        /// <summary>
        /// Uses a 16 sample cubic kernel for interpolation. This mode uses the most processing time, but outputs a higher quality image.
        /// </summary>
        Cubic,
        /// <summary>
        /// Uses 4 linear samples within a single pixel for good edge anti-aliasing. This mode is good for scaling down by small amounts on images with few pixels.
        /// </summary>
        MultiSampleLinear,
        /// <summary>
        /// Uses anisotropic filtering to sample a pattern according to the transformed shape of the bitmap.
        /// </summary>
        Anisotropic,
        /// <summary>
        /// Uses a variable size high quality cubic kernel to perform a pre-downscale the image if downscaling is involved in the transform matrix. Then uses the cubic interpolation mode for the final output.
        /// </summary>
        HighQualityCubic
    }

    /// <summary>
    /// Enum which describes the drawing of the ends of a line.
    /// </summary>
    public enum D2D1CapStyle : uint
    {
        /// <summary>
        /// Flat line cap.
        /// </summary>
        Flat = 0,
        /// <summary>
        /// Square line cap.
        /// </summary>
        Square = 1,
        /// <summary>
        /// Round line cap.
        /// </summary>
        Round = 2,
        /// <summary>
        /// Triangle line cap.
        /// </summary>
        Triangle = 3,
    }

    /// <summary>
    /// Defines a color space.
    /// </summary>
    public enum D2D1ColorSpace
    {
        /// <summary>
        /// The color space is described by accompanying data, such as a color profile.
        /// </summary>
        Custom = 0,
        /// <summary>
        /// The sRGB color space.
        /// </summary>
        SRGB = 1,
        /// <summary>
        /// The scRGB color space.
        /// </summary>
        ScRGB = 2
    }

    /// <summary>
    /// This describes how the individual mapping operation should be performed.
    /// </summary>
    [Flags]
    public enum D2D1MapOptions
    {
        /// <summary>
        /// The mapped pointer has undefined behavior.
        /// </summary>
        None = 0,
        /// <summary>
        /// The mapped pointer can be read from.
        /// </summary>
        Read = 1,
        /// <summary>
        /// The mapped pointer can be written to.
        /// </summary>
        Write = 2,
        /// <summary>
        /// The previous contents of the bitmap are discarded when it is mapped.
        /// </summary>
        Discard = 4
    }

    /// <summary>
    /// Enum which describes the drawing of the corners on the line.
    /// </summary>
    public enum D2D1LineJoin : uint
    {
        /// <summary>
        /// Miter join.
        /// </summary>
        Miter = 0,
        /// <summary>
        /// Bevel join.
        /// </summary>
        Bevel = 1,
        /// <summary>
        /// Round join.
        /// </summary>
        Round = 2,
        /// <summary>
        /// Miter/Bevel join.
        /// </summary>
        MiterOrBevel = 3
    }

    /// <summary>
    /// Describes the sequence of dashes and gaps in a stroke.
    /// </summary>
    public enum D2D1DashStyle : uint
    {
        Solid = 0,
        Dash = 1,
        Dot = 2,
        DashDot = 3,
        DashDotDot = 4,
        Custom = 5
    }

    /// <summary>
    /// Modifications made to the draw text call that influence how the text is
    /// rendered.
    /// </summary>
    [Flags]
    public enum D2D1DrawTextOptions : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Do not snap the baseline of the text vertically.
        /// </summary>
        NoSnap = 0x00000001,
        /// <summary>
        /// Clip the text to the content bounds.
        /// </summary>
        Clip = 0x00000002,
        /// <summary>
        /// Render color versions of glyphs if defined by the font.
        /// </summary>
        EnableColorFont = 0x00000004,
        /// <summary>
        /// Bitmap origins of color glyph bitmaps are not snapped.
        /// </summary>
        DisableColorBitmapSnapping = 0x00000008
    }

    /// <summary>
    /// Enum which describes the manner in which we render edges of non-text primitives.
    /// </summary>
    public enum D2D1AntialiasMode : uint
    {
        /// <summary>
        /// The edges of each primitive are antialiased sequentially.
        /// </summary>
        PerPrimitive = 0,
        /// <summary>
        /// Each pixel is rendered if its pixel center is contained by the geometry.
        /// </summary>
        Aliased = 1,
    }

    /// <summary>
    /// Describes the antialiasing mode used for drawing text.
    /// </summary>
    public enum D2D1TextAntialiasMode : uint
    {
        /// <summary>
        /// Render text using the current system setting.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Render text using ClearType.
        /// </summary>
        ClearType = 1,
        /// <summary>
        /// Render text using gray-scale.
        /// </summary>
        Grayscale = 2,
        /// <summary>
        /// Render text aliased.
        /// </summary>
        Aliased = 3,
    }

    /// <summary>
    /// Specifies how the bitmap can be used.
    /// </summary>
    [Flags]
    public enum D2D1BitmapOptions : uint
    {
        /// <summary>
        /// The bitmap is created with default properties.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// The bitmap can be specified as a target in <see cref="D2D1DeviceContext.Target"/>
        /// </summary>
        Target = 0x00000001,
        /// <summary>
        /// The bitmap cannot be used as an input to DrawBitmap, DrawImage, in a bitmap
        /// brush or as an input to an effect.
        /// </summary>
        CannotDraw = 0x00000002,
        /// <summary>
        /// The bitmap can be read from the CPU.
        /// </summary>
        CpuRead = 0x00000004,
        /// <summary>
        /// The bitmap works with the ID2D1GdiInteropRenderTarget::GetDC API.
        /// </summary>
        GdiCompatible = 0x00000008
    }

    /// <summary>
    /// This defines the valid property types that can be used in an effect property
    /// interface.
    /// </summary>
    public enum D2D1PropertyType : uint
    {
        Unknown = 0,
        String = 1,
        Bool = 2,
        UInt32 = 3,
        Int32 = 4,
        Float = 5,
        Vector2 = 6,
        Vector3 = 7,
        Vector4 = 8,
        Blob = 9,
        IUnknown = 10,
        Enum = 11,
        Array = 12,
        CLSID = 13,
        Matrix3x2 = 14,
        Matrix4x3 = 15,
        Matrix4x4 = 16,
        Matrix5x4 = 17,
        ColorContext = 18
    }

    public enum D2D1GaussianBlurProperty : uint
    {
        /// <summary>
        /// Property Name: "StandardDeviation"<br/>
        /// Property Type: <see cref="float"/>
        /// </summary>
        StandardDeviation = 0,

        /// <summary>
        /// Property Name: "Optimization"<br/>
        /// Property Type: <see cref="D2D1GaussianBlurOptimization"/>
        /// </summary>
        Optimization = 1,

        /// <summary>
        /// Property Name: "BorderMode"<br/>
        /// Property Type: <see cref="D2D1BorderMode"/>
        /// </summary>
        BorderMode = 2,
    }

    public enum D2D1GaussianBlurOptimization : uint
    {
        Speed = 0,
        Balanced = 1,
        Quality = 2,
    }

    /// <summary>
    /// Specifies how the Crop effect handles the crop rectangle falling on fractional
    /// pixel coordinates.
    /// </summary>
    public enum D2D1BorderMode : uint
    {
        Soft = 0,
        Hard = 1,
    }

    /// <summary>
    /// This defines the list of system properties present on the root effect property
    /// interface.
    /// </summary>
    public enum D2D1Property : uint
    {
        CLSID = 0x80000000,
        Displayname = 0x80000001,
        Author = 0x80000002,
        Category = 0x80000003,
        Description = 0x80000004,
        Inputs = 0x80000005,
        Cached = 0x80000006,
        Precision = 0x80000007,
        MinInputs = 0x80000008,
        MaxInputs = 0x80000009
    }

    /// <summary>
    /// Specifies the composite mode that will be applied.
    /// </summary>
    public enum D2D1CompositeMode : uint
    {
        SourceOver = 0,
        DestinationOver = 1,
        SourceIn = 2,
        DestinationIn = 3,
        SourceOut = 4,
        DestinationOut = 5,
        SourceAtop = 6,
        DestinationAtop = 7,
        Xor = 8,
        Plus = 9,
        SourceCopy = 10,
        BoundedSourceCopy = 11,
        MaskInvert = 12
    }
    /// <summary>
    /// This enumeration describes the type of combine operation to be performed.
    /// </summary>
    public enum D2D1CombineMode : uint
    {
        /// <summary>
        /// Produce a geometry representing the set of points contained in either the first
        /// or the second geometry.
        /// </summary>
        Union = 0,
        /// <summary>
        /// Produce a geometry representing the set of points common to the first and the
        /// second geometries.
        /// </summary>
        Intersect = 1,
        /// <summary>
        /// Produce a geometry representing the set of points contained in the first
        /// geometry or the second geometry, but not both.
        /// </summary>
        Xor = 2,
        /// <summary>
        /// Produce a geometry representing the set of points contained in the first
        /// geometry but not the second geometry.
        /// </summary>
        Exclude = 3
    }

    /// <summary>
    /// Describes whether a render target uses hardware or software rendering, or if
    /// Direct2D should select the rendering mode.
    /// </summary>
    public enum D2D1RenderTargetType : uint
    {
        /// <summary>
        /// D2D is free to choose the render target type for the caller.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The render target will render using the CPU.
        /// </summary>
        Software = 1,
        /// <summary>
        /// The render target will render using the GPU.
        /// </summary>
        Hardware = 2
    }

    /// <summary>
    /// Describes how a render target is remoted and whether it should be
    /// GDI-compatible. This enumeration allows a bitwise combination of its member
    /// values.
    /// </summary>
    [Flags]
    public enum D2D1RenderTargetUsages : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Rendering will occur locally, if a terminal-services session is established, the
        /// bitmap updates will be sent to the terminal services client.
        /// </summary>
        ForceBitmapRemoting = 0x00000001,
        /// <summary>
        /// The render target will allow a call to GetDC on the ID2D1GdiInteropRenderTarget
        /// interface. Rendering will also occur locally.
        /// </summary>
        GdiCompatible = 0x00000002
    }

    /// <summary>
    /// Describes the minimum DirectX support required for hardware rendering by a
    /// render target.
    /// </summary>
    public enum D2D1FeatureLevel : uint
    {
        /// <summary>
        /// The caller does not require a particular underlying D3D device level.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The D3D device level is DX9 compatible.
        /// </summary>
        Level_9 = D3DFeatureLevel.Level_9_1,
        /// <summary>
        /// The D3D device level is DX10 compatible.
        /// </summary>
        Level_10 = D3DFeatureLevel.Level_10_0,
    }

    /// <summary>
    /// Specified options that can be applied when a layer resource is applied to create a layer.
    /// </summary>
    public enum D2D1LayerOptions : uint
    {
        None = 0x00000000,

        /// <summary>
        /// The layer will render correctly for ClearType text. <br/>
        /// </summary>
        /// <remarks>
        /// If the render target was set to ClearType previously, the layer will continue to render ClearType. <br/>
        /// If the render target was set to ClearType and this option is not specified, the render target will be set to render gray-scale until the layer is popped.<br/>
        /// The caller can override this default by calling <see cref="D2D1RenderTarget.AntialiasMode"/> while within the layer. <br/>
        /// This flag is slightly slower than the default.
        /// </remarks>
        InitializeForClearType = 0x00000001,
    }
}
