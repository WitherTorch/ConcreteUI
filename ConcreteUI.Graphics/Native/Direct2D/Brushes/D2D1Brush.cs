using System.Numerics;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Brushes
{
    /// <summary>
    /// The root brush interface. All brushes can be used to fill or pen a geometry.
    /// </summary>
    public unsafe class D2D1Brush : D2D1Resource
    {
        protected new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            SetOpacity = _Start,
            SetTransform,
            GetOpacity,
            GetTransform,
            _End,
        }

        public D2D1Brush() : base() { }

        public D2D1Brush(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets or sets the opacity for when the brush is drawn over the entire fill of the brush.
        /// </summary>
        public float Opacity
        {
            get => GetOpacity();
            set => SetOpacity(value);
        }

        /// <summary>
        /// Gets or sets the transform that applies to everything drawn by the brush.
        /// </summary>
        public Matrix3x2 Transform
        {
            get => GetTransform();
            set => SetTransform(&value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetOpacity(float opacity)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetOpacity);
            ((delegate* unmanaged[Stdcall]<void*, float, void>)functionPointer)(nativePointer, opacity);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetTransform(Matrix3x2* transform)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetTransform);
            ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, void>)functionPointer)(nativePointer, transform);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetOpacity()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetOpacity);
            return ((delegate* unmanaged[Stdcall]<void*, float>)functionPointer)(nativePointer);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private Matrix3x2 GetTransform()
        {
            Matrix3x2 result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetTransform);
            ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
