using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct3D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.Direct3D11
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class D3D11Device : ComObject
    {
        public static readonly Guid IID_D3D11Device = new Guid(0xdb6f6ddb, 0xac77, 0x4e88, 0x82, 0x53, 0x81, 0x9d, 0xf9, 0xbb, 0xf1, 0x40);

        public D3D11Device() : base() { }

        public D3D11Device(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [Inline(InlineBehavior.Keep, export: true)]
        public static D3D11Device Create(DXGIAdapter adapter, D3DDriverType driverType, IntPtr software, D3D11CreateDeviceFlag createDeviceFlags)
            => Create(adapter, driverType, software, createDeviceFlags, null, 0u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static D3D11Device Create(DXGIAdapter adapter, D3DDriverType driverType, IntPtr software,
            D3D11CreateDeviceFlag createDeviceFlags, params D3DFeatureLevel[] featureLevels)
        {
            fixed (D3DFeatureLevel* ptr = featureLevels)
                return Create(adapter, driverType, software, createDeviceFlags, ptr, unchecked((uint)featureLevels.Length));
        }

        [LocalsInit(false)]
        public static D3D11Device Create(DXGIAdapter adapter, D3DDriverType driverType, IntPtr software,
            D3D11CreateDeviceFlag createDeviceFlags, D3DFeatureLevel* featureLevels, uint featureLevelCount)
        {
            void* device;
            int hr = D3D11.D3D11CreateDevice(adapter == null ? null : adapter.NativePointer, driverType, software,
                createDeviceFlags, featureLevels, featureLevelCount, D3D11.D3D11_SDK_VERSION, &device, null, null);
            if (hr >= 0)
                return device == null ? null : new D3D11Device(device, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
