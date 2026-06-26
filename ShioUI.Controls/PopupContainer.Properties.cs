using System.Runtime.CompilerServices;

using RiceTea.Core.Extensions;

namespace ShioUI.Controls;

partial class PopupContainer
{
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
