using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;

using InlineMethod;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

#if !NET8_0_OR_GREATER
using WitherTorch.Common.Extensions;
#endif

namespace ConcreteUI.Layout
{
    public sealed class LayoutEngine
    {
        private const int Capacity = 1 << 9; // 512
        private const int SegmentLength = (int)LayoutProperty._Last;

        private readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> _elementDict = new();
        private readonly Dictionary<LayoutNode, int> _computeDict = new();
        private readonly ArrayPool<LayoutNode?> _nodeArrayPool = ArrayPool<LayoutNode?>.Shared;

        private LayoutNode?[]? _currentNodeBuffer;
        private int _currentAvailableIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueueElements(IEnumerable<UIElement?> elements)
        {
            switch (elements)
            {
                case UIElement?[] _array:
                    QueueElements(_array);
                    break;
                case UnwrappableList<UIElement?> _list:
                    QueueElements(_list);
                    break;
                default:
                    QueueElementsCore(elements);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElementsCore(IEnumerable<UIElement?> elements)
        {
            IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement? element = enumerator.Current;
                if (element is null)
                    continue;
                QueueElement(element);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UnwrappableList<UIElement?> list)
            => QueueElements(list.Unwrap(), list.Count);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UIElement?[] elements)
            => QueueElements(elements, elements.Length);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UIElement?[] elements, int length)
        {
            for (int i = 0; i < length; i++)
            {
                UIElement? element = elements[i];
                if (element is null)
                    continue;
                QueueElement(element);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void QueueElement(UIElement element)
        {
            Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;

            ArraySegment<LayoutNode?> segment = default;

            for (LayoutProperty prop = LayoutProperty.Left; prop < LayoutProperty._Last; prop++)
            {
                LayoutNode? expression = element.GetLayoutExpression(prop);
                if (expression is null)
                    continue;
                LayoutNode?[]? array = segment.Array;
                if (array is null)
                {
                    segment = AllocSegment();
                    array = segment.Array;
                }
                UnsafeHelper.AddTypedOffset(in UnsafeHelper.GetArrayDataReference(array!), segment.Offset + (int)prop) = expression;
            }
            if (segment.Array is not null)
                _elementDict[element] = segment;
            if (element is IElementContainer container)
                QueueElements(container.GetElements());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecalculateLayout(in Rect pageRect, UIElement element)
        {
            if (!pageRect.IsValid)
                return;
            QueueElement(element);
            RecalculateLayoutCore(in pageRect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecalculateLayout(in Rect pageRect, UIElement?[] elements)
        {
            if (elements is null || !pageRect.IsValid)
                return;
            RecalculateLayoutCore(pageRect, elements, elements.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecalculateLayout(in Rect pageRect, IEnumerable<UIElement?> elements)
        {
            if (elements is null || !pageRect.IsValid)
                return;
            switch (elements)
            {
                case UIElement?[] array:
                    RecalculateLayoutCore(pageRect, array, array.Length);
                    break;
                case UnwrappableList<UIElement?> list:
                    RecalculateLayoutCore(pageRect, list.Unwrap(), list.Count);
                    break;
                default:
                    RecalculateLayoutCore(pageRect, elements);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateLayoutCore(in Rect pageRect, UIElement?[] elements, int length)
        {
            if (length <= 0 || !ArrayHelper.HasNonNullItem(elements))
                return;
            QueueElements(elements, length);
            RecalculateLayoutCore(in pageRect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateLayoutCore(in Rect pageRect, IEnumerable<UIElement?> elements)
        {
            bool hasAnyItems = false;
            foreach (UIElement? element in elements)
            {
                if (element is null)
                    return;
                hasAnyItems = true;
                QueueElement(element);
            }
            if (!hasAnyItems)
                return;
            RecalculateLayoutCore(in pageRect);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe void RecalculateLayoutCore(in Rect pageRect)
        {
            Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;
            Dictionary<LayoutNode, int> computeDict = _computeDict;
            LayoutNodeManager nodeManager = new LayoutNodeManager(pageRect, elementDict, computeDict);

            Dictionary<UIElement, ArraySegment<LayoutNode?>>.Enumerator enumerator = elementDict.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    (UIElement element, ArraySegment<LayoutNode?> expressions) = enumerator.Current;
                    try
                    {
                        Exception innerException;

                        Rectangle bounds = default;
                        int* values = (int*)&bounds;

                        ref LayoutNode? expressionArrayRef = ref UnsafeHelper.AddTypedOffset(in UnsafeHelper.GetArrayDataReference(expressions.Array!), expressions.Offset);

                        bool hasNull = false;
                        for (nuint i = (nuint)LayoutProperty.Left; i <= (nuint)LayoutProperty.Top; i++)
                        {
                            LayoutNode? expression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i);
                            if (expression is null)
                            {
                                hasNull = true;
                                continue;
                            }
                            try
                            {
                                values[i] = nodeManager.GetComputedValue(expression);
                            }
                            catch (CyclicDependencyException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                innerException = ex;
                                goto FailedWithException;
                            }
                        }
                        for (nuint i = (nuint)LayoutProperty.Width; i <= (nuint)LayoutProperty.Height; i++)
                        {
                            LayoutNode? expression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i);
                            if (expression is null)
                            {
                                hasNull = true;
                                continue;
                            }
                            try
                            {
                                values[i - 2] = nodeManager.GetComputedValue(expression);
                            }
                            catch (CyclicDependencyException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                innerException = ex;
                                goto FailedWithException;
                            }
                        }

                        if (hasNull)
                        {
                            for (nuint i = (nuint)LayoutProperty.Left; i <= (nuint)LayoutProperty.Top; i++)
                            {
                                LayoutNode? expression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i);
                                if (expression is not null)
                                    continue;
                                LayoutNode? leftExpression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i + 2);
                                LayoutNode? rightExpression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i + 4);
                                if (leftExpression is null || rightExpression is null)
                                    goto Failed;
                                values[i] = nodeManager.GetComputedValue(leftExpression) - nodeManager.GetComputedValue(rightExpression);
                            }
                            for (nuint i = (nuint)LayoutProperty.Width; i <= (nuint)LayoutProperty.Height; i++)
                            {
                                LayoutNode? expression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i);
                                if (expression is not null)
                                    continue;
                                LayoutNode? leftExpression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i - 2);
                                LayoutNode? rightExpression = UnsafeHelper.AddTypedOffset(ref expressionArrayRef, i - 4);
                                if (leftExpression is null || rightExpression is null)
                                    goto Failed;
                                values[i - 2] = nodeManager.GetComputedValue(leftExpression) - nodeManager.GetComputedValue(rightExpression);
                            }
                        }
                        element.SetBoundsInternal(bounds);
                        continue;

                    Failed:
                        throw new InvalidOperationException($"Failed to calculate layout for {element}!");

                    FailedWithException:
                        throw new InvalidOperationException($"Failed to calculate layout for {element}!", innerException);
                    }
                    finally
                    {
                        FreeSegment(expressions);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
                elementDict.Clear();
                computeDict.Clear();
                LayoutNode?[]? buffer = ReferenceHelper.Exchange(ref _currentNodeBuffer, null);
                if (buffer is not null && _currentAvailableIndex < Capacity - SegmentLength)
                    _nodeArrayPool.Return(buffer);
                GC.Collect(0, GCCollectionMode.Optimized);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ArraySegment<LayoutNode?> AllocSegment()
        {
            LayoutNode?[]? buffer = _currentNodeBuffer;
            if (buffer is null)
                goto CreateNew;
            else
                goto UseExists;

        CreateNew:
            buffer = _nodeArrayPool.Rent(Capacity);
            _currentNodeBuffer = buffer;
            _currentAvailableIndex = SegmentLength;
            return new ArraySegment<LayoutNode?>(buffer, 0, SegmentLength);

        UseExists:
            int index = _currentAvailableIndex;
            if (index >= Capacity - SegmentLength)
                goto CreateNew;
            _currentAvailableIndex = index + SegmentLength;
            return new ArraySegment<LayoutNode?>(buffer, index, SegmentLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FreeSegment(in ArraySegment<LayoutNode?> segment)
        {
            //Check is last segment of buffer
            if (segment.Offset < Capacity - SegmentLength * 2)
                return;
            _nodeArrayPool.Return(segment.Array!);
        }
    }
}
