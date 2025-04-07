using System.Drawing;
using System.Runtime.CompilerServices;

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

        protected Point SurfaceSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _surfaceSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float contentHeight = _contentBounds.Height;
                if (contentHeight > 0)
                {
                    float surfaceY = _surfaceSize.Y;
                    float valueY = value.Y;
                    if (surfaceY != valueY)
                    {
                        bool originOverflow = surfaceY > contentHeight;
                        bool importOverflow = valueY > contentHeight;
                        if (originOverflow ^ importOverflow)
                        {
                            _surfaceSize = value;
                            RecalculateLayout();
                        }
                        else if (importOverflow)
                        {
                            bool isStick = StickBottom && _viewportPoint.Y + contentHeight >= surfaceY;
                            _surfaceSize = value;
                            if (isStick)
                            {
                                ScrollToEnd();
                            }
                            else
                            {
                                RecalcScrollBarAndUpdate();
                            }
                        }
                        else
                        {
                            _surfaceSize = value;
                        }
                    }
                    else if (_surfaceSize.X != value.X)
                    {
                        _surfaceSize = value;
                    }
                }
                else
                {
                    _surfaceSize = value;
                }
                if (value.X == 0)
                {
                    _surfaceSize.X = Bounds.Width - _scrollBarBounds.Width;
                }
                Point viewportPoint = ViewportPoint;
                if (!viewportPoint.IsEmpty) ScrollingToPoint(viewportPoint.X, viewportPoint.Y);
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

        protected bool UseRawScrollBarBackBrush
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _useRawScrollBarBackBrush;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _useRawScrollBarBackBrush = value;
        }
    }
}
