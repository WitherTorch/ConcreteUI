using System.Security;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class D2D1Image : D2D1Resource
    {
        protected new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            _End = _Start, //No methods
        }

        public D2D1Image() : base() { }

        public D2D1Image(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }
    }
}