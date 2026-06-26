using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShioUI;

public delegate void MouseInteractEventHandler(UIElement sender, ref HandleableMouseEventArgs args);
public delegate void MouseNotifyEventHandler(UIElement sender, in MouseEventArgs args);
public delegate void KeyInteractEventHandler(UIElement sender, ref KeyEventArgs args);
public delegate void CharacterInteractEventHandler(UIElement sender, ref KeyEventArgs args);

public delegate void CancelableEventHandler(object sender, ref CancelableEventArgs e);
public delegate void TextChangingEventHandler(object sender, ref TextChangingEventArgs e);

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct CancelableEventArgs : ICancelableEventArgs
{
    private bool _canceled;

    public readonly bool IsCanceled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _canceled;
    }

    public CancelableEventArgs() => _canceled = false;

    public void Cancel() => _canceled = true;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct TextChangingEventArgs : ICancelableEventArgs
{
    private CancelableEventArgs _cancelableEventArgs;
    private string _text;

    public readonly bool IsCanceled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cancelableEventArgs.IsCanceled;
    }

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
            if (ReferenceEquals(_text, value)) 
                return;
            _text = value;
            IsEdited = true;
        }
    }

    public bool IsEdited { get; private set; }

    public void Cancel() => _cancelableEventArgs.Cancel();
}

public interface ICancelableEventArgs
{
    bool IsCanceled { get; }

    void Cancel();
}

public interface IHandleableEventArgs
{
    bool Handled { get; }

    void Handle();
}
