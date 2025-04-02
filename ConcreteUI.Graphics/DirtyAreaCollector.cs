using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Extensions;
using ConcreteUI.Graphics.Hosting;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.CrossNative;
using WitherTorch.CrossNative.Helpers;
using WitherTorch.CrossNative.Windows.Structures;

namespace ConcreteUI.Graphics
{
    public sealed class DirtyAreaCollector
    {
        private static readonly DirtyAreaCollector _empty = new DirtyAreaCollector(null, null);

        public static DirtyAreaCollector Empty => _empty;

        private readonly SwapChainGraphicsHost1 _host;
        private readonly List<RectF> _list;

        private bool _presentAllMode;

        private DirtyAreaCollector(List<RectF> list, SwapChainGraphicsHost1 host)
        {
            _list = list;
            _host = host;
        }

        public static DirtyAreaCollector TryCreate(SwapChainGraphicsHost1 host)
        {
            if (host is null || host.IsDisposed)
                return null;
            return new DirtyAreaCollector(new List<RectF>(), host);
        }

        public bool IsEmptyInstance => _list is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyDirtyArea()
        {
            if (_presentAllMode)
                return true;
            List<RectF> list = _list;
            if (list is null)
                return false;
            return list.Count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkAsDirty(in RectF rect) => _list?.Add(rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UsePresentAllModeOnce() => _presentAllMode = true;

        public unsafe void Present(double dpiScaleFactor)
        {
            SwapChainGraphicsHost1 host = _host;
            if (host is null)
                return;
            List<RectF> list = _list;
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
            if (!TryGetPresentingRects(dpiScaleFactor, out IArrayPool<Rect> pool, out Rect[] rects, out int count))
                return;
            try
            {
                fixed (Rect* ptr = rects)
                    _host.Present(new DXGIPresentParameters(unchecked((uint)count), ptr));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                pool.Return(rects);
            }
        }

        public unsafe bool TryPresent(double dpiScaleFactor)
        {
            SwapChainGraphicsHost1 host = _host;
            if (host is null)
                return false;
            List<RectF> list = _list;
            if (list is null)
                return host.TryPresent();
            if (_presentAllMode)
            {
                _presentAllMode = false;
                list.Clear();
                return host.TryPresent();
            }
            if (!TryGetPresentingRects(dpiScaleFactor, out IArrayPool<Rect> pool, out Rect[] rects, out int count))
                return false;
            bool result;
            fixed (Rect* ptr = rects)
                result = _host.TryPresent(new DXGIPresentParameters(unchecked((uint)count), ptr));
            pool.Return(rects);
            return result;
        }

        private bool TryGetPresentingRects(double dpiScaleFactor, out IArrayPool<Rect> pool, out Rect[] rects, out int count)
        {
            UnsafeHelper.SkipInit(out pool);
            UnsafeHelper.SkipInit(out rects);

            List<RectF> list = _list;
            count = list.Count;
            if (count <= 0)
                return false;
            pool = WTCrossNative.ArrayPoolProvider.GetArrayPool<Rect>();
            rects = pool.Rent(count);
            ScaleRects(rects, list, count, dpiScaleFactor);
            list.Clear();
            return true;
        }

        [Inline(InlineBehavior.Remove)]
        private static void ScaleRects(Rect[] destination, List<RectF> source, int count, double dpiScaleFactor)
        {
            for (int i = 0; i < count; i++)
            {
                RectF sourceRect = source[i];
                destination[i] = new Rect(
                    left: (sourceRect.Left * dpiScaleFactor).FloorToInt(), top: (sourceRect.Top * dpiScaleFactor).FloorToInt(),
                    right: (sourceRect.Right * dpiScaleFactor).CeilingToInt(), bottom: (sourceRect.Bottom * dpiScaleFactor).CeilingToInt());
            }
        }
    }
}
