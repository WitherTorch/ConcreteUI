using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class Shell32
    {
        private const string SHELL32_DLL = "shell32.dll";

        [DllImport(SHELL32_DLL)]
        public static extern IntPtr SHAppBarMessage(uint dwMessage, AppBarData* pData);
    }
}
