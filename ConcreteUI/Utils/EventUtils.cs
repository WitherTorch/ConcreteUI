using System;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Utils
{
    public delegate void CancelableEventHandler(object sender, CancelableEventArgs e);
    public delegate void TextChangingEventHandler(object sender, TextChangingEventArgs e);

    public class CancelableEventArgs : EventArgs
    {
        public bool IsCanceled { get; private set; } = false;
        public void Cancel() => IsCanceled = true;
    }

    public sealed class TextChangingEventArgs : CancelableEventArgs
    {
        private string _text;

        public TextChangingEventArgs(string text)
        {
            _text = text;
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            set
            {
                if (ReferenceEquals(_text, value)) return;
                _text = value;
                IsEdited = true;
            }
        }

        public bool IsEdited { get; private set; }
    }
}
