using System.Drawing;
using System.Runtime.CompilerServices;

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
                Update(UpdateFlags.RecalcLayout);
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

                Update();
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

                Update(UpdateFlags.RecalcLayout);
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
                Update(UpdateFlags.RecalcLayout);
            }
        }

        protected Point ViewportPoint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _viewportPoint;
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
                _scrollBarThemePrefix = value;
                OnScrollBarThemePrefixChanged(value);
            }
        }
    }
}
