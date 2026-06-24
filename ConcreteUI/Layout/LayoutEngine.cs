using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;



#if !NET8_0_OR_GREATER
using WitherTorch.Common.Extensions;
#endif

namespace ConcreteUI.Layout;

public sealed class LayoutEngine : ILayoutEngine
{
    private static readonly Pool<LayoutEngine> _pool = new Pool<LayoutEngine>(1);

    private const int Capacity = 1 << 9; // 512
    private const int SegmentLength = (int)LayoutProperty._Last;

    private readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> _elementDict = new();
    private readonly Dictionary<LayoutNode, int> _computeDict = new();
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
        RecalculateLayoutCore(pageSize, timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>
    {
        if (pageSize.Width < 0 || pageSize.Height < 0)
            return;
        QueueElements(elements, timestamp);
        if (_elementDict.Count <= 0)
            return;
        RecalculateLayoutCore(pageSize, timestamp);
    }

    private void QueueElements<TEnumerable>(TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>
    {
        UIElement?[] array;
        int length;

        if (typeof(TEnumerable) == typeof(UIElement?[]))
            goto Array;
        if (typeof(TEnumerable) == typeof(UnwrappableList<UIElement?>))
            goto UnwrappableList;
        if (typeof(TEnumerable) == typeof(ObservableList<UIElement?>))
            goto ObservableList;
        if (typeof(TEnumerable) == typeof(ICollection<UIElement?>))
            goto Collection;

        switch (elements)
        {
            case UIElement?[]:
                goto Array;
            case UnwrappableList<UIElement?>:
                goto UnwrappableList;
            case ObservableList<UIElement?>:
                goto ObservableList;
            case ICollection<UIElement?>:
                goto Collection;
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
        goto Collection;

    ArrayLike:
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                Array.Copy(array, buffer, length);
                QueueElementsCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, timestamp);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return;

    Collection:
        ICollection<UIElement?> collection = UnsafeHelper.As<TEnumerable, ICollection<UIElement?>>(elements);
        length = collection.Count;
        if (length > 0)
        {
            ArrayPool<UIElement?> pool = ArrayPool<UIElement?>.Shared;
            UIElement?[] buffer = pool.Rent(length);
            try
            {
                collection.CopyTo(buffer, 0);
                QueueElementsCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, timestamp);
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
            using PooledList<UIElement?> bufferList = new(pool);
            do
            {
                bufferList.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            (UIElement?[] buffer, length) = bufferList;
            try
            {
                QueueElementsCore(in UnsafeHelper.GetArrayDataReference(buffer), (nuint)length, timestamp);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void QueueElementsCore(ref readonly UIElement? elementArrayRef, nuint length, ulong timestamp)
    {
        DebugHelper.ThrowIf(length < 1);
        for (; length >= 4; length -= 4)
        {
            QueueElementCore(in elementArrayRef, length - 1, timestamp);
            QueueElementCore(in elementArrayRef, length - 2, timestamp);
            QueueElementCore(in elementArrayRef, length - 3, timestamp);
            QueueElementCore(in elementArrayRef, length - 4, timestamp);
        }
        switch (length)
        {
            case 3:
                QueueElementCore(in elementArrayRef, length - 1, timestamp);
                QueueElementCore(in elementArrayRef, length - 2, timestamp);
                QueueElementCore(in elementArrayRef, length - 3, timestamp);
                break;
            case 2:
                QueueElementCore(in elementArrayRef, length - 1, timestamp);
                QueueElementCore(in elementArrayRef, length - 2, timestamp);
                break;
            case 1:
                QueueElementCore(in elementArrayRef, length - 1, timestamp);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void QueueElementCore(ref readonly UIElement? elementArrayRef, nuint i, ulong timestamp)
    {
        UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i);
        if (element is null)
            return;
        QueueElement(element, timestamp);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
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
            element.SetLastLayoutTimestampUnsafe(timestamp);
        else
            elementDict[element] = segment;
        if (element is IElementContainer container)
            QueueElements(container.GetElements(), timestamp);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe void RecalculateLayoutCore(Size pageSize, ulong timestamp)
    {
        Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict = _elementDict;
        Dictionary<LayoutNode, int> computeDict = _computeDict;
        LayoutNodeManager nodeManager = new LayoutNodeManager(elementDict, computeDict, pageSize);

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
