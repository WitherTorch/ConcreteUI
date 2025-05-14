using System;

namespace ConcreteUI.Graphics.Native.WIC
{
    [Flags]
    public enum WICBitmapDecoderCapabilities : uint
    {
        None = 0x0,
        SameEncoder = 0x1,
        CanDecodeAllImages = 0x2,
        CanDecodeSomeImages = 0x4,
        CanEnumerateMetadata = 0x8,
        CanDecodeThumbnail = 0x10,
    }

    [Flags]
    public enum WICDecodeOptions : uint
    {
        DecodeMetadataCacheOnDemand = 0,
        DecodeMetadataCacheOnLoad = 0x1,
    }
}
