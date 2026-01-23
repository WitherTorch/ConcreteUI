using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Hosting;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    public sealed partial class DirtyAreaCollector
    {
        private static readonly DirtyAreaCollector _empty = new DirtyAreaCollector(null, null);
        public static DirtyAreaCollector Empty => _empty;

        private readonly SwapChainGraphicsHost1? _host;
        private readonly UnwrappableList<RectF>? _list;

        private bool _presentAllMode;

        private DirtyAreaCollector(UnwrappableList<RectF>? list, SwapChainGraphicsHost1? host)
        {
            _list = list;
            _host = host;
        }

        public static DirtyAreaCollector? TryCreate(SwapChainGraphicsHost1? host)
        {
            if (host is null || host.IsDisposed)
                return null;
            return new DirtyAreaCollector(new UnwrappableList<RectF>(), host);
        }

        public bool IsEmptyInstance => _list is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyDirtyArea()
        {
            if (_presentAllMode)
                return true;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
                return false;
            return list.Count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void MarkAsDirty(in RectF rect) => _list?.Add(rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UsePresentAllModeOnce() => _presentAllMode = true;

        public unsafe void Present(Vector2 pointsPerPixel)
        {
            SwapChainGraphicsHost1? host = _host;
            if (host is null)
                return;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
            {
                host.Present();
                return;
            }
            if (_presentAllMode)
            {
                _presentAllMode = false;
                list.Clear();
                host.Present();
                return;
            }
            if (!TryGetPresentingRects(pointsPerPixel, out ArrayPool<Rect>? pool, out Rect[]? rects, out uint count))
                return;
            try
            {
                fixed (Rect* ptr = rects)
                    host.Present(new DXGIPresentParameters(count, ptr));
            }
            finally
            {
                pool.Return(rects);
            }
        }

        public unsafe bool TryPresent(Vector2 pointsPerPixel)
        {
            SwapChainGraphicsHost1? host = _host;
            if (host is null)
                return false;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
                return host.TryPresent();
            if (_presentAllMode)
            {
                _presentAllMode = false;
                list.Clear();
                return host.TryPresent();
            }
            if (!TryGetPresentingRects(pointsPerPixel, out ArrayPool<Rect>? pool, out Rect[]? rects, out uint count))
                return false;
            try
            {
                fixed (Rect* ptr = rects)
                    return host.TryPresent(new DXGIPresentParameters(count, ptr));
            }
            finally
            {
                pool.Return(rects);
            }
        }

        private bool TryGetPresentingRects(Vector2 pointsPerPixel,
            [NotNullWhen(true)] out ArrayPool<Rect>? pool, [NotNullWhen(true)] out Rect[]? rects, out uint count)
        {
            UnwrappableList<RectF> list = _list!;
            int countRaw = list.Count;
            if (countRaw <= 0)
            {
                pool = null;
                rects = null;
                count = 0;
                return false;
            }
            count = (uint)countRaw;
            RectF[] sourceRects = list.Unwrap();
            pool = ArrayPool<Rect>.Shared;
            rects = pool.Rent(count);
            count = ScaleRects(rects, sourceRects, count, pointsPerPixel);
            list.Clear();
            return true;
        }

        private static unsafe uint ScaleRects(Rect[] destination, RectF[] source, uint count, Vector2 pointsPerPixel)
        {
            uint j = 0;
            ref Rect destinationArrayRef = ref destination[0];

            fixed (RectF* ptr = source)
            {
                ScaleRects(ptr, count, pointsPerPixel);
                for (uint i = 0; i < count; i++)
                {
                    Rect area = *(Rect*)(ptr + i);
                    if (!area.IsValid)
                        continue;
                    UnsafeHelper.AddTypedOffset(ref destinationArrayRef, j++) = area;
                }
            }

            return j;
        }

        private static unsafe partial void ScaleRects(RectF* source, uint count, Vector2 pointsPerPixel);
    }
}
