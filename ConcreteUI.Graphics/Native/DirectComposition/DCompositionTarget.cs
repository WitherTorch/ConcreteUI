using System;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;

namespace ConcreteUI.Graphics.Native.DirectComposition
{
    /// <summary>
    /// Represents a binding between a DirectComposition visual tree and a destination on top of which the visual tree should be composed.
    /// </summary>
    public sealed unsafe class DCompositionTarget : ComObject
    {
        public static readonly Guid IID_IDCompositionTarget = new Guid(0xeacdd04c, 0x117e, 0x4e17, 0x88, 0xf4, 0xd1, 0xb1, 0x2b, 0x0e, 0x3d, 0x89);

        public DCompositionTarget() { }

        public DCompositionTarget(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            SetRoot = _Start,
            _End
        }

        public void SetRoot(DCompositionVisual visual)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetRoot);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, int>)functionPointer)(nativePointer, visual.NativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
