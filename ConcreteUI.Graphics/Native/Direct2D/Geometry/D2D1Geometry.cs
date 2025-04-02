using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineMethod;

using LocalsInit;

using WitherTorch.CrossNative;
using WitherTorch.CrossNative.Helpers;
using WitherTorch.CrossNative.Windows.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    public unsafe class D2D1Geometry : D2D1Resource
    {
        protected new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            GetBounds = _Start,
            GetWidenedBounds,
            StrokeContainsPoint,
            FillContainsPoint,
            CompareWithGeometry,
            Simplify,
            Tessellate,
            CombineWithGeometry,
            Outline,
            ComputeArea,
            ComputeLength,
            ComputePointAtLength,
            Widen,
            _End
        }

        public D2D1Geometry() : base() { }

        public D2D1Geometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Retrieve the bounds of the geometry.
        /// </summary>
        [Inline(InlineBehavior.Keep, export: true)]
        public RectF GetBounds() => GetBounds(null);

        /// <inheritdoc cref="GetBounds(Matrix3x2*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RectF GetBounds(in Matrix3x2 worldTransform)
            => GetBounds(UnsafeHelper.AsPointerIn(in worldTransform));

        /// <summary>
        /// Retrieve the bounds of the geometry, with an optional applied transform.
        /// </summary>
        [LocalsInit(false)]
        public RectF GetBounds(Matrix3x2* worldTransform)
        {
            RectF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBounds);
            int hr = ((delegate*<void*, Matrix3x2*, RectF*, int>)functionPointer)(nativePointer, worldTransform, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="GetWidenedBounds(float, D2D1StrokeStyle, Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance)
            => GetWidenedBounds(strokeWidth, strokeStyle, null, flatteningTolerance);

        /// <inheritdoc cref="GetWidenedBounds(float, D2D1StrokeStyle, Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance)
            => GetWidenedBounds(strokeWidth, strokeStyle, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance);

        /// <summary>
        /// Get the bounds of the corresponding geometry after it has been widened or have
        /// an optional pen style applied.
        /// </summary>
        [LocalsInit(false)]
        public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance)
        {
            RectF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetWidenedBounds);
            int hr = ((delegate*<void*, float, void*, Matrix3x2*, float, RectF*, int>)functionPointer)(nativePointer, strokeWidth,
                strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="StrokeContainsPoint(PointF, float, D2D1StrokeStyle, Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public bool StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance)
            => StrokeContainsPoint(point, strokeWidth, strokeStyle, null, flatteningTolerance);

        /// <inheritdoc cref="StrokeContainsPoint(PointF, float, D2D1StrokeStyle, Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance)
            => StrokeContainsPoint(point, strokeWidth, strokeStyle, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance);

        /// <summary>
        /// Checks to see whether the corresponding penned and widened geometry contains the
        /// given point.
        /// </summary>
        [LocalsInit(false)]
        public bool StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance)
        {
            bool result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.StrokeContainsPoint);
            int hr = ((delegate*<void*, PointF, float, void*, Matrix3x2*, float, bool*, int>)functionPointer)(nativePointer, point, strokeWidth,
                strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="FillContainsPoint(PointF, Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public bool FillContainsPoint(PointF point, float flatteningTolerance)
            => FillContainsPoint(point, null, flatteningTolerance);

        /// <inheritdoc cref="FillContainsPoint(PointF, Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FillContainsPoint(PointF point, in Matrix3x2 worldTransform, float flatteningTolerance)
            => FillContainsPoint(point, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance);

        /// <summary>
        /// Test whether the given fill of this geometry would contain this point.
        /// </summary>
        [LocalsInit(false)]
        public bool FillContainsPoint(PointF point, Matrix3x2* worldTransform, float flatteningTolerance)
        {
            bool result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillContainsPoint);
            int hr = ((delegate*<void*, PointF, Matrix3x2*, float, bool*, int>)functionPointer)(nativePointer, point, worldTransform, flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="CompareWithGeometry(D2D1Geometry, Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, float flatteningTolerance)
            => CompareWithGeometry(geometry, null, flatteningTolerance);

        /// <inheritdoc cref="CompareWithGeometry(D2D1Geometry, Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, in Matrix3x2 inputGeometryTransform, float flatteningTolerance)
            => CompareWithGeometry(geometry, UnsafeHelper.AsPointerIn(in inputGeometryTransform), flatteningTolerance);

        /// <summary>
        /// Compare how one geometry intersects or contains another geometry.
        /// </summary>
        [LocalsInit(false)]
        public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, Matrix3x2* inputGeometryTransform, float flatteningTolerance)
        {
            D2D1GeometryRelation result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CompareWithGeometry);
            int hr = ((delegate*<void*, void*, Matrix3x2*, float, D2D1GeometryRelation*, int>)functionPointer)(nativePointer,
                geometry.NativePointer, inputGeometryTransform, flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="Simplify(D2D1GeometrySimplificationOption, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void Simplify(D2D1GeometrySimplificationOption simplificationOption, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
            => Simplify(simplificationOption, null, flatteningTolerance, geometrySink);

        /// <inheritdoc cref="Simplify(D2D1GeometrySimplificationOption, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Simplify(D2D1GeometrySimplificationOption simplificationOption, in Matrix3x2 worldTransform,
            float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
            => Simplify(simplificationOption, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance, geometrySink);

        /// <summary>
        /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
        /// removed.
        /// </summary>
        public void Simplify(D2D1GeometrySimplificationOption simplificationOption, Matrix3x2* worldTransform,
            float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Simplify);
            int hr = ((delegate*<void*, D2D1GeometrySimplificationOption, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer,
                simplificationOption, worldTransform, flatteningTolerance, geometrySink.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="Tessellate(Matrix3x2*, float, D2D1TessellationSink)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void Tessellate(float flatteningTolerance, D2D1TessellationSink tessellationSink)
            => Tessellate(null, flatteningTolerance, tessellationSink);

        /// <inheritdoc cref="Tessellate(Matrix3x2*, float, D2D1TessellationSink)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tessellate(in Matrix3x2 worldTransform, float flatteningTolerance, D2D1TessellationSink tessellationSink)
            => Tessellate(UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance, tessellationSink);

        /// <summary>
        /// Tessellates a geometry into triangles.
        /// </summary>
        public void Tessellate(Matrix3x2* worldTransform, float flatteningTolerance, D2D1TessellationSink tessellationSink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Tessellate);
            int hr = ((delegate*<void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer,
                worldTransform, flatteningTolerance, tessellationSink.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="CombineWithGeometry(D2D1Geometry, D2D1CombineMode, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, float flatteningTolerance,
            D2D1SimplifiedGeometrySink geometrySink)
            => CombineWithGeometry(inputGeometry, combineMode, null, flatteningTolerance, geometrySink);

        /// <inheritdoc cref="CombineWithGeometry(D2D1Geometry, D2D1CombineMode, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, in Matrix3x2 inputGeometryTransform,
            float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
            => CombineWithGeometry(inputGeometry, combineMode, UnsafeHelper.AsPointerIn(in inputGeometryTransform),
                flatteningTolerance, geometrySink);

        /// <summary>
        /// Performs a combine operation between the two geometries to produce a resulting
        /// geometry.
        /// </summary>
        public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, Matrix3x2* inputGeometryTransform,
            float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CombineWithGeometry);
            int hr = ((delegate*<void*, void*, D2D1CombineMode, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, inputGeometry.NativePointer,
                combineMode, inputGeometryTransform, flatteningTolerance, geometrySink.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="Outline(Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void Outline(float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
            => Outline(null, flatteningTolerance, geometrySink);

        /// <inheritdoc cref="Outline(Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Outline(in Matrix3x2 worldTransform, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
            => Outline(UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance, geometrySink);

        /// <summary>
        /// Computes the outline of the geometry. The result is written back into a
        /// simplified geometry sink.
        /// </summary>
        public void Outline(Matrix3x2* worldTransform, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Outline);
            int hr = ((delegate*<void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, worldTransform,
                flatteningTolerance, geometrySink.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="ComputeArea(Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public float ComputeArea(float flatteningTolerance)
            => ComputeArea(null, flatteningTolerance);

        /// <inheritdoc cref="ComputeArea(Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ComputeArea(in Matrix3x2 worldTransform, float flatteningTolerance)
            => ComputeArea(UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        [LocalsInit(false)]
        public float ComputeArea(Matrix3x2* worldTransform, float flatteningTolerance)
        {
            float result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputeArea);
            int hr = ((delegate*<void*, Matrix3x2*, float, float*, int>)functionPointer)(nativePointer, worldTransform,
                flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="ComputeLength(Matrix3x2*, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public float ComputeLength(float flatteningTolerance)
            => ComputeLength(null, flatteningTolerance);

        /// <inheritdoc cref="ComputeLength(Matrix3x2*, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ComputeLength(in Matrix3x2 worldTransform, float flatteningTolerance)
            => ComputeLength(UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance);

        /// <summary>
        /// Computes the length of the geometry.
        /// </summary>
        [LocalsInit(false)]
        public float ComputeLength(Matrix3x2* worldTransform, float flatteningTolerance)
        {
            float result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputeLength);
            int hr = ((delegate*<void*, Matrix3x2*, float, float*, int>)functionPointer)(nativePointer, worldTransform,
                flatteningTolerance, &result);
            if (hr >= 0)
                return result;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="ComputePointAtLength(float, Matrix3x2*, float, PointF*, PointF*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComputePointAtLength(float length, float flatteningTolerance, out PointF point, out PointF unitTangentVector)
            => ComputePointAtLength(length, null, flatteningTolerance, UnsafeHelper.AsPointerOut(out point), UnsafeHelper.AsPointerOut(out unitTangentVector));

        /// <inheritdoc cref="ComputePointAtLength(float, Matrix3x2*, float, PointF*, PointF*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComputePointAtLength(float length, in Matrix3x2 worldTransform, float flatteningTolerance, out PointF point, out PointF unitTangentVector)
            => ComputePointAtLength(length, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance,
                UnsafeHelper.AsPointerOut(out point), UnsafeHelper.AsPointerOut(out unitTangentVector));

        /// <summary>
        /// Computes the point and tangent a given distance along the path.
        /// </summary>
        [LocalsInit(false)]
        public void ComputePointAtLength(float length, Matrix3x2* worldTransform, float flatteningTolerance, PointF* point, PointF* unitTangentVector)
        {
            float result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputePointAtLength);
            int hr = ((delegate*<void*, Matrix3x2*, float, float*, int>)functionPointer)(nativePointer, worldTransform,
                flatteningTolerance, &result);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="Widen(float, D2D1StrokeStyle, Matrix3x2*, float, D2D1GeometrySink)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance, D2D1GeometrySink geometrySink)
            => Widen(strokeWidth, strokeStyle, null, flatteningTolerance, geometrySink);

        /// <inheritdoc cref="Widen(float, D2D1StrokeStyle, Matrix3x2*, float, D2D1GeometrySink)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance, D2D1GeometrySink geometrySink)
            => Widen(strokeWidth, strokeStyle, UnsafeHelper.AsPointerIn(in worldTransform), flatteningTolerance, geometrySink);

        /// <summary>
        /// Get the geometry and widen it as well as apply an optional pen style.
        /// </summary>
        public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance, D2D1GeometrySink geometrySink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Widen);
            int hr = ((delegate*<void*, float, void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, strokeWidth,
                strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, geometrySink.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}