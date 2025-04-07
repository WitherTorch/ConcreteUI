using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Encapsulates a device- and transform-dependent representation of a filled or
    /// stroked geometry.
    /// </summary>
    public sealed unsafe class D2D1GeometryRealization : D2D1Resource
    {
        //No Method Table

        public D2D1GeometryRealization() : base() { }

        public D2D1GeometryRealization(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }
    }
}
