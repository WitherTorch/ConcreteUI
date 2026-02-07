using System.Runtime.InteropServices;

namespace ConcreteUI.Graphics.Internals.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TimeCapability
    {
        public uint wPeriodMin;
        public uint wPeriodMax;
    }
}
