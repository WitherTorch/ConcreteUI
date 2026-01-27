#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Utils;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Imm32
    {
        private const string IMM32_DLL = "imm32.dll";

        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmGetContext(IntPtr hWND);

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmReleaseContext(IntPtr hWND, IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmCreateContext();

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmDestroyContext(IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);

        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetOpenStatus(IntPtr hIMC, bool open);

        [DllImport(IMM32_DLL)]
        public static extern partial long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetCandidateWindow(IntPtr hIMC, CandidateForm* lpCandidate);

        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetCompositionWindow(IntPtr hIMC, CompositionForm* lpCompForm);

        [DllImport(IMM32_DLL)]
        public static extern partial uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
#endif