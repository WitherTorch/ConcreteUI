using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Graphics
{
    public readonly ref struct RenderingLayerToken : IDisposable
    {
        private readonly D2D1Layer? _layer;
        private readonly D2D1DeviceContext _context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe RenderingLayerToken(D2D1DeviceContext context, scoped in D2D1LayerParameters parameters)
            : this(context, parameters, !SystemHelper.IsWindows8OrHigher) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe RenderingLayerToken(D2D1DeviceContext context, scoped in D2D1LayerParametersNative parameters)
            : this(context, parameters, !SystemHelper.IsWindows8OrHigher) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe RenderingLayerToken(D2D1DeviceContext context, scoped in D2D1LayerParameters parameters, bool createLayerObject)
        {
            _context = context;
            if (createLayerObject)
            {
                D2D1Layer layer = context.CreateLayer(parameters.ContentBounds.Size);
                context.PushLayer(parameters, layer);
                _layer = layer;
            }
            else
                context.PushLayer(parameters, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe RenderingLayerToken(D2D1DeviceContext context, scoped in D2D1LayerParametersNative parameters, bool createLayerObject)
        {
            _context = context;
            if (createLayerObject)
            {
                D2D1Layer layer = context.CreateLayer(parameters.ContentBounds.Size);
                context.PushLayer(parameters, layer);
                _layer = layer;
            }
            else
                context.PushLayer(parameters, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _context?.PopLayer();
            _layer?.Dispose();
        }

    }
}
