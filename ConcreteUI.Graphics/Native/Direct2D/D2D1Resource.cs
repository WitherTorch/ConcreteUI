using System.Security;

using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// The root interface for all resources in D2D.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public abstract unsafe class D2D1Resource : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetFactory = _Start,
            _End
        }

        protected D2D1Resource() : base() { }

        protected D2D1Resource(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Retrieve the factory associated with this resource.
        /// </summary>
        public D2D1Factory? GetFactory()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFactory);
            ((delegate* unmanaged[Stdcall]<void*, void**, void>)functionPointer)(nativePointer, &nativePointer);
            return nativePointer == null ? null : new D2D1Factory(nativePointer, ReferenceType.Owned);
        }
    }
}
