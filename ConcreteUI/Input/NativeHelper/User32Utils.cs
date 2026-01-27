using ConcreteUI.Internals.Native;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Input.NativeHelper
{
    internal static class User32Utils
    {
        public static ushort GetCurrentInputLanguage()
        {
            return User32.GetKeyboardLayout(0).GetWords().LowWord;
        }
    }
}
