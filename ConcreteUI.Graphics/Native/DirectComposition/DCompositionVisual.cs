using System;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;

namespace ConcreteUI.Graphics.Native.DirectComposition
{
    /// <summary>
    /// Represents a visual that participates in a visual tree.
    /// </summary>
    public sealed unsafe class DCompositionVisual : ComObject
    {
        public static readonly Guid IID_IDCompositionVisual = new Guid(0x4d93059d, 0x097b, 0x4651, 0x9a, 0x60, 0xf0, 0xf2, 0x51, 0x16, 0xe2, 0xf3);

        public DCompositionVisual() { }

        public DCompositionVisual(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            SetOffsetX = _Start,
            SetOffsetX_Animation, // SetOffsetX's second overload
            SetOffsetY,
            SetOffsetY_Animation, // SetOffsetY's second overload
            SetTransform,
            SetTransform_Animation, // SetTransform's second overload
            SetTransformParent,
            SetEffect,
            SetBitmapInterpolationMode,
            SetBorderMode,
            SetClip,
            SetClip_Object, // SetClip's second overload
            SetContent,
            AddVisual,
            RemoveVisual,
            RemoveAllVisuals,
            SetCompositeMode,
            _End
        }

        public void SetContent(ComObject? content)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetContent);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, int>)functionPointer)(nativePointer, content is null ? null : content.NativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}