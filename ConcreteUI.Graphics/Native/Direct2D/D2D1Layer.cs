using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Represents the backing store required to render a layer.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class D2D1Layer : D2D1Resource
    {
        private new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            GetSize = _Start,
            _End,
        }

        public D2D1Layer() : base() { }

        public D2D1Layer(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the size of the layer in DIPs.
        /// </summary>
        public SizeF Size
        {
            [LocalsInit(false)]
            get => GetSize();
        }

        [Inline(InlineBehavior.Remove)]
        private SizeF GetSize()
        {
            SizeF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSize);
            ((delegate* unmanaged[Stdcall]<void*, SizeF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }
    }
}
