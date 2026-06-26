using System.Runtime.CompilerServices;

using ShioUI.Utils;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class FontIconButton
{
    public FontIcon? Icon
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _icon;
        set
        {
            if (ReferenceEquals(_icon, value))
                return;
            DisposeHelper.SwapDisposeInterlocked(ref _icon, value);
            Update();
        }
    }
}
