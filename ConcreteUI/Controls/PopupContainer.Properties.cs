using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    partial class PopupContainer
    {
        public IReadOnlyCollection<UIElement> Children => _children.GetUnderlyingList().AsReadOnly();

        public UIElement FirstChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _children.GetUnderlyingList().FirstOrDefault();
        }

        public UIElement LastChild
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _children.GetUnderlyingList().LastOrDefault();
        }
    }
}
