using System;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGISwapChain2 : DXGISwapChain1
    {
        public static readonly Guid IID_IDXGISwapChain2 = new Guid(0xa8be2ac4, 0x199f, 0x4946, 0xb3, 0x31, 0x79, 0x59, 0x9f, 0xb9, 0x8d, 0xe7);

        private new enum MethodTable
        {
            _Start = DXGISwapChain1.MethodTable._End,
            SetSourceSize = _Start,
            GetSourceSize,
            SetMaximumFrameLatency,
            GetMaximumFrameLatency,
            GetFrameLatencyWaitableObject,
            SetMatrixTransform,
            GetMatrixTransform,
            _End
        }

        public DXGISwapChain2() : base() { }

        public DXGISwapChain2(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public SizeU SourceSize
        {
            [LocalsInit(false)]
            get => GetSourceSize();
            set => SetSourceSize(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetSourceSize(SizeU size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetSourceSize);
            int hr = ((delegate*<void*, uint, uint, int>)functionPointer)(nativePointer, size.Width, size.Height);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private SizeU GetSourceSize()
        {
            SizeU size;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSourceSize);
            int hr = ((delegate*<void*, uint*, uint*, int>)functionPointer)(nativePointer, (uint*)&size - 1, (uint*)&size);
            if (hr >= 0)
                return size;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
