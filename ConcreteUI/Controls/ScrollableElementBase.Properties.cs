using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class ScrollableElementBase
    {
        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;

                OnEnableChanged(value);
                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        protected bool DrawWhenDisabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _drawWhenDisabled;
            set
            {
                if (_drawWhenDisabled == value)
                    return;
                _drawWhenDisabled = value;

                Update(ScrollableElementUpdateFlags.All);
            }
        }

        protected ScrollBarType ScrollBarType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scrollBarType;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_scrollBarType == value)
                    return;
                _scrollBarType = value;

                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        protected Size SurfaceSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _surfaceSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_surfaceSize == value)
                    return;
                _surfaceSize = value;
                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        public Point ViewportPoint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _viewportPoint;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set
            {
                if (_viewportPoint == value)
                    return;
                Size surfaceSize = _surfaceSize;
                Rect bounds = _contentBounds;
                if (value.X < 0)
                    value.X = 0;
                else
                {
                    int maxX = MathHelper.Max(surfaceSize.Width - bounds.Width, 0);
                    if (value.X > maxX)
                        value.X = maxX;
                }
                if (value.Y < 0)
                    value.Y = 0;
                else
                {
                    int maxY = MathHelper.Max(surfaceSize.Height - bounds.Height, 0);
                    if (value.Y > maxY)
                        value.Y = maxY;
                }
                if (_viewportPoint == value)
                    return;
                _viewportPoint = value;
                Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
            }
        }

        protected Rect ContentBounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _contentBounds;
        }

        protected bool StickBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _stickBottom;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _stickBottom = value;
        }

        public string ScrollBarThemePrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scrollBarThemePrefix;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (SequenceHelper.Equals(_scrollBarThemePrefix, value))
                    return;
                _scrollBarThemePrefix = value.ToLowerAscii();
            }
        }
    }
}
