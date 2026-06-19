using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Windows;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls.Extensions;

public static class CoreWindowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OpenContextMenu(this CoreWindow _this, UIElement elementRelativeTo, ContextMenu.ContextMenuItem[] items, Point location)
    {
        if (!items.HasAnyItem())
            return;

        OpenContextMenuCore(_this, items, elementRelativeTo.LocalToPage(location));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OpenContextMenu(this CoreWindow _this, ContextMenu.ContextMenuItem[] items, Point location)
    {
        if (!items.HasAnyItem())
            return;

        OpenContextMenuCore(_this, items, location);
    }

    private static void OpenContextMenuCore(this CoreWindow _this, ContextMenu.ContextMenuItem[] items, Point location)
    {
        ContextMenu contextMenu = new ContextMenu(_this, items);
        _this.ChangeOverlayElement(contextMenu)?.Dispose();
        Rectangle pageBounds = _this.PageBounds;
        if (location.X + contextMenu.Width >= pageBounds.Right)
            location.X = location.X - contextMenu.Width + 1;
        if (location.Y + contextMenu.Height >= pageBounds.Bottom)
            location.Y = location.Y - contextMenu.Height + 1;
        contextMenu.Location = location;
    }
}
