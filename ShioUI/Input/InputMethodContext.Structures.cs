using System.Drawing;
using System.Runtime.InteropServices;

using RiceTea.Core.Structures;

namespace ShioUI.Input;

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
