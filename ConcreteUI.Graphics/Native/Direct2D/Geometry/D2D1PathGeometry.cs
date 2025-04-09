using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Represents a complex shape that may be composed of arcs, curves, and lines.
    /// </summary>
    public unsafe sealed class D2D1PathGeometry : D2D1Geometry
    {
        private new enum MethodTable
        {
            _Start = D2D1Geometry.MethodTable._End,
            Open = _Start,
            Stream,
            GetSegmentCount,
            GetFigureCount,
            _End
        }

        public D2D1PathGeometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public uint SegmentCount
        {
            [LocalsInit(false)]
            get => GetSegmentCount();
        }

        public uint FigureCount
        {
            [LocalsInit(false)]
            get => GetFigureCount();
        }

        /// <summary>
        /// Opens a geometry sink that will be used to create this path geometry.
        /// </summary>
        [LocalsInit(false)]
        public D2D1GeometrySink Open()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Open);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1GeometrySink(nativePointer, ReferenceType.Owned);
        }

        /// <summary>
        /// Retrieve the contents of this geometry. The caller passes a <see cref="D2D1GeometrySink"/> object to receive the data.
        /// </summary>
        public void Stream(D2D1GeometrySink sink)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Stream);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, int>)functionPointer)(nativePointer, sink.NativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private uint GetSegmentCount()
        {
            uint result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSegmentCount);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private uint GetFigureCount()
        {
            uint result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFigureCount);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }
    }
}