using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class PopupPanel
    {
        private sealed class OneUIElementCollection : IReadOnlyCollection<UIElement>, ICheckableDisposable
        {
            private readonly PopupPanel _owner;

            private UIElement? _element;
            private bool _disposed;

            public bool IsDisposed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _disposed;
            }

            public OneUIElementCollection(PopupPanel owner)
            {
                _owner = owner;
            }

            public int Count => MathHelper.BooleanToInt32(_element is null);

            public UIElement? Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _element;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    UIElement? oldElement = _element;
                    if (oldElement == value)
                    {
                        if (oldElement is not null)
                            oldElement.Parent = null;
                        return;
                    }
                    _element = value;
                    if (value is not null)
                        value.Parent = _owner;
                }
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

            private void Dispose(bool disposing)
            {
                if (ReferenceHelper.Exchange(ref _disposed, true))
                    return;
                DisposeHelper.SwapDisposeWeak(ref _element);
            }

            ~OneUIElementCollection() => Dispose(disposing: false);

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

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
                    if (index == -1)
                    {
                        _index = index + 1;
                        return true;
                    }
                    index = 1;
                    return false;
                }

                public void Reset() => _index = -1;
            }
        }
    }
}
