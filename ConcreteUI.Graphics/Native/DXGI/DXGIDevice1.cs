using System;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGIDevice1 : DXGIDevice
    {
        public static readonly Guid IID_IDXGIDevice1 = new Guid(0x77db970f, 0x6276, 0x48ba, 0xba, 0x28, 0x07, 0x01, 0x43, 0xb4, 0x39, 0x2c);

        private new enum MethodTable
        {
            _Start = DXGIDevice.MethodTable._End,
            SetMaximumFrameLatency = _Start,
            GetMaximumFrameLatency,
            _End,
        }

        public DXGIDevice1() : base() { }

        public DXGIDevice1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public uint MaximumFrameLatency
        {
            [LocalsInit(false)]
            get => GetMaximumFrameLatency();
            set => SetMaximumFrameLatency(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMaximumFrameLatency(uint value)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMaximumFrameLatency);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, int>)functionPointer)(nativePointer, value);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private uint GetMaximumFrameLatency()
        {
            uint result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaximumFrameLatency);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }
    }
}
