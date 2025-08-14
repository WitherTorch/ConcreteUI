using System;
using System.Runtime.InteropServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Window;

namespace ConcreteUI.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly ref struct ClearTypeToken : IDisposable
    {
        private readonly D2D1DeviceContext? _context;
        private readonly D2D1TextAntialiasMode _antialiasMode;

        private ClearTypeToken(D2D1DeviceContext context, D2D1TextAntialiasMode antialiasMode)
        {
            _context = context;
            _antialiasMode = antialiasMode;
        }

        public static ClearTypeToken TryEnterClearTypeMode(IRenderer renderer, D2D1DeviceContext deviceContext, D2D1Brush backgroundBrush)
        {
            D2D1TextAntialiasMode oldAntialiasMode = deviceContext.TextAntialiasMode;
            if (oldAntialiasMode == D2D1TextAntialiasMode.ClearType || 
                !GraphicsUtils.CheckBrushIsSolid(backgroundBrush) && (renderer is not CoreWindow window || window.WindowMaterial != WindowMaterial.None))
                return default; // Empty token

            deviceContext.TextAntialiasMode = D2D1TextAntialiasMode.ClearType;
            return new ClearTypeToken(deviceContext, oldAntialiasMode);
        }

        public void Dispose()
        {
            D2D1DeviceContext? context = _context;
            if (context is null)
                return;
            context.TextAntialiasMode = _antialiasMode;
        }
    }
}
