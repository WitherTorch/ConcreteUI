using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;

using InlineMethod;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        public string Title
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _title;
            set
            {
                if (ReferenceEquals(_title, value))
                    return;
                _title = value ?? string.Empty;
                Update(RenderObjectUpdateFlags.Title, RedrawType.RedrawAllContent);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            set
            {
                if (ReferenceEquals(_text, value))
                    return;
                _text = value ?? string.Empty;
                Update(RenderObjectUpdateFlags.Text, RedrawType.RedrawText);
            }
        }

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

        public int ContentLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLeftCore(Location.X);
        }

        public LayoutVariable ContentLeftReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Left);
        }

        public int ContentTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentTopCore(Location.Y);
        }

        public LayoutVariable ContentTopReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Top);
        }

        public int ContentRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentRightCore(Location.Y);
        }

        public LayoutVariable ContentRightReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Right);
        }

        public int ContentBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentBottomCore(Location.Y);
        }

        public LayoutVariable ContentBottomReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Bottom);
        }

        public int ContentWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Rectangle bounds = Bounds;
                int left = GetContentLeftCore(bounds.Left);
                int right = GetContentRightCore(bounds.Right);
                return right - left;
            }
        }

        public LayoutVariable ContentWidthReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Width);
        }

        public int ContentHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Rectangle bounds = Bounds;
                int top = GetContentTopCore(bounds.Top);
                int bottom = GetContentBottomCore(bounds.Bottom);
                return bottom - top;
            }
        }

        public LayoutVariable ContentHeightReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetContentLayoutReference(LayoutProperty.Height);
        }

        public int TextTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetTextTopCore(Location.Y);
        }

        public LayoutVariable TextTopReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _textTopVariableLazy.Value;
        }

        public Point ContentLocation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Point location = Location;
                return new Point(GetContentLeftCore(location.X), GetContentTopCore(location.Y));
            }
        }

        public Point TextLocation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Point location = Location;
                return new Point(GetContentLeftCore(location.X), GetTextTopCore(location.Y));
            }
        }
    }
}
