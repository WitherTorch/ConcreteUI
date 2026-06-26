using System.Threading;

using ShioUI.Utils;

using ShioUI.Graphics;

namespace ShioUI.Test;

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
