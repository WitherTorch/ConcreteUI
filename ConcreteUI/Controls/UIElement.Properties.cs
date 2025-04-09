using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Theme;

using InlineMethod;

namespace ConcreteUI.Controls
{
    partial class UIElement
    {
        public int ElementId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _identifier;
        }

        public IRenderer Renderer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _renderer;
        }

        public IContainerElement? Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _parent;
            set
            {
                if (_parent == value)
                    return;
                _parent = value;
                Update();
            }
        }

        public Point Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Location;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.Location = value;
                OnLocationChanged();
            }
        }

        public Size Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Size;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.Size = value;
                OnSizeChanged();
            }
        }

        public Rectangle Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Point location = _bounds.Location;
                Size size = _bounds.Size;
                _bounds = value;
                bool isLocationChanged = location != value.Location;
                bool isSizeChanged = size != value.Size;
                if (isLocationChanged)
                    OnLocationChanged();
                if (isSizeChanged)
                    OnSizeChanged();
            }
        }

        public int X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.X = value;
                OnLocationChanged();
            }
        }

        public int Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Left;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.X = value;
                OnLocationChanged();
            }
        }

        public AbstractCalculation LeftCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Left);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Left, value);
        }

        public int Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.Y = value;
                OnLocationChanged();
            }
        }

        public int Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Top;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.X = value;
                OnLocationChanged();
            }
        }

        public AbstractCalculation TopCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Top);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Top, value);
        }

        public int Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Right;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int newWidth = value - _bounds.X;
                _bounds.Width = newWidth;
                OnSizeChanged();
            }
        }

        public AbstractCalculation RightCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Right);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Right, value);
        }

        public int Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Bottom;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int newBottom = value - _bounds.Y;
                _bounds.Height = newBottom;
                OnSizeChanged();
            }
        }

        public AbstractCalculation BottomCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Bottom);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Bottom, value);
        }

        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Height;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.Height = value;
                OnSizeChanged();
            }
        }

        public AbstractCalculation HeightCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Height);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Height, value);
        }

        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bounds.Width;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bounds.Width = value;
                OnSizeChanged();
            }
        }

        public AbstractCalculation WidthCalculation
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutCalculation(LayoutProperty.Width);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutCalculation(LayoutProperty.Width, value);
        }

        public IThemeContext? CurrentTheme
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _themeContext;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (ReferenceEquals(_themeContext, value))
                    return;
                _themeContext = value;
                if (value is not null)
                {
                    ApplyThemeContext(value);
                    return;
                }
                ApplyThemeCore(Renderer.GetThemeResourceProvider());
                Update();
            }
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public int GetProperty([InlineParameter] LayoutProperty property)
            => property switch
            {
                LayoutProperty.Left => Left,
                LayoutProperty.Top => Top,
                LayoutProperty.Right => Right,
                LayoutProperty.Bottom => Bottom,
                LayoutProperty.Height => Height,
                LayoutProperty.Width => Width,
                _ => 0,
            };

        [Inline(InlineBehavior.Keep, export: true)]
        public void SetProperty([InlineParameter] LayoutProperty property, int value)
        {
            switch (property)
            {
                case LayoutProperty.Left:
                    Left = value;
                    break;
                case LayoutProperty.Top:
                    Top = value;
                    break;
                case LayoutProperty.Right:
                    Right = value;
                    break;
                case LayoutProperty.Bottom:
                    Bottom = value;
                    break;
                case LayoutProperty.Height:
                    Height = value;
                    break;
                case LayoutProperty.Width:
                    Width = value;
                    break;
            }
        }
    }
}
