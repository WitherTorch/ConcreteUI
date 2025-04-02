using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


using WitherTorch.CrossNative;
using WitherTorch.CrossNative.Windows;
using WitherTorch.CrossNative.Helpers;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Populates a <see cref="D2D1Mesh"/> object with triangles.
    /// </summary>
    public sealed unsafe class D2D1TessellationSink : ComObject
    {
        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            AddTriangles = _Start,
            Close,
            _End
        }

        public D2D1TessellationSink() : base() { }

        public D2D1TessellationSink(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTriangles(in D2D1Triangle triangle)
            => AddTriangles(UnsafeHelper.AsPointerIn(in triangle), 1u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTriangles(params D2D1Triangle[] triangles)
        {
            fixed (D2D1Triangle* ptr = triangles)
                AddTriangles(ptr, unchecked((uint)triangles.Length));
        }

        public void AddTriangles(D2D1Triangle* triangles, uint trianglesCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddTriangles);
            ((delegate*<void*, D2D1Triangle*, uint, void>)functionPointer)(nativePointer, triangles, trianglesCount);
        }

        public void Close()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Close);
            int hr = ((delegate*<void*, int>)functionPointer)(nativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
