using System;
using System.Runtime.InteropServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Window;

namespace ConcreteUI.Utils
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ClearTypeScope : IDisposable
    {
        private readonly D2D1DeviceContext? _context;
        private readonly D2D1TextAntialiasMode _antialiasMode;

        private ClearTypeScope(D2D1DeviceContext context, D2D1TextAntialiasMode antialiasMode)
        {
            _context = context;
            _antialiasMode = antialiasMode;
        }

        public static ClearTypeScope Enter(IRenderer renderer, D2D1DeviceContext deviceContext, D2D1Brush backgroundBrush)
        {
            D2D1TextAntialiasMode oldAntialiasMode = deviceContext.TextAntialiasMode;
            if (oldAntialiasMode == D2D1TextAntialiasMode.ClearType || 
                !GraphicsUtils.CheckBrushIsSolid(backgroundBrush) && (renderer is not CoreWindow window || window.WindowMaterial != WindowMaterial.None))
                return default; // Empty token

            deviceContext.TextAntialiasMode = D2D1TextAntialiasMode.ClearType;
            return new ClearTypeScope(deviceContext, oldAntialiasMode);
        }

        public static ClearTypeScope Enter(IRenderer renderer, scoped in RegionalRenderingContext renderingContext, D2D1Brush backgroundBrush)
            => Enter(renderer, renderingContext.DeviceContext, backgroundBrush);

        public void Dispose()
        {
            D2D1DeviceContext? context = _context;
            if (context is null)
                return;
            context.TextAntialiasMode = _antialiasMode;
        }
    }
}
