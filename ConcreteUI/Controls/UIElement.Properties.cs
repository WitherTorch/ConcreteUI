﻿using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;
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

        public LayoutVariable LeftReference
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Left);
        }

        public LayoutVariable? LeftVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Left);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Left, value);
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
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Top);
        }

        public LayoutVariable? TopVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Top);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Top, value);
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
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Right);
        }

        public LayoutVariable? RightVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Right);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Right, value);
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
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Bottom);
        }

        public LayoutVariable? BottomVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Bottom);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Bottom, value);
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
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Height);
        }

        public LayoutVariable? HeightVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Height);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Height, value);
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
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutReference(LayoutProperty.Width);
        }

        public LayoutVariable? WidthVariable
        {
            [Inline(InlineBehavior.Keep, export: true)]
            get => GetLayoutVariable(LayoutProperty.Width);
            [Inline(InlineBehavior.Keep, export: true)]
            set => SetLayoutVariable(LayoutProperty.Width, value);
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
                IThemeResourceProvider? provider = Renderer.GetThemeResourceProvider();
                if (provider is not null)
                    ApplyThemeCore(provider);
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
