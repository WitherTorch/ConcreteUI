namespace ConcreteUI.Graphics.Native
{
    internal static unsafe partial class KernelBase
    {
        public static partial bool CheckSupported(string methodName);

        public static partial bool WaitOnAddress(void* address, void* compareAddress, nuint addressSize, uint dwMilliseconds);

        public static partial void WakeByAddressAll(void* address);
    }
}
