using System;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGIAdapter1 : DXGIAdapter
    {
        public static readonly Guid IID_IDXGIAdapter1 = new Guid(0x29038f61, 0x3839, 0x4626, 0x91, 0xfd, 0x08, 0x68, 0x79, 0x01, 0x1a, 0x05);

        private new enum MethodTable
        {
            _Start = DXGIAdapter.MethodTable._End,
            GetDesc1 = _Start,
            _End
        }

        public DXGIAdapter1() : base() { }

        public DXGIAdapter1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGIAdapterDescription1 Description1
        {
            [LocalsInit(false)]
            get => GetDesc1();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGIAdapterDescription1 GetDesc1()
        {
            DXGIAdapterDescription1 desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc1);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGIAdapterDescription1*, int>)functionPointer)(nativePointer, &desc);
            ThrowHelper.ThrowExceptionForHR(hr);
            return desc;
        }
    }
}
