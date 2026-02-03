using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Internals;
using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Graphics
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct RenderingLayerScope : IDisposable
    {
        private readonly D2D1DeviceContext _context;
        private readonly D2D1Layer? _layer;

        private RenderingLayerScope(D2D1DeviceContext context, D2D1Layer? layer)
        {
            _context = context;
            _layer = layer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe RenderingLayerScope Enter(D2D1DeviceContext context, scoped in D2D1LayerParameters parameters)
            => Enter(context, parameters, !SystemHelper.IsWindows8OrHigher);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe RenderingLayerScope Enter(D2D1DeviceContext context, scoped in D2D1LayerParametersNative parameters)
            => Enter(context, parameters, !SystemHelper.IsWindows8OrHigher);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe RenderingLayerScope Enter(D2D1DeviceContext context, scoped in D2D1LayerParameters parameters, bool createLayerObject)
        {
            if (createLayerObject)
            {
                D2D1Layer layer = context.CreateLayer(parameters.ContentBounds.Size);
                context.PushLayer(parameters, layer);
                return new RenderingLayerScope(context, layer);
            }
            else
            {
                context.PushLayer(parameters, null);
                return new RenderingLayerScope(context, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe RenderingLayerScope Enter(D2D1DeviceContext context, scoped in D2D1LayerParametersNative parameters, bool createLayerObject)
        {
            if (createLayerObject)
            {
                D2D1Layer layer = context.CreateLayer(parameters.ContentBounds.Size);
                context.PushLayer(parameters, layer);
                return new RenderingLayerScope(context, layer);
            }
            else
            {
                context.PushLayer(parameters, null);
                return new RenderingLayerScope(context, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _context?.PopLayer();
            _layer?.Dispose();
        }

    }
}
