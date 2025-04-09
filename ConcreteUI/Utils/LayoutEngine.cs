using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Controls.Calculation;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Utils
{
    public sealed class LayoutEngine
    {
        private readonly TreeDictionary<UIElement, ICalculationContext[]> _contextDict;

        public LayoutEngine()
        {
            _contextDict = new TreeDictionary<UIElement, ICalculationContext[]>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueueElements(ArrayPool<ICalculationContext> pool, IEnumerable<UIElement> elements)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    QueueElements(pool, _array);
                    break;
                case UnwrappableList<UIElement> _list:
                    QueueElements(pool, _list);
                    break;
                default:
                    QueueElementsCore(pool, elements);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElementsCore(ArrayPool<ICalculationContext> pool, IEnumerable<UIElement> elements)
        {
            IEnumerator<UIElement> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                QueueElement(pool, element);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(ArrayPool<ICalculationContext> pool, UnwrappableList<UIElement> list)
            => QueueElements(pool, list.Unwrap(), list.Count);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(ArrayPool<ICalculationContext> pool, UIElement[] elements)
            => QueueElements(pool, elements, elements.Length);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(ArrayPool<ICalculationContext> pool, UIElement[] elements, int length)
        {
            for (int i = 0; i < length; i++)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                QueueElement(pool, element);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void QueueElement(ArrayPool<ICalculationContext> pool, UIElement element)
        {
            ICalculationContext[] contexts = pool.Rent((int)LayoutProperty._Last);
            for (LayoutProperty prop = LayoutProperty.Left; prop < LayoutProperty._Last; prop++)
            {
                ICalculationContext? context = element.GetLayoutCalculation(prop)?.CreateContext();
                if (context is null)
                    continue;
                contexts[(int)prop] = context;
            }
            _contextDict[element] = contexts;
            if (element is IContainerElement containerElement)
                QueueElements(pool, containerElement.Children);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RecalculateLayout(in Rect pageRect, UIElement element)
        {
            if (!pageRect.IsValid)
                return;
            ArrayPool<ICalculationContext> pool = ArrayPool<ICalculationContext>.Shared;
            QueueElement(pool, element);
            RecalculateLayoutCore(pool, UnsafeHelper.AsPointerIn(in pageRect));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RecalculateLayout(in Rect pageRect, UIElement[] elements)
        {
            if (elements is null || !pageRect.IsValid)
                return;
            RecalculateLayoutCore(pageRect, elements, elements.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RecalculateLayout(in Rect pageRect, IEnumerable<UIElement> elements)
        {
            if (elements is null || !pageRect.IsValid)
                return;
            switch (elements)
            {
                case UIElement[] array:
                    RecalculateLayoutCore(pageRect, array, array.Length); 
                    break;
                case UnwrappableList<UIElement> list:
                    RecalculateLayoutCore(pageRect, list.Unwrap(), list.Count); 
                    break;
                default:
                    RecalculateLayoutCore(pageRect, elements);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RecalculateLayoutCore(Rect pageRect, UIElement[] elements, int length)
        {
            if (length <= 0 || !ArrayHelper.HasNonNullItem(elements, elements.Length))
                return;
            ArrayPool<ICalculationContext> pool = ArrayPool<ICalculationContext>.Shared;
            QueueElements(pool, elements, length);
            RecalculateLayoutCore(pool, UnsafeHelper.AsPointerIn(in pageRect));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RecalculateLayoutCore(Rect pageRect, IEnumerable<UIElement> elements)
        {
            bool hasAnyItems = false;
            ArrayPool<ICalculationContext> pool = ArrayPool<ICalculationContext>.Shared;
            foreach (UIElement element in elements)
            {
                if (element is null)
                    return;
                hasAnyItems = true;
                QueueElement(pool, element);
            }
            if (!hasAnyItems)
                return;
            RecalculateLayoutCore(pool, UnsafeHelper.AsPointerIn(in pageRect));
        }

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe void RecalculateLayoutCore(ArrayPool<ICalculationContext> pool, Rect* pPageRect)
        {
            TreeDictionary<UIElement, ICalculationContext[]> contextDict = _contextDict;
            int* values = stackalloc int[(int)LayoutProperty._Last];
            var enumerator = contextDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Destruct(out UIElement? element, out ICalculationContext[]? contexts);
                UnsafeHelper.InitBlock(values, 0, sizeof(int) * (int)LayoutProperty._Last);
                ICalculationContext? context;
                for (LayoutProperty prop = LayoutProperty.Left; prop < LayoutProperty._Last; prop++)
                {
                    context = contexts[(int)prop];
                    if (context is null)
                        continue;
                    values[(int)prop] = ResolveContext(pPageRect, context);
                }
                //右、下屬性處理
                if (contexts[(int)LayoutProperty.Right] is object)
                {
                    int value = values[(int)LayoutProperty.Right];
                    bool hasLeft = contexts[(int)LayoutProperty.Left] is object;
                    bool hasWidth = contexts[(int)LayoutProperty.Width] is object;
                    if (hasLeft)
                    {
                        if (!hasWidth)
                            values[(int)LayoutProperty.Width] = value - values[(int)LayoutProperty.Left];
                    }
                    else if (hasWidth)
                    {
                        values[(int)LayoutProperty.Left] = value - values[(int)LayoutProperty.Width];
                    }
                }
                if (contexts[(int)LayoutProperty.Bottom] is object)
                {
                    int value = values[(int)LayoutProperty.Bottom];
                    bool hasTop = contexts[(int)LayoutProperty.Top] is object;
                    bool hasHeight = contexts[(int)LayoutProperty.Height] is object;
                    if (hasTop)
                    {
                        if (!hasHeight)
                            values[(int)LayoutProperty.Height] = value - values[(int)LayoutProperty.Top];
                    }
                    else if (hasHeight)
                    {
                        values[(int)LayoutProperty.Top] = value - values[(int)LayoutProperty.Height];
                    }
                }
                element.Bounds = new Rectangle(
                    x: values[(int)LayoutProperty.Left], y: values[(int)LayoutProperty.Top],
                    height: values[(int)LayoutProperty.Height], width: values[(int)LayoutProperty.Width]);
                enumerator.Remove();
                pool.Return(contexts);
            }
            GC.Collect(0, GCCollectionMode.Optimized);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe int ResolveContext(Rect* pPageRect, ICalculationContext context)
        {
            if (context.TryGetResultIfCalculated(out int result))
                return result;

            bool dependPageRect = context.DependPageRect;
            UIElement? dependElement = context.DependedElement;
            LayoutProperty dependProperty = context.DependedProperty;

            if (dependElement is not null)
            {
                ICalculationContext? dependContext = ResolveDependencyContext(dependElement, dependProperty, out ICalculationContext? extraDependContext);
                int value;
                if (dependContext is null)
                    value = dependElement.GetProperty(dependProperty);
                else
                {
                    value = ResolveContext(pPageRect, dependContext);
                    if (extraDependContext is object)
                    {
                        switch (dependProperty)
                        {
                            case LayoutProperty.Left: //depend = 右, extra = 寬
                            case LayoutProperty.Top: //depend = 下, extra = 寬
                                value -= ResolveContext(pPageRect, extraDependContext);
                                break;
                            case LayoutProperty.Right: //depend = 左, extra = 寬
                            case LayoutProperty.Bottom: //depend = 上, extra = 高
                                value += ResolveContext(pPageRect, extraDependContext);
                                break;
                            case LayoutProperty.Height: //depend = 上, extra = 下
                            case LayoutProperty.Width: //depend = 左, extra = 右
                                value = ResolveContext(pPageRect, extraDependContext) - value;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (dependPageRect)
                    return context.DoCalc(pPageRect, value);
                else
                    return context.DoCalc(null, value);
            }

            if (dependPageRect)
                return context.DoCalc(pPageRect, 0);

            return context.DoCalc(null, 0);
        }

        [Inline(InlineBehavior.Remove)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICalculationContext? ResolveDependencyContext(UIElement element, LayoutProperty property, out ICalculationContext? extraContext)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
            {
                extraContext = null;
                return null;
            }
            //ICalculationContext[] contexts = _contextDict.TryGetValue(element, out ICalculationContext[] value) ? value : null;
            ICalculationContext[]? contexts = _contextDict[element];
            if (contexts is null)
            {
                extraContext = null;
                return null;
            }
            ICalculationContext result = contexts[(int)property];
            if (result is object)
            {
                extraContext = null;
                return result;
            }
            switch (property)
            {
                case LayoutProperty.Left:
                    result = contexts[(int)LayoutProperty.Right];
                    extraContext = contexts[(int)LayoutProperty.Width];
                    break;
                case LayoutProperty.Top:
                    result = contexts[(int)LayoutProperty.Bottom];
                    extraContext = contexts[(int)LayoutProperty.Height];
                    break;
                case LayoutProperty.Right:
                    result = contexts[(int)LayoutProperty.Left];
                    extraContext = contexts[(int)LayoutProperty.Width];
                    break;
                case LayoutProperty.Bottom:
                    result = contexts[(int)LayoutProperty.Top];
                    extraContext = contexts[(int)LayoutProperty.Height];
                    break;
                case LayoutProperty.Height:
                    result = contexts[(int)LayoutProperty.Top];
                    extraContext = contexts[(int)LayoutProperty.Bottom];
                    break;
                case LayoutProperty.Width:
                    result = contexts[(int)LayoutProperty.Left];
                    extraContext = contexts[(int)LayoutProperty.Right];
                    break;
                default:
                    extraContext = null;
                    return null;
            }
            return extraContext is null ? null : result;
        }
    }
}
