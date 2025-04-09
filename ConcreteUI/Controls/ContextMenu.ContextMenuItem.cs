using System;

namespace ConcreteUI.Controls
{
    partial class ContextMenu
    {
        public sealed class ContextMenuItem
        {
            public event EventHandler? Click;

            public bool Enabled { get; set; }

            public string Text { get; set; }

            public object? Tag { get; set; }

            public ContextMenuItem(string text)
            {
                Enabled = true;
                Text = text;
            }

            public void OnClick()
            {
                if (!Enabled)
                    return;
                Click?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
