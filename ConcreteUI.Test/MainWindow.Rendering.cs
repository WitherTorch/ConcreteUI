using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;

namespace ConcreteUI.Test;

partial class MainWindow
{
    protected override RenderResult RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
    {
        try
        {
            return base.RenderPage(context, data);
        }
        finally
        {
            if (Volatile.Read(ref _isAnimating))
                UpdateAndResize();
        }
    }
}
