using ConcreteUI.Internals.Native;

namespace ConcreteUI.Internals
{
    internal static class CustomWindowMessages
    {
        public static readonly uint ConcreteWindowInvoke 
            = User32.RegisterWindowMessage(nameof(ConcreteWindowInvoke));

        public static readonly uint ConcreteDestroyWindowAsync 
            = User32.RegisterWindowMessage(nameof(ConcreteDestroyWindowAsync));
    }
}
