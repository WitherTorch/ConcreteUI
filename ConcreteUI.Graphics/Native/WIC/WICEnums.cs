using System;

namespace ConcreteUI.Graphics.Native.WIC
{
    /// <summary>
    /// Specifies the capabilities of the decoder.
    /// </summary>
    [Flags]
    public enum WICBitmapDecoderCapabilities : uint
    {
        /// <summary>
        /// No capabilities specified. This is the default value.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Decoder recognizes the image was encoded with an encoder produced by the same vendor.
        /// </summary>
        SameEncoder = 0x1,
        /// <summary>
        /// Decoder can decode all the images within an image container.
        /// </summary>
        CanDecodeAllImages = 0x2,
        /// <summary>
        /// Decoder can decode some of the images within an image container.
        /// </summary>
        CanDecodeSomeImages = 0x4,
        /// <summary>
        /// Decoder can enumerate the metadata blocks within a container format.
        /// </summary>
        CanEnumerateMetadata = 0x8,
        /// <summary>
        /// Decoder can find and decode a thumbnail.
        /// </summary>
        CanDecodeThumbnail = 0x10,
    }

    /// <summary>
    /// Specifies decode options.
    /// </summary>
    [Flags]
    public enum WICDecodeOptions : uint
    {
        /// <summary>
        /// Cache metadata when needed.
        /// </summary>
        CacheMetadataOnDemand = 0,
        /// <summary>
        /// Cache metadata when decoder is loaded.
        /// </summary>
        CacheMetadataOnLoad = 0x1,
    }

    /// <summary>
    /// Specifies the sampling or filtering mode to use when scaling an image.
    /// </summary>
    public enum WICBitmapInterpolationMode : uint
    {
        /// <summary>
        /// Nearest neighbor interpolation algorithm. Also known as nearest pixel or point interpolation.
        /// </summary>
        /// <remarks>
        /// The output pixel is assigned the value of the pixel that the point falls within. No other pixels are considered.
        /// </remarks>
        NearestNeighbor = 0,
        /// <summary>
        /// Bilinear interpolation algorithm.
        /// </summary>
        /// <remarks>
        /// The output pixel values are computed as a weighted average of the nearest four pixels in a 2x2 grid.
        ///</remarks>
        Linear = 0x1,
        /// <summary>
        /// Bicubic interpolation algorithm.
        /// </summary>
        /// <remarks>
        /// Destination pixel values are computed as a weighted average of the nearest sixteen pixels in a 4x4 grid.
        /// </remarks>
        Cubic = 0x2,
        /// <summary>
        /// Fant resampling algorithm.
        /// </summary>
        /// <remarks>
        /// Destination pixel values are computed as a weighted average of the all the pixels that map to the new pixel.
        /// </remarks>
        Fant = 0x3,
        /// <summary>
        /// High quality bicubic interpolation algorithm
        /// </summary>
        /// <remarks>
        /// Destination pixel values are computed using a much denser sampling kernel than regular cubic. <br/>
        /// The kernel is resized in response to the scale factor, making it suitable for downscaling by factors greater than 2. <br/>
        /// <br/>
        /// <b>Note: This value is supported beginning with Windows 10.</b>
        /// </remarks>
        HighQualityCubic = 0x4,
    }

    /// <summary>
    /// Specifies the flip and rotation transforms.
    /// </summary>
    public enum WICBitmapTransformOptions : uint
    {
        /// <summary>
        /// Rotation of 0 degrees.
        /// </summary>
        Rotate0 = 0,
        /// <summary>
        /// Clockwise rotation of 90 degrees.
        /// </summary>
        Rotate90 = 0x1,
        /// <summary>
        /// Clockwise rotation of 180 degrees.
        /// </summary>
        Rotate180 = 0x2,
        /// <summary>
        /// Clockwise rotation of 270 degrees.
        /// </summary>
        Rotate270 = 0x3,
        /// <summary>
        /// Horizontal flip. Pixels are flipped around the vertical y-axis.
        /// </summary>
        FlipHorizontal = 0x8,
        /// <summary>
        /// Vertical flip. Pixels are flipped around the horizontal x-axis.
        /// </summary>
        FlipVertical = 0x10,
    }
}
