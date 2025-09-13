using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Native.Direct2D;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct RenderingClipToken : IDisposable
    {
        private readonly D2D1DeviceContext? _context;
        private readonly RectF _clipRect;

        public RectF ClipRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _clipRect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe RenderingClipToken(D2D1DeviceContext context, scoped in RectF clipRect, D2D1AntialiasMode antialiasMode)
        {
            _context = context;
            _clipRect = clipRect;

            context.PushAxisAlignedClip(in clipRect, antialiasMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _context?.PopAxisAlignedClip();
    }
}
