using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static class KernelBase
    {
        private const string LibraryName = "kernelbase.dll";

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
            _functionPointers = MethodImportHelper.GetImportedMethodPointers(LibraryName, _methodNames);
        }

        public static bool CheckSupported(string methodName)
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

        public static SysBool WaitOnAddress(void* address, void* compareAddress, nuint addressSize, uint dwMilliseconds)
        {
            void* ptr = _functionPointers[(int)MethodTable.WaitOnAddress];
            if (ptr == null)
                return false;
            return ((delegate* unmanaged[Stdcall]<void*, void*, nuint, uint, SysBool>)ptr)(address, compareAddress, addressSize, dwMilliseconds);
        }

        public static void WakeByAddressAll(void* address)
        {
            void* ptr = _functionPointers[(int)MethodTable.WakeByAddressAll];
            if (ptr == null)
                return;
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
                [Stdcall, SuppressGCTransition]
#else
                [Stdcall]
#endif
                <void*, void>)ptr)(address);
        }
    }
}