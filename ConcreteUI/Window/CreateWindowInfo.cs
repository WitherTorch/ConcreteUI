using System.Runtime.InteropServices;

namespace ConcreteUI.Window
{
    [StructLayout(LayoutKind.Auto)]
    public struct CreateWindowInfo
    {
        public WindowStyles Styles;
        public WindowExtendedStyles ExtendedStyles;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public CreateWindowInfo(WindowStyles styles, WindowExtendedStyles extendedStyles, int x, int y, int width, int height)
        {
            Styles = styles;
            ExtendedStyles = extendedStyles;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
