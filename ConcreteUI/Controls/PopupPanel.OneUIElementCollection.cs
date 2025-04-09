using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupPanel
    {
        private sealed class OneUIElementCollection : IReadOnlyCollection<UIElement>, IDisposable
        {
            private UIElement? _element;

            public int Count => MathHelper.BooleanToInt32(_element is null);

            public UIElement? Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _element;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _element = value;
            }

            public IEnumerator<UIElement> GetEnumerator()
            {
                UIElement? element = _element;
                if (element is null)
                    return CollectionHelper.EmptyEnumerator<UIElement>();
                return new Enumerator(element);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                UIElement? element = _element;
                if (element is null)
                    return CollectionHelper.EmptyEnumerator();
                return new Enumerator(element);
            }

            ~OneUIElementCollection()
            {
                DisposeCore();
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }

            private void DisposeCore() => DisposeHelper.SwapDisposeWeak(ref _element);

            private sealed class Enumerator : IEnumerator<UIElement>
            {
                private readonly UIElement _element;

                private int _index;

                public Enumerator(UIElement element)
                {
                    _element = element;
                    _index = -1;
                }

                public UIElement Current
                {
                    get
                    {
                        if (_index == 0)
                            return _element;
                        throw new InvalidOperationException();
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    int index = _index;
                    if (index > 1)
                        return false;
                    _index = index + 1;
                    return true;
                }

                public void Reset() => _index = -1;
            }
        }
    }
}
