using System.Security;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class UxTheme
    {
        private const string UXTHEME_DLL = "uxtheme.dll";

        private static readonly void* SetPreferredAppModePtr;

        static UxTheme()
        {
            SetPreferredAppModePtr = MethodImportHelper.GetImportedMethodPointer(UXTHEME_DLL, 135);
        }

        public static int SetPreferredAppMode(PreferredAppMode preferredAppMode)
        {
            const int E_NOTIMPL = unchecked((int)0x80004001);
            void* pointer = SetPreferredAppModePtr;
            if (pointer == null)
                return E_NOTIMPL;
            return ((delegate*<PreferredAppMode, int>)pointer)(preferredAppMode);
        }
    }
}
