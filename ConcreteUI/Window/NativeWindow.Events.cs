using System;

using ConcreteUI.Window;

namespace ConcreteUI.Window
{
    public delegate void ClosingEventHandler(object? sender, ref ClosingEventArgs args);
    public delegate void WindowStateChangedEventHandler(object? sender, in WindowStateChangedEventArgs args);

    partial class NativeWindow
    {
        public event EventHandler? Shown;
        public event EventHandler? Destroyed;
        public event EventHandler? FocusedChanged;
        public event EventHandler? Resizing;
        public event EventHandler? Resized; 
        public event WindowStateChangedEventHandler? WindowStateChanged;
        public event ClosingEventHandler? Closing;

        protected virtual void OnShown(EventArgs args) => Shown?.Invoke(this, args);
        protected virtual void OnDestroyed(EventArgs args) => Destroyed?.Invoke(this, args);
        protected virtual void OnFocusedChanged(EventArgs args) => FocusedChanged?.Invoke(this, args);
        protected virtual void OnResizing(EventArgs args) => Resizing?.Invoke(this, args);
        protected virtual void OnResized(EventArgs args) => Resized?.Invoke(this, args);
        protected virtual void OnWindowStateChanged(in WindowStateChangedEventArgs args) => WindowStateChanged?.Invoke(this, args);
        protected virtual void OnClosing(ref ClosingEventArgs args) => Closing?.Invoke(this, ref args);
    }

    public ref struct ClosingEventArgs
    {
        private readonly CloseReason _reason;

        private bool _cancelled;

        public ClosingEventArgs(CloseReason reason, bool cancelled = false)
        {
            _reason = reason;
            _cancelled = cancelled;
        }

        public readonly bool Cancelled => _cancelled;

        public readonly CloseReason Reason => _reason;

        public void SetCancelled(bool cancelled) => _cancelled = cancelled;
    }

    public readonly ref struct WindowStateChangedEventArgs
    {
        private readonly WindowState _oldState, _newState;

        public WindowState OldState => _oldState;

        public WindowState NewState => _newState;

        public WindowStateChangedEventArgs(WindowState oldState, WindowState newState)
        {
            _oldState = oldState;
            _newState = newState;
        }
    }
}
