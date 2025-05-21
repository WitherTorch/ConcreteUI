using System.Runtime.CompilerServices;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    public sealed unsafe class D2D1RoundedRectangleGeometry : D2D1Geometry
    {
        private new enum MethodTable
        {
            _Start = D2D1Geometry.MethodTable._End,
            GetRoundedRect = _Start,
            _End,
        }

        public D2D1RoundedRectangleGeometry() { }

        public D2D1RoundedRectangleGeometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1RoundedRectangle RoundedRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetRoundedRect();
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private D2D1RoundedRectangle GetRoundedRect()
        {
            D2D1RoundedRectangle result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetRoundedRect);
            ((delegate* unmanaged[Stdcall]<void*, D2D1RoundedRectangle*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
