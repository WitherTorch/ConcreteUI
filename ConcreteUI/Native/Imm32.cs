using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class Imm32
    {
        private const string IMM32_DLL = "imm32.dll";

        [DllImport(IMM32_DLL)]
        public static extern IntPtr ImmGetContext(IntPtr hWND);

        [DllImport(IMM32_DLL)]
        public static extern bool ImmReleaseContext(IntPtr hWND, IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern IntPtr ImmCreateContext();

        [DllImport(IMM32_DLL)]
        public static extern bool ImmDestroyContext(IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);

        [DllImport(IMM32_DLL)]
        public static extern IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);

        [DllImport(IMM32_DLL)]
        public static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport(IMM32_DLL)]
        public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool open);

        [DllImport(IMM32_DLL)]
        public static extern long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);

        [DllImport(IMM32_DLL)]
        public static extern bool ImmSetCandidateWindow(IntPtr hIMC, CandidateForm* lpCandidate);

        [DllImport(IMM32_DLL)]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, CompositionForm* lpCompForm);

        [DllImport(IMM32_DLL)]
        public static extern uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
