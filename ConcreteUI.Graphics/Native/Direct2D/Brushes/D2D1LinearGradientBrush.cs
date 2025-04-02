using System.Drawing;

using InlineMethod;

using LocalsInit;

using WitherTorch.CrossNative;

namespace ConcreteUI.Graphics.Native.Direct2D.Brushes
{
    public unsafe sealed class D2D1LinearGradientBrush : D2D1Brush
    {
        private new enum MethodTable
        {
            _Start = D2D1Brush.MethodTable._End,
            SetStartPoint = _Start,
            SetEndPoint,
            GetStartPoint,
            GetEndPoint,
            GetGradientStopCollection,
            _End
        }

        public D2D1LinearGradientBrush() : base() { }

        public D2D1LinearGradientBrush(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public PointF StartPoint
        {
            get => GetStartPoint();
            set => SetStartPoint(value);
        }

        public PointF EndPoint
        {
            get => GetEndPoint();
            set => SetEndPoint(value);
        }

        public D2D1GradientStopCollection GradientStopCollection => GetGradientStopCollection();

        [Inline(InlineBehavior.Remove)]
        private void SetStartPoint(PointF startPoint)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetStartPoint);
            ((delegate*<void*, PointF, void>)functionPointer)(nativePointer, startPoint);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetEndPoint(PointF endPoint)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetEndPoint);
            ((delegate*<void*, PointF, void>)functionPointer)(nativePointer, endPoint);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private PointF GetStartPoint()
        {
            PointF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStartPoint);
            ((delegate*<void*, PointF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private PointF GetEndPoint()
        {
            PointF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetEndPoint);
            ((delegate*<void*, PointF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1GradientStopCollection GetGradientStopCollection()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGradientStopCollection);
            ((delegate*<void*, void**, void>)functionPointer)(nativePointer, &nativePointer);
            return nativePointer == null ? null : new D2D1GradientStopCollection(nativePointer, ReferenceType.Owned);
        }
    }
}
