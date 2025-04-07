using System;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGIDevice : DXGIObject
    {
        public static readonly Guid IID_DXGIDevice = new Guid(0x54ec77fa, 0x1377, 0x44e6, 0x8c, 0x32, 0x88, 0xfd, 0x5f, 0x44, 0xc8, 0x4c);

        private new enum MethodTable
        {
            _Start = DXGIObject.MethodTable._End,
            GetAdapter = _Start,
            CreateSurface,
            QueryResourceResidency,
            SetGPUThreadPriority,
            GetGPUThreadPriority,
            _End,
        }

        public DXGIDevice() : base() { }

        public DXGIDevice(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public int GPUThreadPriority
        {
            [LocalsInit(false)]
            get => GetGPUThreadPriority();
            set => SetGPUThreadPriority(value);
        }

        public DXGIAdapter GetAdapter()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetAdapter);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DXGIAdapter(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetGPUThreadPriority(int priority)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetGPUThreadPriority);
            int hr = ((delegate*<void*, int, int>)functionPointer)(nativePointer, priority);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private int GetGPUThreadPriority()
        {
            int priority;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGPUThreadPriority);
            int hr = ((delegate*<void*, int*, int>)functionPointer)(nativePointer, &priority);
            if (hr >= 0)
                return priority;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
