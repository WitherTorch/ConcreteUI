using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
    }
}
