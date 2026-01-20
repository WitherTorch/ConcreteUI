using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;
using ConcreteUI.Theme;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

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

        public IElementContainer? Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InterlockedHelper.Read(ref _parent);
            set
            {
                if (InterlockedHelper.CompareExchange(ref _parent, value, null) is not null)
                    return;
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

        public LayoutVariable LeftReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Left);
        }

        public LayoutVariable? LeftVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Left);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Left, value);
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

        public LayoutVariable TopReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Top);
        }

        public LayoutVariable? TopVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Top);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Top, value);
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

        public LayoutVariable RightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Right);
        }

        public LayoutVariable? RightVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Right);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Right, value);
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

        public LayoutVariable BottomReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Bottom);
        }

        public LayoutVariable? BottomVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Bottom);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Bottom, value);
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

        public LayoutVariable HeightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Height);
        }

        public LayoutVariable? HeightVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Height);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Height, value);
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

        public LayoutVariable WidthReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore(LayoutProperty.Width);
        }

        public LayoutVariable? WidthVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore(LayoutProperty.Width);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore(LayoutProperty.Width, value);
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
                ApplyThemeContext(value);
            }
        }

        public string ThemePrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _themePrefix;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (SequenceHelper.Equals(_themePrefix, value))
                    return;
                _themePrefix = value.ToLowerAscii();
                ApplyThemeContext(_themeContext);
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
