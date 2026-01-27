#if NET8_0_OR_GREATER
using System.Security;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class UxTheme
    {
        private const string UXTHEME_DLL = "uxtheme.dll";

        private static readonly void* SetPreferredAppModePtr;

        static UxTheme()
        {
            SetPreferredAppModePtr = MethodImportHelper.GetImportedMethodPointer(UXTHEME_DLL, 135);
        }

        public static partial int SetPreferredAppMode(PreferredAppMode preferredAppMode)
        {
            const int E_NOTIMPL = unchecked((int)0x80004001);
            void* pointer = SetPreferredAppModePtr;
            if (pointer == null)
                return E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall, SuppressGCTransition]<PreferredAppMode, int>)pointer)(preferredAppMode);
        }
    }
}
#endif