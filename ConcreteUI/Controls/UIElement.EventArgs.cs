using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ConcreteUI.Controls
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

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public readonly ref struct MouseInteractEventArgs
    {
        public readonly PointF Location;
        public readonly float X;
        public readonly float Y;
        public readonly MouseButtons Button;
        public readonly int Delta;

        public MouseInteractEventArgs(PointF point)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = MouseButtons.None;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, MouseButtons buttons)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = buttons;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, int delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = MouseButtons.None;
            Delta = delta;
        }

        public MouseInteractEventArgs(PointF point, MouseButtons buttons, int delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = buttons;
            Delta = delta;
        }
    }
}
