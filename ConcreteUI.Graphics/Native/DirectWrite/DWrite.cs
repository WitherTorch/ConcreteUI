using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class DWrite
    {
        private const string DWrite_DLL = "dwrite.dll";

#if NET8_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(DWrite_DLL)]
        public static extern int DWriteCreateFactory(DWriteFactoryType factoryType, Guid* iid, void** pFactory);
    }
}
