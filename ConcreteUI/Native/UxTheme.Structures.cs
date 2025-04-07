using System.Runtime.InteropServices;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public Margins(int value)
        {
            Left = value;
            Right = value;
            Top = value;
            Bottom = value;
        }
    }
}
