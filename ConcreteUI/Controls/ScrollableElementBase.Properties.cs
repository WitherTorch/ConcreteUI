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
                RecalculateLayout();
                Update();
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

                RecalculateLayout();
                Update();
            }
        }

        protected Size SurfaceSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _surfaceSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Size oldSize = _surfaceSize;
                if (oldSize == value)
                    return;
                _surfaceSize = value;

                int contentHeight = _contentBounds.Height;
                if (contentHeight <= 0)
                    return;
                int oldHeight = oldSize.Height;
                int newHeight = value.Height;
                if (oldHeight == newHeight)
                    return;
                bool isSticky = StickBottom;
                bool recalcScrollBarImmediately = false;
                if (oldHeight > contentHeight)
                {
                    isSticky &= _viewportPoint.Y + contentHeight >= oldHeight;
                    recalcScrollBarImmediately = newHeight != oldHeight;
                }
                else
                {
                    recalcScrollBarImmediately = newHeight > contentHeight;
                }
                if (recalcScrollBarImmediately)
                {
                    RecalculateLayout();
                    if (value.Width == 0)
                    {
                        value.Width = _contentBounds.Width;
                        _surfaceSize = value;
                    }
                }
                else
                {
                    RecalcScrollBarAndUpdate();
                    if (value.Width == 0)
                    {
                        value.Width = oldSize.Width;
                        _surfaceSize = value;
                    }
                }
                if (isSticky)
                    ScrollToEnd();
                Point viewportPoint = ViewportPoint;
                if (!viewportPoint.IsEmpty) 
                    ScrollingToPoint(viewportPoint.X, viewportPoint.Y);
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
