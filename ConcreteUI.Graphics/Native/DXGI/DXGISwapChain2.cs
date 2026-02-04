using System;
using System.Numerics;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

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

        public uint MaximumFrameLatency
        {
            [LocalsInit(false)]
            get => GetMaximumFrameLatency();
            set => SetMaximumFrameLatency(value);
        }

        public Matrix3x2 MatrixTransform
        {
            [LocalsInit(false)]
            get => GetMatrixTransform();
            set => SetMatrixTransform(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetSourceSize(SizeU size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetSourceSize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint, int>)functionPointer)(nativePointer, size.Width, size.Height);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private SizeU GetSourceSize()
        {
            SizeU size;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSourceSize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, uint*, int>)functionPointer)(nativePointer, (uint*)&size - 1, (uint*)&size);
            ThrowHelper.ThrowExceptionForHR(hr);
            return size;
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMaximumFrameLatency(uint value)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMaximumFrameLatency);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, int>)functionPointer)(nativePointer, value);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private uint GetMaximumFrameLatency()
        {
            uint result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaximumFrameLatency);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        public IntPtr GetFrameLatencyWaitableObject()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFrameLatencyWaitableObject);
            return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMatrixTransform(in Matrix3x2 value)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMatrixTransform);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, int>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(in value));
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private Matrix3x2 GetMatrixTransform()
        {
            Matrix3x2 result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMatrixTransform);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }
    }
}
