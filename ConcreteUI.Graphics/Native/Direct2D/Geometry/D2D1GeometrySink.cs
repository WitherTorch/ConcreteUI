using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Describes a geometric path that can contain lines, arcs, cubic Bezier curves,
    /// and quadratic Bezier curves.
    /// </summary>
    public unsafe sealed class D2D1GeometrySink : D2D1SimplifiedGeometrySink
    {
        private new enum MethodTable
        {
            _Start = D2D1SimplifiedGeometrySink.MethodTable._End,
            AddLine = _Start,
            AddBezier,
            AddQuadraticBezier,
            AddQuadraticBeziers,
            AddArc,
            _End
        }

        public D2D1GeometrySink() : base() { }

        public D2D1GeometrySink(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public void AddLine(PointF point)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddLine);
            ((delegate*<void*, PointF, void>)functionPointer)(nativePointer, point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBezier(in D2D1BezierSegment bezier)
            => AddBezier(UnsafeHelper.AsPointerIn(in bezier));

        public void AddBezier(D2D1BezierSegment* bezier)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddBezier);
            ((delegate*<void*, D2D1BezierSegment*, void>)functionPointer)(nativePointer, bezier);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddQuadraticBezier(in D2D1QuadraticBezierSegment bezier)
            => AddQuadraticBezier(UnsafeHelper.AsPointerIn(in bezier));

        public void AddQuadraticBezier(D2D1QuadraticBezierSegment* bezier)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddQuadraticBezier);
            ((delegate*<void*, D2D1QuadraticBezierSegment*, void>)functionPointer)(nativePointer, bezier);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddQuadraticBeziers(in D2D1QuadraticBezierSegment bezier)
            => AddQuadraticBeziers(UnsafeHelper.AsPointerIn(in bezier), 1u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddQuadraticBeziers(params D2D1QuadraticBezierSegment[] beziers)
        {
            fixed (D2D1QuadraticBezierSegment* pBeziers = beziers)
                AddQuadraticBeziers(pBeziers, unchecked((uint)beziers.Length));
        }

        public void AddQuadraticBeziers(D2D1QuadraticBezierSegment* beziers, uint beziersCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddQuadraticBeziers);
            ((delegate*<void*, D2D1QuadraticBezierSegment*, uint, void>)functionPointer)(nativePointer, beziers, beziersCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddArc(in D2D1ArcSegment arc)
            => AddArc(UnsafeHelper.AsPointerIn(in arc));

        public void AddArc(D2D1ArcSegment* arc)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddArc);
            ((delegate*<void*, D2D1ArcSegment*, void>)functionPointer)(nativePointer, arc);
        }
    }
}