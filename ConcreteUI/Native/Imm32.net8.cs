#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Imm32
    {
        private const string IMM32_DLL = "imm32.dll";

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmGetContext(IntPtr hWND);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmReleaseContext(IntPtr hWND, IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmCreateContext();

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmDestroyContext(IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmGetOpenStatus(IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetOpenStatus(IntPtr hIMC, bool open);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetCandidateWindow(IntPtr hIMC, CandidateForm* lpCandidate);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial bool ImmSetCompositionWindow(IntPtr hIMC, CompositionForm* lpCompForm);

        [SuppressGCTransition]
        [DllImport(IMM32_DLL)]
        public static extern partial uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
#endif