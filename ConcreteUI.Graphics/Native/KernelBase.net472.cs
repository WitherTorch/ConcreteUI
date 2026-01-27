#if NET472_OR_GREATER
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics.Native
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class KernelBase
    {
        private const string KERNELBASE_DLL = "kernelbase.dll";

        private enum MethodTable
        {
            WaitOnAddress,
            WakeByAddressAll,
            _Last
        }

        private static readonly string[] _methodNames = new string[(int)MethodTable._Last] { nameof(WaitOnAddress), nameof(WakeByAddressAll) };
        private static readonly void*[] _functionPointers;

        static KernelBase()
        {
            _functionPointers = MethodImportHelper.GetImportedMethodPointers(KERNELBASE_DLL, _methodNames);
        }

        public static partial bool CheckSupported(string methodName)
        {
            string[] methodNames = _methodNames;
            ref string methodNameArrayRef = ref methodNames[0];
            for (int i = 0, length = methodNames.Length; i < length; i++)
            {
                if (SequenceHelper.Equals(methodName, UnsafeHelper.AddTypedOffset(ref methodNameArrayRef, i)))
                    return _functionPointers[i] != null;
            }
            return false;
        }

        public static partial bool WaitOnAddress(void* address, void* compareAddress, nuint addressSize, uint dwMilliseconds)
        {
            void* ptr = _functionPointers[(int)MethodTable.WaitOnAddress];
            if (ptr == null)
                return false;
            return ((delegate* unmanaged[Stdcall]<void*, void*, nuint, uint, bool>)ptr)(address, compareAddress, addressSize, dwMilliseconds);
        }

        public static partial void WakeByAddressAll(void* address)
        {
            void* ptr = _functionPointers[(int)MethodTable.WakeByAddressAll];
            if (ptr == null)
                return;
            ((delegate* unmanaged[Stdcall]<void*, void>)ptr)(address);
        }
    }
}
#endif