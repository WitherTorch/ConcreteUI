using System;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct3D;

namespace ConcreteUI.Graphics.Native.Direct3D11
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class D3D11
    {
        private const string LibraryName = "d3d11.dll";
        public const uint D3D11_SDK_VERSION = 7U;

#if NET8_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibraryName)]
        public static extern int D3D11CreateDevice(void* pAdapter, D3DDriverType DriverType, IntPtr Software, D3D11CreateDeviceFlags Flags,
            D3DFeatureLevel* pFeatureLevels, uint FeatureLevels, uint SDKVersion, void** ppDevice, D3DFeatureLevel* pFeatureLevel, void** ppImmediateContext);
    }
}
