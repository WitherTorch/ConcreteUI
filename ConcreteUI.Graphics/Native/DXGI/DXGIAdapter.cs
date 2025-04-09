using System;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIAdapter : DXGIObject
    {
        public static readonly Guid IID_DXGIAdapter = new Guid(0x2411e7e1, 0x12ac, 0x4ccf, 0xbd, 0x14, 0x97, 0x98, 0xe8, 0x53, 0x4d, 0xc0);

        protected new enum MethodTable
        {
            _Start = DXGIObject.MethodTable._End,
            EnumOutputs = _Start,
            GetDesc,
            CheckInterfaceSupport,
            _End,
        }

        public DXGIAdapter() : base() { }

        public DXGIAdapter(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGIAdapterDescription Description
        {
            [LocalsInit(false)]
            get => GetDesc();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGIAdapterDescription GetDesc()
        {
            DXGIAdapterDescription desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGIAdapterDescription*, int>)functionPointer)(nativePointer, &desc);
            ThrowHelper.ThrowExceptionForHR(hr);
            return desc;
        }
    }
}
