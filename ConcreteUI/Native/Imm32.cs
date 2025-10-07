using System;

namespace ConcreteUI.Native
{
    internal static unsafe partial class Imm32
    {
        public static partial IntPtr ImmGetContext(IntPtr hWND);
        public static partial bool ImmReleaseContext(IntPtr hWND, IntPtr hIMC);
        public static partial IntPtr ImmCreateContext();
        public static partial bool ImmDestroyContext(IntPtr hIMC);
        public static partial IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);
        public static partial IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);
        public static partial IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);
        public static partial bool ImmGetOpenStatus(IntPtr hIMC);
        public static partial bool ImmSetOpenStatus(IntPtr hIMC, bool open);
        public static partial long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);
        public static partial bool ImmSetCandidateWindow(IntPtr hIMC, CandidateForm* lpCandidate);
        public static partial bool ImmSetCompositionWindow(IntPtr hIMC, CompositionForm* lpCompForm);
        public static partial uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
