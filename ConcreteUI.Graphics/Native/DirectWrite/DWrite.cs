using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class DWrite
    {
        private const string LibraryName = "dwrite.dll";

#if NET8_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibraryName)]
        public static extern int DWriteCreateFactory(DWriteFactoryType factoryType, Guid* iid, void** pFactory);
    }
}
