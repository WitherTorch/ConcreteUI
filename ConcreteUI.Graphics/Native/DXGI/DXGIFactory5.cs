using System;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory5 : DXGIFactory4
    {
        public static readonly Guid IID_DXGIFactory5 = new Guid(0x7632e1f5, 0xee65, 0x4dca, 0x87, 0xfd, 0x84, 0xcd, 0x75, 0xf8, 0x83, 0x8d);

        protected new enum MethodTable
        {
            _Start = DXGIFactory4.MethodTable._End,
            CheckFeatureSupport = _Start,
            _End,
        }

        public DXGIFactory5() : base() { }

        public DXGIFactory5(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public void CheckFeatureSupport(DXGIFeature feature, void* pFeatureSupportData, uint featureSupportDataSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CheckFeatureSupport);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGIFeature, void*, uint, int>)functionPointer)(nativePointer, feature, pFeatureSupportData, featureSupportDataSize);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
