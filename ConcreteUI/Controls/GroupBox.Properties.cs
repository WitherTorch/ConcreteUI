using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

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
            get => GetContentLeftCore(Left);
        }

        public LayoutNode ContentLeftDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Left);
        }

        public int ContentTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentTopCore(Top);
        }

        public LayoutNode ContentTopDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Top);
        }

        public int ContentRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentRightCore(Right);
        }

        public LayoutNode ContentRightDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Right);
        }

        public int ContentBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentBottomCore(Bottom);
        }

        public LayoutNode ContentBottomDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Bottom);
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

        public LayoutNode ContentWidthDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Width);
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

        public LayoutNode ContentHeightDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentLayoutDefinitionCore((nuint)LayoutProperty.Height);
        }

        public int TextTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetTextTopCore(Location.Y);
        }

        public LayoutNode TextTopDefinition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                TextTopNode? result = _textTopReference;
                if (result is null)
                {
                    WeakReference<GroupBox>? reference = InterlockedHelper.Read(ref _reference);
                    if (reference is null)
                    {
                        reference = new WeakReference<GroupBox>(this);
                        WeakReference<GroupBox>? oldReference = InterlockedHelper.CompareExchange(ref _reference, reference, null);
                        if (oldReference is not null)
                            reference = oldReference;
                    }
                    _textTopReference = result = new TextTopNode(reference);
                }
                return result;
            }
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
