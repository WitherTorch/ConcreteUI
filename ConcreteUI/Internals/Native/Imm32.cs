using System;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Utils;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Imm32
    {
        private const string LibraryName = "imm32.dll";

        [DllImport(LibraryName)]
        public static extern IntPtr ImmGetContext(IntPtr hWND);

        [DllImport(LibraryName)]
        public static extern bool ImmReleaseContext(IntPtr hWND, IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr ImmCreateContext();

        [DllImport(LibraryName)]
        public static extern bool ImmDestroyContext(IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);

        [DllImport(LibraryName)]
        public static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool open);

        [DllImport(LibraryName)]
        public static extern long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);

        [DllImport(LibraryName)]
        public static extern bool ImmSetCandidateWindow(IntPtr hIMC, CandidateForm* lpCandidate);

        [DllImport(LibraryName)]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, CompositionForm* lpCompForm);

        [DllImport(LibraryName)]
        public static extern uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
