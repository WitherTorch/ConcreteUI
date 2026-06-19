using System.Threading;

using ConcreteUI.Graphics;

namespace ConcreteUI.Test;

partial class MainWindow
{
    protected override void RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
    {
        base.RenderPage(context, data);
        if (Volatile.Read(ref _isAnimating))
            UpdateAndResize();
    }
}
