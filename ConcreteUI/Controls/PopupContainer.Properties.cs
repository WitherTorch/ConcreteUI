using System.Collections.Generic;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    partial class PopupContainer
    {
        public IReadOnlyCollection<UIElement> Children => _children.GetUnderlyingList().AsReadOnlyList();

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
