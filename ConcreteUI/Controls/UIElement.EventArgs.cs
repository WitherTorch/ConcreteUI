using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Window2;

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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly ref struct MouseInteractEventArgs
    {
        public readonly PointF Location;
        public readonly float X;
        public readonly float Y;
        public readonly MouseKeys Keys;
        public readonly ushort Delta;

        public MouseInteractEventArgs(PointF point)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Keys = MouseKeys.None;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, MouseKeys keys)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Keys = keys;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, ushort delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Keys = MouseKeys.None;
            Delta = delta;
        }

        public MouseInteractEventArgs(PointF point, MouseKeys keys, ushort delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Keys = keys;
            Delta = delta;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly ref struct KeyInteractEventArgs
    {
        public readonly VirtualKey Key;
        public readonly ushort RepeatCount;

        public KeyInteractEventArgs(VirtualKey key)
        {
            Key = key;
            RepeatCount = 1;
        }

        public KeyInteractEventArgs(VirtualKey key, ushort repeatCount)
        {
            Key = key;
            RepeatCount = repeatCount;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public ref struct CancellableKeyInteractEventArgs
    {
        private readonly KeyInteractEventArgs _args;
        
        private bool _cancelled;

        public readonly VirtualKey Key => _args.Key;
        public readonly ushort RepeatCount => _args.RepeatCount;
        public readonly bool IsCancelled => _cancelled;

        public CancellableKeyInteractEventArgs(in KeyInteractEventArgs args)
        {
            _args = args;
            _cancelled = false;
        }

        public CancellableKeyInteractEventArgs(in KeyInteractEventArgs args, bool cancelled)
        {
            _args = args;
            _cancelled = cancelled;
        }

        public void SetCancelled(bool cancelled) => _cancelled = cancelled;
    }
}
