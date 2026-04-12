using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Theme;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public static partial class UIElementHelper
    {
        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            if (length <= 0)
                return;
            ApplyThemeUnsafe(provider, brushes, nodes, (nuint)length);
        }

        public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix)
        {
            int length = brushes.Length;
            if (length != nodes.Length)
                throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
            if (length <= 0)
                return;
            ApplyThemeUnsafe(provider, brushes, nodes, nodePrefix, (nuint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, nuint length)
        {
            ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
            ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
            for (nuint i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), UnsafeHelper.AddTypedOffset(in nodesRef, i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix, nuint length)
        {
            ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
            ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
            for (nuint i = 0; i < length; i++)
                ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), nodePrefix + "." + UnsafeHelper.AddTypedOffset(in nodesRef, i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyTheme(IThemeResourceProvider provider, ref D2D1Brush? brushRef, string node)
            => DisposeHelper.SwapDispose(ref brushRef, provider.TryGetBrush(node, out D2D1Brush? result) ? result.Clone() : null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyTheme<TEnumerable>(IThemeResourceProvider provider, TEnumerable elements)
            where TEnumerable : IEnumerable<UIElement>
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    ApplyThemeCore(provider, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    list.CopyTo(buffer, 0);
                    ApplyThemeCore(provider, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                ref UIElement? bufferRef = ref UnsafeHelper.GetArrayDataReference(buffer);
                try
                {
                    for (int i = 0; i < length; i++)
                        UnsafeHelper.AddTypedOffset(ref bufferRef, i) = readOnlyList[i];
                    ApplyThemeCore(provider, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
            if (enumerator.MoveNext())
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                using PooledList<UIElement?> bufferList = new PooledList<UIElement?>(pool, capacity: 16);
                do
                {
                    bufferList.Add(enumerator.Current);
                } while (enumerator.MoveNext());
                (UIElement?[] buffer, length) = bufferList;
                try
                {
                    ApplyThemeCore(provider, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        public static void RenderElements<TEnumerable>(D2D1DeviceContext context, DirtyAreaCollector collector, Vector2 pointPerPixel,
            TEnumerable elements, bool ignoreNeedRefresh) where TEnumerable : IEnumerable<UIElement?>
        {
            UIElement?[] array;
            int length;

            if (typeof(TEnumerable) == typeof(UIElement?[]))
                goto Array;
            if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
                goto UnwrappableList;
            if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
                goto ObservableList;
            if (typeof(TEnumerable) == typeof(IList<UIElement?>))
                goto List;
            if (typeof(TEnumerable) == typeof(IReadOnlyList<UIElement?>))
                goto ReadOnlyList;

            switch (elements)
            {
                case UIElement?[]:
                    goto Array;
                case UnwrappableList<UIElement?>:
                    goto UnwrappableList;
                case ObservableList<UIElement?>:
                    goto ObservableList;
                case IList<UIElement?>:
                    goto List;
                case IReadOnlyList<UIElement?>:
                    goto ReadOnlyList;
                default:
                    goto Fallback;
            }

        Array:
            array = UnsafeHelper.As<TEnumerable, UIElement?[]>(elements);
            length = array.Length;
            goto ArrayLike;

        UnwrappableList:
            UnwrappableList<UIElement?> unwrappableList = UnsafeHelper.As<TEnumerable, UnwrappableList<UIElement?>>(elements);
            array = unwrappableList.Unwrap();
            length = unwrappableList.Count;
            goto ArrayLike;

        ObservableList:
            IList<UIElement?> underlyingList = UnsafeHelper.As<TEnumerable, ObservableList<UIElement?>>(elements).GetUnderlyingList();
            elements = UnsafeHelper.As<IList<UIElement?>, TEnumerable>(underlyingList);
            if (underlyingList is UIElement?[])
                goto Array;
            if (underlyingList is UnwrappableList<UIElement?>)
                goto UnwrappableList;
            if (underlyingList is ObservableList<UIElement?>)
                goto ObservableList;
            goto List;

        ArrayLike:
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    Array.Copy(array, buffer, length);
                    RenderElementsCore(context, collector, pointPerPixel, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        List:
            IList<UIElement?> list = UnsafeHelper.As<TEnumerable, IList<UIElement?>>(elements);
            length = list.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                try
                {
                    list.CopyTo(buffer, 0);
                    RenderElementsCore(context, collector, pointPerPixel, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        ReadOnlyList:
            IReadOnlyList<UIElement?> readOnlyList = UnsafeHelper.As<TEnumerable, IReadOnlyList<UIElement?>>(elements);
            length = readOnlyList.Count;
            if (length > 0)
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                UIElement?[] buffer = pool.Rent(length);
                ref UIElement? bufferRef = ref UnsafeHelper.GetArrayDataReference(buffer);
                try
                {
                    for (int i = 0; i < length; i++)
                        UnsafeHelper.AddTypedOffset(ref bufferRef, i) = readOnlyList[i];
                    RenderElementsCore(context, collector, pointPerPixel, ref bufferRef, (nuint)length, ignoreNeedRefresh);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            return;

        Fallback:
            using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
            if (enumerator.MoveNext())
            {
                ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
                using PooledList<UIElement?> bufferList = new PooledList<UIElement?>(pool, capacity: 16);
                do
                {
                    bufferList.Add(enumerator.Current);
                } while (enumerator.MoveNext());
                (UIElement?[] buffer, length) = bufferList;
                try
                {
                    RenderElementsCore(context, collector, pointPerPixel, in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, ignoreNeedRefresh);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyThemeCore(IThemeResourceProvider provider, ref readonly UIElement? elementArrayRef, nuint length)
        {
            int i;
            for (i = 0; length >= 4; length -= 4, i += 4)
            {
                UnsafeHelper.AddTypedOffset(in elementArrayRef, i)?.ApplyTheme(provider);
                UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 1)?.ApplyTheme(provider);
                UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 2)?.ApplyTheme(provider);
                UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 3)?.ApplyTheme(provider);
            }
            switch (length)
            {
                case 3:
                    UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 2)?.ApplyTheme(provider);
                    goto case 2;
                case 2:
                    UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 1)?.ApplyTheme(provider);
                    goto case 1;
                case 1:
                    UnsafeHelper.AddTypedOffset(in elementArrayRef, i)?.ApplyTheme(provider);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RenderElementsCore(D2D1DeviceContext context, DirtyAreaCollector collector, Vector2 pointPerPixel,
            ref readonly UIElement? elementArrayRef, nuint length, bool ignoreNeedRefresh)
        {
            int i;
            for (i = 0; length >= 4; length -= 4, i += 4)
            {
                RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i), ignoreNeedRefresh);
                RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 1), ignoreNeedRefresh);
                RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 2), ignoreNeedRefresh);
                RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 3), ignoreNeedRefresh);
            }
            switch (length)
            {
                case 3:
                    RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 2), ignoreNeedRefresh);
                    goto case 2;
                case 2:
                    RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i + 1), ignoreNeedRefresh);
                    goto case 1;
                case 1:
                    RenderElement(context, collector, pointPerPixel, UnsafeHelper.AddTypedOffset(in elementArrayRef, i), ignoreNeedRefresh);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RenderElement(D2D1DeviceContext context, DirtyAreaCollector collector, Vector2 pointPerPixel,
            UIElement? element, bool ignoreNeedRefresh)
        {
            if (element is null)
                return;
            if (ignoreNeedRefresh || element.NeedRefresh() || collector.IsEmptyInstance)
            {
                bool isOpaque = element.IsBackgroundOpaque();
                ClearTypeSwitcher.SetClearType(context, isOpaque);
                using (RegionalRenderingContext renderingContext =
                    RegionalRenderingContext.Create(context, collector, pointPerPixel, (RectF)element.Bounds, D2D1AntialiasMode.Aliased, isOpaque, out _))
                    element.Render(renderingContext);
                collector = DirtyAreaCollector.Empty;
            }
            if (element is not IElementContainer container)
                return;
            RenderElements(context, collector, pointPerPixel, container.GetActiveElements(), ignoreNeedRefresh);
        }
    }
}
