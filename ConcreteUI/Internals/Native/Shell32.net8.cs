#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Shell32
    {
        private const string SHELL32_DLL = "shell32.dll";

        [DllImport(SHELL32_DLL)]
        public static extern partial IntPtr SHAppBarMessage(uint dwMessage, AppBarData* pData);
    }
}
#endif