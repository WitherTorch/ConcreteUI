using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class PopupPanel
    {
        public bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _collection.IsDisposed;
        }

        public IReadOnlyCollection<UIElement> Children
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _collection;
        }

        public UIElement? FirstChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _collection.Value;
        }

        public UIElement? LastChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _collection.Value;
        }
    }
}
