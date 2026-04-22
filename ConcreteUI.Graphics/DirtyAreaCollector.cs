using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Hosts;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics
{
    public sealed partial class DirtyAreaCollector
    {
        public static readonly DirtyAreaCollector Empty = new DirtyAreaCollector(null, null);

        private readonly SimpleGraphicsHost? _host;
        private readonly UnwrappableList<RectF>? _list;

        private bool _presentAllMode;

        public DirtyAreaCollector(SimpleGraphicsHost host) :
            this(host is OptimizedGraphicsHost ? new UnwrappableList<RectF>() : null, host)
        { }

        private DirtyAreaCollector(UnwrappableList<RectF>? list, SimpleGraphicsHost? host)
        {
            _list = list;
            _host = host;
        }

        public bool IsEmptyInstance => _host is null && _list is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyDirtyArea()
        {
            if (_presentAllMode)
                return true;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
                return true;
            return list.Count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkAsDirty(in RectF rect) => _list?.Add(rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UsePresentAllModeOnce() => _presentAllMode = true;

        public unsafe void Present(Vector2 pointsPerPixel)
        {
            SimpleGraphicsHost? host = _host;
            if (host is null)
                return;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
            {
                host.Present();
                return;
            }
            if (host is not OptimizedGraphicsHost host1 || ReferenceHelper.Exchange(ref _presentAllMode, false))
            {
                list.Clear();
                host.Present();
                return;
            }
            RectF[] array = list.Unwrap();
            int count = list.Count;
            if (count <= 0)
                return;
            fixed (RectF* ptr = array)
            {
                uint length = unchecked((uint)count);
                ScaleRects(ptr, length, pointsPerPixel);
                CleanInvalidRect((Rect*)ptr, length);
                try
                {
                    host1.Present(new DXGIPresentParameters(length, (Rect*)ptr));
                }
                finally
                {
                    list.Clear();
                }
            }
        }

        public unsafe bool TryPresent(Vector2 pointsPerPixel)
        {
            SimpleGraphicsHost? host = _host;
            if (host is null)
                return false;
            UnwrappableList<RectF>? list = _list;
            if (list is null)
                return host.TryPresent();
            if (host is not OptimizedGraphicsHost host1 || ReferenceHelper.Exchange(ref _presentAllMode, false))
            {
                list.Clear();
                return host.TryPresent();
            }
            RectF[] array = list.Unwrap();
            int count = list.Count;
            if (count <= 0)
                return true;
            bool result;
            fixed (RectF* ptr = array)
            {
                uint length = unchecked((uint)count);
                ScaleRects(ptr, length, pointsPerPixel);
                CleanInvalidRect((Rect*)ptr, length);
                result = host1.TryPresent(new DXGIPresentParameters(length, (Rect*)ptr));
            }
            list.Clear();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CleanInvalidRect(Rect* ptr, nuint length)
        {
            for (; length >= 4; length -= 4, ptr += 4)
            {
                if (!ptr[0].IsValid)
                    ptr[0] = default;
                if (!ptr[1].IsValid)
                    ptr[1] = default;
                if (!ptr[2].IsValid)
                    ptr[2] = default;
                if (!ptr[3].IsValid)
                    ptr[3] = default;
            }
            Rect* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return;
            if (!ptr->IsValid)
                *ptr = default;
            ptr++;
            if (ptr >= ptrEnd)
                return;
            if (!ptr->IsValid)
                *ptr = default;
            ptr++;
            if (ptr >= ptrEnd)
                return;
            if (!ptr->IsValid)
                *ptr = default;
        }

        private static unsafe void ScaleRects(RectF* ptr, nuint length, Vector2 pointsPerPixel)
        {
            DebugHelper.ThrowIf(sizeof(Rect) != sizeof(RectF));

            if (Limits.CheckTypeCanBeVectorized<float>() && Limits.CheckTypeCanBeVectorized<int>())
            {
                nuint limit = Limits.GetLimitForVectorizing<float>();
                if (limit >= UnsafeHelper.SizeOf<RectF>() - 1)
                {
                    VectorizedScaleRects(ptr, length, pointsPerPixel);
                    return;
                }
            }
            ScalarizedScaleRects(ref ptr, ref length, pointsPerPixel);
        }

        [Inline(InlineBehavior.Remove)]
        private static unsafe void VectorizedScaleRects(RectF* ptr, nuint length, Vector2 pointsPerPixel)
            => VectorizedScaleRects((float*)ptr, length * 4, pointsPerPixel);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 pointsPerPixel);

        [Inline(InlineBehavior.Remove)]
        private static unsafe void ScalarizedScaleRects(ref RectF* ptr, ref nuint length, Vector2 pointsPerPixel)
        {
            (float pointsPerPixelX, float pointsPerPixelY) = pointsPerPixel;
            for (; length >= 4; length -= 4, ptr += 4)
            {
                ScaleRectAndStore(ptr, pointsPerPixelX, pointsPerPixelY);
                ScaleRectAndStore(ptr + 1, pointsPerPixelX, pointsPerPixelY);
                ScaleRectAndStore(ptr + 2, pointsPerPixelX, pointsPerPixelY);
                ScaleRectAndStore(ptr + 3, pointsPerPixelX, pointsPerPixelY);
            }
            RectF* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return;
            ScaleRectAndStore(ptr, pointsPerPixelX, pointsPerPixelY);
            ptr++;
            if (ptr >= ptrEnd)
                return;
            ScaleRectAndStore(ptr, pointsPerPixelX, pointsPerPixelY);
            ptr++;
            if (ptr >= ptrEnd)
                return;
            ScaleRectAndStore(ptr, pointsPerPixelX, pointsPerPixelY);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ScaleRectAndStore(RectF* ptr, float pointsPerPixelX, float pointsPerPixelY)
            {
                ScaleAndStore((float*)ptr, pointsPerPixelX);
                ScaleAndStore(((float*)ptr) + 1, pointsPerPixelY);
                ScaleAndStore(((float*)ptr) + 2, pointsPerPixelX);
                ScaleAndStore(((float*)ptr) + 3, pointsPerPixelY);
            }

            [Inline(InlineBehavior.Remove)]
            static void ScaleAndStore(float* ptr, float pointsPerPixel) => *ptr = MathI.Round(*ptr * pointsPerPixel, MidpointRounding.AwayFromZero);
        }
    }
}
