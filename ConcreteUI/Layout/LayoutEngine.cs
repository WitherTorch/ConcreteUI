using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Layout
{
    public sealed class LayoutEngine
    {
        private readonly TreeDictionary<UIElement, LayoutVariable?[]> _elementDict = new();
        private readonly TreeDictionary<LayoutVariable, StrongBox<int?>> _computeDict = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueueElements(IEnumerable<UIElement> elements)
        {
            switch (elements)
            {
                case UIElement[] _array:
                    QueueElements(_array);
                    break;
                case UnwrappableList<UIElement> _list:
                    QueueElements(_list);
                    break;
                default:
                    QueueElementsCore(elements);
                    break;
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElementsCore(IEnumerable<UIElement> elements)
        {
            IEnumerator<UIElement> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIElement element = enumerator.Current;
                if (element is null)
                    continue;
                QueueElement(element);
            }
            enumerator.Dispose();
        }

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UnwrappableList<UIElement> list)
            => QueueElements(list.Unwrap(), list.Count);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UIElement[] elements)
            => QueueElements(elements, elements.Length);

        [Inline(InlineBehavior.Remove)]
        private void QueueElements(UIElement[] elements, int length)
        {
            for (int i = 0; i < length; i++)
            {
                UIElement element = elements[i];
                if (element is null)
                    continue;
                QueueElement(element);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void QueueElement(UIElement element)
        {
            LazyTinyRef<LayoutVariable[]> contextsLazy = new LazyTinyRef<LayoutVariable[]>(static () => new LayoutVariable[(int)LayoutProperty._Last]);

            TreeDictionary<UIElement, LayoutVariable?[]> elementDict = _elementDict;
            TreeDictionary<LayoutVariable, StrongBox<int?>> computeDict = _computeDict;
            for (LayoutProperty prop = LayoutProperty.Left; prop < LayoutProperty._Last; prop++)
            {
                LayoutVariable? variable = element.GetLayoutVariable(prop);
                if (variable is null)
                    continue;
                contextsLazy.Value[(int)prop] = variable;
                if (computeDict[variable] is not null)
                    continue;
                computeDict[variable] = new StrongBox<int?>(null);
            }
            _elementDict[element] = contextsLazy.GetValueDirectly();
            if (element is IContainerElement containerElement)
                QueueElements(containerElement.Children);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RecalculateLayout(in Rect pageRect, UIElement element)
        {
            if (!pageRect.IsValid)
                return;
            QueueElement(element);
            RecalculateLayoutCore(in pageRect);
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
        private unsafe void RecalculateLayoutCore(in Rect pageRect, UIElement[] elements, int length)
        {
            if (length <= 0 || !ArrayHelper.HasNonNullItem(elements))
                return;
            QueueElements(elements, length);
            RecalculateLayoutCore(in pageRect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RecalculateLayoutCore(in Rect pageRect, IEnumerable<UIElement> elements)
        {
            bool hasAnyItems = false;
            foreach (UIElement element in elements)
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

        private unsafe void RecalculateLayoutCore(in Rect pageRect)
        {
            TreeDictionary<UIElement, LayoutVariable?[]> elementDict = _elementDict;
            TreeDictionary<LayoutVariable, StrongBox<int?>> computeDict = _computeDict;
            LayoutVariableManager variableManager = new LayoutVariableManager(pageRect, elementDict, computeDict);

            foreach ((UIElement element, LayoutVariable?[] variables) in elementDict)
            {
                Rectangle bounds = default;
                int* values = (int*)&bounds;

                ref LayoutVariable? variableArrayRef = ref variables[0];

                bool hasNull = false;
                for (nuint i = (nuint)LayoutProperty.Left; i <= (nuint)LayoutProperty.Top; i++)
                {
                    LayoutVariable? variable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i);
                    if (variable is null)
                    {
                        hasNull = true;
                        continue;
                    }
                    values[i] = variableManager.GetComputedValue(variable);
                }
                for (nuint i = (nuint)LayoutProperty.Width; i <= (nuint)LayoutProperty.Height; i++)
                {
                    LayoutVariable? variable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i);
                    if (variable is null)
                    {
                        hasNull = true;
                        continue;
                    }
                    values[i - 2] = variableManager.GetComputedValue(variable);
                }

                if (hasNull)
                {
                    for (nuint i = (nuint)LayoutProperty.Left; i <= (nuint)LayoutProperty.Top; i++)
                    {
                        LayoutVariable? variable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i);
                        if (variable is not null)
                            continue;
                        LayoutVariable leftVariable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i + 2) ?? throw new InvalidOperationException();
                        LayoutVariable rightVariable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i + 4) ?? throw new InvalidOperationException();
                        values[i] = variableManager.GetComputedValue(leftVariable) - variableManager.GetComputedValue(rightVariable);
                    }
                    for (nuint i = (nuint)LayoutProperty.Width; i <= (nuint)LayoutProperty.Height; i++)
                    {
                        LayoutVariable? variable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i);
                        if (variable is not null)
                            continue;
                        LayoutVariable leftVariable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i - 2) ?? throw new InvalidOperationException();
                        LayoutVariable rightVariable = UnsafeHelper.AddTypedOffset(ref variableArrayRef, i - 4) ?? throw new InvalidOperationException();
                        values[i - 2] = variableManager.GetComputedValue(leftVariable) - variableManager.GetComputedValue(rightVariable);
                    }
                }
                element.Bounds = bounds;
            }

            elementDict.Clear();
            computeDict.Clear();
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }
}
