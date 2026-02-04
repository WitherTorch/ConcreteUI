using System;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.DirectComposition
{
    /// <summary>
    /// Serves as the root factory for all other DirectComposition objects and controls transactional composition.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DCompositionDevice : ComObject
    {
        public static readonly Guid IID_IDCompositionDevice = new Guid(0xC37EA93A, 0xE7AA, 0x450D, 0xB1, 0x6F, 0x97, 0x46, 0xCB, 0x04, 0x07, 0xF3);

        public DCompositionDevice() { }

        public DCompositionDevice(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            Commit = _Start,
            WaitForCommitCompletion,
            GetFrameStatistics,
            CreateTargetForHwnd,
            CreateVisual,
            CreateSurface,
            CreateVirtualSurface,
            CreateSurfaceFromHandle,
            CreateSurfaceFromHwnd,
            CreateTranslateTransform,
            CreateScaleTransform,
            CreateRotateTransform,
            CreateSkewTransform,
            CreateMatrixTransform,
            CreateTransformGroup,
            CreateTranslateTransform3D,
            CreateScaleTransform3D,
            CreateRotateTransform3D,
            CreateMatrixTransform3D,
            CreateTransform3DGroup,
            CreateEffectGroup,
            CreateRectangleClip,
            CreateAnimation,
            CheckDeviceState,
            _End
        }

        public void Commit()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Commit);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        public void WaitForCommitCompletion()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.WaitForCommitCompletion);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        public DCompositionTarget CreateTargetForHwnd(IntPtr hwnd, bool topMost)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateTargetForHwnd);
            int hr = ((delegate* unmanaged[Stdcall]<void*, IntPtr, SysBool, void**, int>)functionPointer)(nativePointer, hwnd, topMost, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DCompositionTarget(nativePointer, ReferenceType.Owned);
        }

        public DCompositionVisual CreateVisual()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateVisual);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DCompositionVisual(nativePointer, ReferenceType.Owned);
        }
    }
}
