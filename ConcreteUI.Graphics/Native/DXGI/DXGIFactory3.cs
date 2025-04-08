using System;
using System.Security;

using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory3 : DXGIFactory2
    {
        public static readonly Guid IID_DXGIFactory3 = new Guid(0x25483823, 0xcd46, 0x4c7d, 0x86, 0xca, 0x47, 0xaa, 0x95, 0xb8, 0x37, 0xbd);

        protected new enum MethodTable
        {
            _Start = DXGIFactory2.MethodTable._End,
            GetCreationFlags = _Start,
            _End,
        }

        public DXGIFactory3() : base() { }

        public DXGIFactory3(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGICreateFactoryFlags CreationFlags => GetCreationFlags();

        [Inline(InlineBehavior.Remove)]
        private DXGICreateFactoryFlags GetCreationFlags()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetCreationFlags);
            return ((delegate* unmanaged[Stdcall]<void*, DXGICreateFactoryFlags>)functionPointer)(nativePointer);
        }
    }
}
