using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Graphics
{
    public interface IBrushStyler
    {
        void SetBrush(string subKey, D2D1Brush brush);

        void Clean();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        string GetPrefix();
    }
}
