using System;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGISurface : DXGIDeviceSubObject
    {
        public static readonly Guid IID_DXGISurface = new Guid(0xcafcb56c, 0x6ac3, 0x4889, 0xbf, 0x47, 0x9e, 0x23, 0xbb, 0xd2, 0x60, 0xec);

        private new enum MethodTable
        {
            _Start = DXGIDeviceSubObject.MethodTable._End,
            GetDesc = _Start,
            Map,
            Unmap,
            _End
        }

        public DXGISurface() : base() { }

        public DXGISurface(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGISurfaceDescription Description
        {
            [LocalsInit(false)]
            get => GetDesc();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGISurfaceDescription GetDesc()
        {
            DXGISurfaceDescription desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGISurfaceDescription*, int>)functionPointer)(nativePointer, &desc);
            ThrowHelper.ThrowExceptionForHR(hr);
            return desc;
        }

        [LocalsInit(false)]
        public DXGIMappedRect Map(DXGIMapFlags flags)
        {
            DXGIMappedRect lockedRect;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Map);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGIMappedRect*, DXGIMapFlags, int>)functionPointer)(nativePointer, &lockedRect, flags);
            ThrowHelper.ThrowExceptionForHR(hr);
            return lockedRect;
        }

        public void Unmap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Unmap);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
