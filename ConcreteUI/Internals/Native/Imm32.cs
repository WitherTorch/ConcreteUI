using System;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Input;
using ConcreteUI.Utils;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Imm32
    {
        private const string LibraryName = "imm32.dll";

        [DllImport(LibraryName)]
        public static extern IntPtr ImmGetContext(IntPtr hWND);

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmReleaseContext(IntPtr hWND, IntPtr hIMC);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr ImmCreateContext();

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmDestroyContext(IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWND, uint hIMC);

        [DllImport(LibraryName)]
        public static extern IntPtr ImmAssociateContextEx(IntPtr hWND, IntPtr hIMC, ImmAssociateContextEx_Flags flag);

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmGetOpenStatus(IntPtr hIMC);

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmSetOpenStatus(IntPtr hIMC, SysBool32 open);

        [DllImport(LibraryName)]
        public static extern long ImmGetCompositionStringW(IntPtr hIMC, IMECompositionFlags flags, void* lpBuf, int dwBufLen);

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmSetCandidateWindow(IntPtr hIMC, IMECandidateForm* lpCandidate);

        [DllImport(LibraryName)]
        public static extern SysBool32 ImmSetCompositionWindow(IntPtr hIMC, IMECompositionForm* lpCompForm);

        [DllImport(LibraryName)]
        public static extern uint ImmGetVirtualKey(IntPtr hIMC);
    }
}
