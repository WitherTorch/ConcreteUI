using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioUI.Layout;

public sealed class LayoutEngine : ILayoutEngine
{
    private static readonly Pool<LayoutEngine> _pool = new Pool<LayoutEngine>(1);

    private const int Capacity = 1 << 9; // 512
    private const int SegmentLength = (int)LayoutProperty._Last;

    private readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> _elementDict = new();
    private readonly Dictionary<UIElement, ArraySegment<UIElement>> _childrenDict = new();
    private readonly Dictionary<LayoutNode, int> _computeDict = new();
    private readonly ArrayPool<UIElement> _childrenArrayPool = ArrayPool<UIElement>.Shared;
    private readonly ArrayPool<LayoutNode?> _nodeArrayPool = ArrayPool<LayoutNode?>.Shared;

    private LayoutNode?[]? _currentNodeBuffer;
    private int _currentAvailableIndex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutEngineRentScope Rent() => LayoutEngineRentScope.Rent(_pool);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RecalculateLayout(Size pageSize, UIElement? element, ulong timestamp)
    {
        if (element is null || pageSize.Width < 0 || pageSize.Height < 0)
            return;
        QueueElement(element, timestamp);
        RecalculateLayoutInternal(pageSize, timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>
    {
        if (pageSize.Width < 0 || pageSize.Height < 0)
            return;
        QueueElements(elements, timestamp);
        if (_elementDict.Count <= 0)
            return;
        RecalculateLayoutInternal(pageSize, timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void QueueElements<TEnumerable>(TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        DispatchArray(in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), timestamp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DispatchArray(ref readonly UIElement? elementArrayRef, nuint length, ulong timestamp)
        {
            for (; length >= 4; length -= 4)
            {
                Dispatch(in elementArrayRef, length - 1, timestamp);
                Dispatch(in elementArrayRef, length - 2, timestamp);
                Dispatch(in elementArrayRef, length - 3, timestamp);
                Dispatch(in elementArrayRef, length - 4, timestamp);
            }
            switch (length)
            {
                case 3:
                    Dispatch(in elementArrayRef, length - 1, timestamp);
                    Dispatch(in elementArrayRef, length - 2, timestamp);
                    Dispatch(in elementArrayRef, length - 3, timestamp);
                    break;
                case 2:
                    Dispatch(in elementArrayRef, length - 1, timestamp);
                    Dispatch(in elementArrayRef, length - 2, timestamp);
                    break;
                case 1:
                    Dispatch(in elementArrayRef, length - 1, timestamp);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Dispatch(ref readonly UIElement? elementArrayRef, nuint i, ulong timestamp)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            QueueElement(element, timestamp);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void QueueElements<TEnumerable>(UIElement parent, TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        int count = scope.Count;
        if (count <= 0)
            return;
        using PooledList<UIElement> list = new PooledList<UIElement>(_childrenArrayPool, capacity: count);
        DispatchArray(list, in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count), timestamp);
        (UIElement[] buffer, count) = list;
        if (count <= 0)
            return;
        _childrenDict[parent] = new ArraySegment<UIElement>(buffer, 0, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DispatchArray(PooledList<UIElement> list, ref readonly UIElement? elementArrayRef, nuint length, ulong timestamp)
        {
            for (; length >= 4; length -= 4)
            {
                Dispatch(list, in elementArrayRef, length - 1, timestamp);
                Dispatch(list, in elementArrayRef, length - 2, timestamp);
                Dispatch(list, in elementArrayRef, length - 3, timestamp);
                Dispatch(list, in elementArrayRef, length - 4, timestamp);
            }
            switch (length)
            {
                case 3:
                    Dispatch(list, in elementArrayRef, length - 1, timestamp);
                    Dispatch(list, in elementArrayRef, length - 2, timestamp);
                    Dispatch(list, in elementArrayRef, length - 3, timestamp);
                    break;
                case 2:
                    Dispatch(list, in elementArrayRef, length - 1, timestamp);
                    Dispatch(list, in elementArrayRef, length - 2, timestamp);
                    break;
                case 1:
                    Dispatch(list, in elementArrayRef, length - 1, timestamp);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Dispatch(PooledList<UIElement> list, ref readonly UIElement? elementArrayRef, nuint i, ulong timestamp)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
            if (element is null)
                return;
            list.Add(element);
            QueueElement(element, timestamp);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void QueueElement(UIElement element, ulong timestamp)
    {
        Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;
        ArraySegment<LayoutNode?> segment = default;

        element.EnsureThemeIsApplied();

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
            UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(array!), segment.Offset + (int)prop) = expression;
        }
        if (segment.Array is null)
            element.UpdateLayoutTimestamp(timestamp);
        else
            elementDict[element] = segment;
        if (element is IElementContainer container)
            QueueElements(element, container.GetElements(), timestamp);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RecalculateLayoutInternal(Size pageSize, ulong timestamp)
    {

        try
        {
            RecalculateLayoutCore(pageSize, timestamp);
        }
        finally
        {
            Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;
            Dictionary<UIElement, ArraySegment<UIElement>> childrenDict = _childrenDict;
            Dictionary<LayoutNode, int> computeDict = _computeDict;
            ArrayPool<UIElement> childrenArrayPool = _childrenArrayPool;

            computeDict.Clear();

            foreach (ArraySegment<LayoutNode?> segment in elementDict.Values)
                FreeSegment(segment);
            elementDict.Clear();

            foreach (ArraySegment<UIElement> segment in childrenDict.Values)
                childrenArrayPool.Return(segment.Array!);
            childrenDict.Clear();

            LayoutNode?[]? buffer = ReferenceHelper.Exchange(ref _currentNodeBuffer, null);
            if (buffer is not null && _currentAvailableIndex < Capacity - SegmentLength)
                _nodeArrayPool.Return(buffer);
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe void RecalculateLayoutCore(Size pageSize, ulong timestamp)
    {
        Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;
        LayoutNodeManager nodeManager = new LayoutNodeManager(elementDict, _childrenDict, _computeDict, pageSize);
        using Dictionary<UIElement, ArraySegment<LayoutNode?>>.Enumerator enumerator = elementDict.GetEnumerator();
        while (enumerator.MoveNext())
        {
            (UIElement element, ArraySegment<LayoutNode?> expressions) = enumerator.Current;
            Exception innerException;

            Rectangle bounds = default;
            int* values = (int*)&bounds;

            ref LayoutNode? expressionArrayRef = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(expressions.Array!), expressions.Offset);

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
            element.SetBoundsInternal(bounds, timestamp);
            continue;

        Failed:
            throw new InvalidOperationException($"Failed to calculate layout for {element}!");

        FailedWithException:
            throw new InvalidOperationException($"Failed to calculate layout for {element}!", innerException);
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
