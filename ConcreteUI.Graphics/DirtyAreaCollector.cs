using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Hosting;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics
{
    public sealed class DirtyAreaCollector
    {
        private static readonly DirtyAreaCollector _empty = new DirtyAreaCollector(null, null, null);

        public static DirtyAreaCollector Empty => _empty;

        private readonly SwapChainGraphicsHost1 _host;
        private readonly UnwrappableList<Rect> _list;
        private readonly UnwrappableList<bool> _typeList;

        private bool _presentAllMode;

        private DirtyAreaCollector(UnwrappableList<Rect> list, UnwrappableList<bool> typeList, SwapChainGraphicsHost1 host)
        {
            _list = list;
            _typeList = typeList;
            _host = host;
        }

        public static DirtyAreaCollector TryCreate(SwapChainGraphicsHost1 host)
        {
            if (host is null || host.IsDisposed)
                return null;
            return new DirtyAreaCollector(new UnwrappableList<Rect>(), new UnwrappableList<bool>(), host);
        }

        public bool IsEmptyInstance => _list is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyDirtyArea()
        {
            if (_presentAllMode)
                return true;
            UnwrappableList<Rect> list = _list;
            if (list is null)
                return false;
            return list.Count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkAsDirty(in Rect rect)
        {
            UnwrappableList<Rect> list = _list;
            if (list is null) 
                return;
            list.Add(rect);
            _typeList.Add(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void MarkAsDirty(in RectF rect)
        {
            UnwrappableList<Rect> list = _list;
            if (list is null)
                return;
            list.Add(*(Rect*)UnsafeHelper.AsPointerIn(in rect));
            _typeList.Add(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UsePresentAllModeOnce() => _presentAllMode = true;

        public unsafe void Present(double dpiScaleFactor)
        {
            SwapChainGraphicsHost1 host = _host;
            if (host is null)
                return;
            UnwrappableList<Rect> list = _list;
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
            if (!TryGetPresentingRects(dpiScaleFactor, out ArrayPool<Rect> pool, out Rect[] rects, out int count))
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
                pool?.Return(rects);
            }
        }

        public unsafe bool TryPresent(double dpiScaleFactor)
        {
            SwapChainGraphicsHost1 host = _host;
            if (host is null)
                return false;
            UnwrappableList<Rect> list = _list;
            if (list is null)
                return host.TryPresent();
            if (_presentAllMode)
            {
                _presentAllMode = false;
                list.Clear();
                return host.TryPresent();
            }
            if (!TryGetPresentingRects(dpiScaleFactor, out ArrayPool<Rect> pool, out Rect[] rects, out int count))
                return false;
            bool result;
            fixed (Rect* ptr = rects)
                result = _host.TryPresent(new DXGIPresentParameters(unchecked((uint)count), ptr));
            pool?.Return(rects);
            return result;
        }

        private bool TryGetPresentingRects(double dpiScaleFactor, out ArrayPool<Rect> pool, out Rect[] rects, out int count)
        {
            UnwrappableList<Rect> list = _list;
            count = list.Count;
            if (count <= 0)
            {
                pool = null;
                rects = null;
                return false;
            }
            UnwrappableList<bool> typeList = _typeList;
            list.Clear();
            typeList.Clear();
            Rect[] sourceRects = list.Unwrap();
            bool[] sourceRectTypes = typeList.Unwrap();
            if (dpiScaleFactor == 1.0f && !SequenceHelper.ContainsExclude(sourceRectTypes, false))
            {
                pool = null;
                rects = sourceRects;
                return true;
            }
            pool = ArrayPool<Rect>.Shared;
            rects = pool.Rent(count);
            ScaleRects(rects, sourceRects, sourceRectTypes, count, dpiScaleFactor);
            return true;
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void ScaleRects(Rect[] destination, Rect[] source, bool[] typeOfSource, int count, double dpiScaleFactor)
        {
            for (int i = 0; i < count; i++)
            {
                if (typeOfSource[i])
                {
                    RectF sourceRect = UnsafeHelper.As<Rect, RectF>(source[i]);
                    destination[i] = new Rect(
                        left: MathI.Floor(sourceRect.Left * dpiScaleFactor), top: MathI.Floor(sourceRect.Top * dpiScaleFactor),
                        right: MathI.Ceiling(sourceRect.Right * dpiScaleFactor), bottom: MathI.Ceiling(sourceRect.Bottom * dpiScaleFactor));
                    continue;
                }
                if (dpiScaleFactor == 1.0f)
                    destination[i] = source[i];
                else
                {
                    Rect sourceRect = source[i];
                    destination[i] = new Rect(
                        left: MathI.Floor(sourceRect.Left * dpiScaleFactor), top: MathI.Floor(sourceRect.Top * dpiScaleFactor),
                        right: MathI.Ceiling(sourceRect.Right * dpiScaleFactor), bottom: MathI.Ceiling(sourceRect.Bottom * dpiScaleFactor));
                }
            }
        }
    }
}
