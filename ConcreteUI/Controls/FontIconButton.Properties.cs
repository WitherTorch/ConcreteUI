using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class FontIconButton
    {
        public FontIcon Icon
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => NullSafetyHelper.ThrowIfNull(_icon);
            set
            {
                if (ReferenceEquals(_icon, value))
                    return;
                DisposeHelper.SwapDisposeInterlocked(ref _icon, value);
                Update();
            }
        }
    }
}
