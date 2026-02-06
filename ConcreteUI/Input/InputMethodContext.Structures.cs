using System.Drawing;
using System.Runtime.InteropServices;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IMECandidateForm
    {
        public int dwIndex;
        public IMECandicateStyle dwStyle;
        public Point ptCurrentPos;
        public Rect rcArea;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMECompositionForm
    {
        public IMECompositionStyle dwStyle;
        public Point ptCurrentPos;
        public Rect rcArea;
    }
}
