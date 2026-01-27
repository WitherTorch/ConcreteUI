using System.Security;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static class UxTheme
    {
        private const string LibraryName = "uxtheme.dll";

        private static readonly void* SetPreferredAppModePtr;

        static UxTheme()
        {
            SetPreferredAppModePtr = MethodImportHelper.GetImportedMethodPointer(LibraryName, 135);
        }

        public static int SetPreferredAppMode(PreferredAppMode preferredAppMode)
        {
            const int E_NOTIMPL = unchecked((int)0x80004001);
            void* pointer = SetPreferredAppModePtr;
            if (pointer == null)
                return E_NOTIMPL;
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
                [Stdcall, SuppressGCTransition]
#else
                [Stdcall]
#endif
                <PreferredAppMode, int>)pointer)(preferredAppMode);
        }
    }
}