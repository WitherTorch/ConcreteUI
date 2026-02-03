using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Native.Direct2D;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct RenderingClipScope : IDisposable
    {
        private readonly D2D1DeviceContext? _context;
        private readonly RectF _clipRect;

        public RectF ClipRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _clipRect;
        }

        private RenderingClipScope(D2D1DeviceContext context, scoped in RectF clipRect)
        {
            _context = context;
            _clipRect = clipRect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderingClipScope Enter(D2D1DeviceContext context, scoped in RectF clipRect)
            => Enter(context, clipRect, D2D1AntialiasMode.Aliased);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderingClipScope Enter(D2D1DeviceContext context, scoped in RectF clipRect, D2D1AntialiasMode antialiasMode)
        {
            context.PushAxisAlignedClip(in clipRect, antialiasMode);
            return new RenderingClipScope(context, clipRect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _context?.PopAxisAlignedClip();
    }
}
