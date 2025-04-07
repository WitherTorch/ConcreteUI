using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Brushes
{
    public unsafe sealed class D2D1SolidColorBrush : D2D1Brush
    {
        private new enum MethodTable
        {
            _Start = D2D1Brush.MethodTable._End,
            SetColor = _Start,
            GetColor,
            _End
        }

        public D2D1SolidColorBrush() : base() { }

        public D2D1SolidColorBrush(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1ColorF Color
        {
            get => GetColor();
            set => SetColor(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetColor(in D2D1ColorF color)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetColor);
            ((delegate*<void*, D2D1ColorF*, void>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(in color));
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private D2D1ColorF GetColor()
        {
            D2D1ColorF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetColor);
            ((delegate*<void*, D2D1ColorF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
