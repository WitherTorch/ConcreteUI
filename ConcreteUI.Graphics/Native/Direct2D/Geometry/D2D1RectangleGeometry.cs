using System.Runtime.CompilerServices;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    public sealed unsafe class D2D1RectangleGeometry : D2D1Geometry
    {
        private new enum MethodTable
        {
            _Start = D2D1Geometry.MethodTable._End,
            GetRect = _Start,
            _End,
        }

        public D2D1RectangleGeometry() { }

        public D2D1RectangleGeometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public RectF Rect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetRect();
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private RectF GetRect()
        {
            RectF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetRect);
            ((delegate* unmanaged[Stdcall]<void*, RectF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
