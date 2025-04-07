using System.Drawing;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CandidateForm
    {
        public int dwIndex;
        public int dwStyle;
        public Point ptCurrentPos;
        public Rect rcArea;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CompositionForm
    {
        public int dwStyle;
        public Point ptCurrentPos;
        public Rect rcArea;
    }
}
