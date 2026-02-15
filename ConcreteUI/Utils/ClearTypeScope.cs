using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ClearTypeScope : IDisposable
    {
        private readonly D2D1DeviceContext _context;
        private readonly uint _state; // 0b1_ enabled, 0b_1 state changed

        private ClearTypeScope(D2D1DeviceContext context, uint state)
        {
            _context = context;
            _state = state;
        }

        public static ClearTypeScope Enter(D2D1DeviceContext context, bool enable)
        {
            if (ClearTypeSwitcher.IsEnabled && ClearTypeSwitcher.SetClearType(context, enable))
                return new ClearTypeScope(context, 0b10u | MathHelper.BooleanToUInt32(enable));

            return new ClearTypeScope(context, 0b00u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClearTypeScope Enter(scoped in RegionalRenderingContext context, bool enable)
            => Enter(context.DeviceContext, enable);

        public static ClearTypeScope Enter(D2D1DeviceContext context, UIElement element, D2D1Brush backBrush)
        {
            if (!ClearTypeSwitcher.IsEnabled)
                goto Else;

            bool enable = element.IsBackgroundOpaque() || GraphicsUtils.CheckBrushIsSolid(backBrush);

            if (ClearTypeSwitcher.SetClearType(context, enable))
                return new ClearTypeScope(context, 0b10u | MathHelper.BooleanToUInt32(enable));

        Else:
            return new ClearTypeScope(context, 0b00u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClearTypeScope Enter(scoped in RegionalRenderingContext context, UIElement element, D2D1Brush backBrush)
            => Enter(context.DeviceContext, element, backBrush);

        public void Dispose()
        {
            uint state = _state;
            if ((state & 0b10) == 0)
                return;
            ClearTypeSwitcher.SetClearType(_context, (state & 0b01) != 0);
        }
    }
}
