using System;

using ConcreteUI.Internals.Native;

namespace ConcreteUI.Internals.Native
{
    internal static unsafe partial class Shell32
    {
        public static partial IntPtr SHAppBarMessage(uint dwMessage, AppBarData* pData);
    }
}
