using System.Runtime.InteropServices;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Represents a set of vertices that form a list of triangles.
    /// </summary>
    public sealed unsafe class D2D1Mesh : D2D1Resource
    {
        private new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            Open = _Start,
            _End
        }

        public D2D1Mesh() : base() { }

        public D2D1Mesh(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Opens the mesh for population.
        /// </summary>
        public D2D1TessellationSink Open()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Open);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1TessellationSink(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
