using System.Collections.Generic;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    partial class PopupContainer
    {
        public bool IsDisposed => _disposed;

        public UIElement? FirstChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _children.GetUnderlyingList().FirstOrDefault();
        }

        public UIElement? LastChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _children.GetUnderlyingList().LastOrDefault();
        }
    }
}
