using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    partial class UIElement
    {
        public int ElementId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _identifier;
        }

        public CoreWindow Window => Parent.GetWindow();

        protected IRenderer Renderer => Parent.GetRenderer();

        public bool IsRenderedOnce
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CheckIsRenderedOnce(InterlockedHelper.Read(ref _requestRedraw));
        }

        public IElementContainer Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly IElementContainer parentRef = ref _parent;
                ref readonly nuint versionRef = ref _parentVersion;
                IElementContainer parent = OptimisticLock.EnterWithObject(in parentRef, in versionRef, out nuint version);
                while (!OptimisticLock.TryLeaveWithObject(in parentRef, in versionRef, ref parent, ref version)) ;
                return parent;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref IElementContainer parentRef = ref _parent;
                ref nuint versionRef = ref _parentVersion;
                if (ReferenceEquals(InterlockedHelper.Exchange(ref _parent, value), value))
                    return;
                OptimisticLock.Increase(ref versionRef);
                Update();
            }
        }

        public Point Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLocationCore(value);
        }

        public Size Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetSizeCore();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetSizeCore(value);
        }

        public Rectangle Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Rectangle(GetLocationCore(), GetSizeCore());
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                bool locationChanged = SetLocationCore_Pure(value.Location);
                bool sizeChanged = SetSizeCore_Pure(value.Size);
                if (locationChanged)
                {
                    OptimisticLock.Increase(ref _boundsVersion);
                    OnLocationChanged();
                    if (sizeChanged)
                        OnSizeChanged();
                }
                else if (sizeChanged)
                {
                    OptimisticLock.Increase(ref _boundsVersion);
                    OnSizeChanged();
                }
            }
        }

        public int X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLocationCore(GetLocationCore() with { X = value });
        }

        public int Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLocationCore(GetLocationCore() with { X = value });
        }

        public LayoutVariable LeftReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Left);
        }

        public LayoutVariable? LeftVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Left);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Left, value);
        }

        public int Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLocationCore(GetLocationCore() with { Y = value });
        }

        public int Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLocationCore(GetLocationCore() with { Y = value });
        }

        public LayoutVariable TopReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Top);
        }

        public LayoutVariable? TopVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Top);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Top, value);
        }

        public int Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().X + GetSizeCore().Width;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetSizeCore(GetSizeCore() with { Width = value - GetLocationCore().X });
        }

        public LayoutVariable RightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Right);
        }

        public LayoutVariable? RightVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Right);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Right, value);
        }

        public int Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLocationCore().Y + GetSizeCore().Height;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetSizeCore(GetSizeCore() with { Height = value - GetLocationCore().Y });
        }

        public LayoutVariable BottomReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Bottom);
        }

        public LayoutVariable? BottomVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Bottom);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Bottom, value);
        }

        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetSizeCore().Height;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetSizeCore(GetSizeCore() with { Height = value });
        }

        public LayoutVariable HeightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Height);
        }

        public LayoutVariable? HeightVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Height);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Height, value);
        }

        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetSizeCore().Width;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetSizeCore(GetSizeCore() with { Width = value });
        }

        public LayoutVariable WidthReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutReferenceCore((nuint)LayoutProperty.Width);
        }

        public LayoutVariable? WidthVariable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetLayoutVariableCore((nuint)LayoutProperty.Width);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLayoutVariableCore((nuint)LayoutProperty.Width, value);
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
                _themePrefix = value;
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
