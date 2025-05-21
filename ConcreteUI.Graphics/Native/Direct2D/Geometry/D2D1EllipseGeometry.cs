using System.Runtime.CompilerServices;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    public sealed unsafe class D2D1EllipseGeometry : D2D1Geometry
    {
        private new enum MethodTable
        {
            _Start = D2D1Geometry.MethodTable._End,
            GetEllipse = _Start,
            _End,
        }

        public D2D1EllipseGeometry() { }

        public D2D1EllipseGeometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1Ellipse Ellipse
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetEllipse();
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private D2D1Ellipse GetEllipse()
        {
            D2D1Ellipse result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetEllipse);
            ((delegate* unmanaged[Stdcall]<void*, D2D1Ellipse*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
