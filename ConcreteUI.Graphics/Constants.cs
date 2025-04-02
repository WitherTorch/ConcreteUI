using ConcreteUI.Graphics.Native.Direct3D;
using ConcreteUI.Graphics.Native.DXGI;

namespace ConcreteUI.Graphics
{
    public static class Constants
    {
        public const bool UsePreciseSleepFunction = false;

        public const int S_OK = unchecked(0);
        public const int E_OUTOFMEMORY = unchecked((int)0x8007000E);
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int E_INSUFFICIENT_BUFFER = unchecked((int)0x8007007A);
        public const int DXGI_ERROR_DEVICE_REMOVED = unchecked((int)0x887A0005);
        public const int DXGI_ERROR_NOT_FOUND = unchecked((int)0x887A0002);
        public const int DXGI_STATUS_OCCLUDED = unchecked(0x087A0001);
        public const uint AdapterEnumerationLimit = 256u;
        public const float D2D1_DEFAULT_FLATTENING_TOLERANCE = 0.25f;
        public const DXGIFormat Format = DXGIFormat.B8G8R8A8_UNorm;
        public static readonly D3DFeatureLevel[] FeatureLevels =
        [
            D3DFeatureLevel.Level_11_1,
            D3DFeatureLevel.Level_11_0,
            D3DFeatureLevel.Level_10_1,
            D3DFeatureLevel.Level_10_0,
            D3DFeatureLevel.Level_9_3,
            D3DFeatureLevel.Level_9_2,
            D3DFeatureLevel.Level_9_1,
        ];
    }
}
