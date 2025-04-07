using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class D2D1ColorContext : D2D1Resource
    {
        private new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            GetColorSpace,
            GetProfileSize,
            GetProfile,
            _End,
        }

        public D2D1ColorContext() : base() { }

        public D2D1ColorContext(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Retrieves the color space of the color context.
        /// </summary>
        public D2D1ColorSpace ColorSpace => GetColorSpace();

        [Inline(InlineBehavior.Remove)]
        private D2D1ColorSpace GetColorSpace()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetColorSpace);
            return ((delegate*<void*, D2D1ColorSpace>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Retrieves the size of the color profile, in bytes.
        /// </summary>
        public uint GetProfileSize()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetProfileSize);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="GetProfile(byte*, uint)"/>
        public byte[] GetProfile()
        {
            uint size = GetProfileSize();
            byte[] result = new byte[size];
            fixed (byte* ptr = result)
                GetProfile(ptr, size);
            return result;
        }

        /// <summary>
        /// Retrieves the color profile bytes.
        /// </summary>
        public void GetProfile(byte* profile, uint profileSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetProfile);
            int hr = ((delegate*<void*, byte*, uint, int>)functionPointer)(nativePointer, profile, profileSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
