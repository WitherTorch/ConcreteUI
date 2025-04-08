using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory1 : DXGIFactory
    {
        public static readonly Guid IID_DXGIFactory1 = new Guid(0x770aae78, 0xf26f, 0x4dba, 0xa8, 0x29, 0x25, 0x3c, 0x83, 0xd1, 0xb3, 0x87);

        protected new enum MethodTable
        {
            _Start = DXGIFactory.MethodTable._End,
            EnumAdapters1 = _Start,
            IsCurrent,
            _End,
        }

        public DXGIFactory1() : base() { }

        public DXGIFactory1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static new DXGIFactory1 Create(in Guid riid, bool throwException = true)
            => Create(UnsafeHelper.AsPointerIn(in riid), throwException);

        [LocalsInit(false)]
        public static new DXGIFactory1 Create(Guid* riid, bool throwException = true)
        {
            void* factory;
            int hr = DXGI.CreateDXGIFactory1(riid, &factory);
            if (hr >= 0)
                return factory == null ? null : new DXGIFactory1(factory, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }

        public bool IsCurrent => IsCurrentCore();

        public DXGIAdapter1 EnumAdapters1(uint adapter, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapters1);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, adapter, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DXGIAdapter1(nativePointer, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }

        [Inline(InlineBehavior.Remove)]
        private bool IsCurrentCore()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.IsCurrent);
            return ((delegate* unmanaged[Stdcall]<void*, bool>)functionPointer)(nativePointer);
        }
    }
}
