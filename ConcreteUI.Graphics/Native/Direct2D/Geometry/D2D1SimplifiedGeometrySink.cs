using InlineMethod;

using System.Drawing;
using System.Runtime.CompilerServices;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Describes a geometric path that does not contain quadratic bezier curves or
    /// arcs.
    /// </summary>
    public unsafe class D2D1SimplifiedGeometrySink : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            SetFillMode = _Start,
            SetSegmentFlags,
            BeginFigure,
            AddLines,
            AddBeziers,
            EndFigure,
            Close,
            _End,
        }

        public D2D1SimplifiedGeometrySink() : base() { }

        public D2D1SimplifiedGeometrySink(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public void SetFillMode(D2D1FillMode fillMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFillMode);
            ((delegate* unmanaged[Stdcall]<void*, D2D1FillMode, void>)functionPointer)(nativePointer, fillMode);
        }

        public void SetSegmentFlags(D2D1PathSegment vertexFlags)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetSegmentFlags);
            ((delegate* unmanaged[Stdcall]<void*, D2D1PathSegment, void>)functionPointer)(nativePointer, vertexFlags);
        }

        public void BeginFigure(PointF startPoint, D2D1FigureBegin figureBegin)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.BeginFigure);
            ((delegate* unmanaged[Stdcall]<void*, PointF, D2D1FigureBegin, void>)functionPointer)(nativePointer, startPoint, figureBegin);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void AddLines(PointF point)
            => AddLines(&point, 1u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLines(params PointF[] points)
        {
            fixed (PointF* ptr = points)
                AddLines(ptr, unchecked((uint)points.Length));
        }

        public void AddLines(PointF* points, uint pointsCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddLines);
            ((delegate* unmanaged[Stdcall]<void*, PointF*, uint, void>)functionPointer)(nativePointer, points, pointsCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBeziers(in D2D1BezierSegment bezier)
            => AddBeziers(UnsafeHelper.AsPointerIn(in bezier), 1u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBeziers(params D2D1BezierSegment[] beziers)
        {
            fixed (D2D1BezierSegment* ptr = beziers)
                AddBeziers(ptr, unchecked((uint)beziers.Length));
        }

        public void AddBeziers(D2D1BezierSegment* beziers, uint beziersCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddBeziers);
            ((delegate* unmanaged[Stdcall]<void*, D2D1BezierSegment*, uint, void>)functionPointer)(nativePointer, beziers, beziersCount);
        }

        public void EndFigure(D2D1FigureEnd figureEnd)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EndFigure);
            ((delegate* unmanaged[Stdcall]<void*, D2D1FigureEnd, void>)functionPointer)(nativePointer, figureEnd);
        }

        public void Close()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Close);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}