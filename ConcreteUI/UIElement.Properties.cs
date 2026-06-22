using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Windows;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI;

partial class UIElement
{
    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InterlockedHelper.Read(ref _disposed) != 0;
    }

    public int ElementId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _identifier;
    }

    public CoreWindow Window
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Parent.GetWindow();
    }

    protected IRenderer Renderer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Parent.GetRenderer();
    }

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
            ResetLastLayoutTimestamp();
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
        set => SetBoundsCore(value);
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

    public LayoutNode LeftDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Left);
    }

    public LayoutNode? LeftExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Left, value);
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

    public LayoutNode TopDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Top);
    }

    public LayoutNode? TopExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Top);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Top, value);
    }

    public int Right
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLocationCore().X + GetSizeCore().Width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetSizeCore(GetSizeCore() with { Width = value - GetLocationCore().X });
    }

    public LayoutNode RightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Right);
    }

    public LayoutNode? RightExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Right, value);
    }

    public int Bottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLocationCore().Y + GetSizeCore().Height;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetSizeCore(GetSizeCore() with { Height = value - GetLocationCore().Y });
    }

    public LayoutNode BottomDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Bottom);
    }

    public LayoutNode? BottomExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Bottom);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Bottom, value);
    }

    public int Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetSizeCore().Height;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetSizeCore(GetSizeCore() with { Height = value });
    }

    public LayoutNode HeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Height);
    }

    public LayoutNode? HeightExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Height);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Height, value);
    }

    public int Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetSizeCore().Width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetSizeCore(GetSizeCore() with { Width = value });
    }

    public LayoutNode WidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Width);
    }

    public LayoutNode? WidthExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Width);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Width, value);
    }

    public IThemeContext? CurrentTheme
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InterlockedHelper.Read(ref _themeContext);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (ReferenceEquals(InterlockedHelper.Exchange(ref _themeContext, value), value))
                return;
            lock (_themeAccessLock)
            {
                if (!ReferenceEquals(InterlockedHelper.Read(ref _themeContext), value))
                    return;
                ApplyThemeContext(value);
            }
        }
    }

    public string ThemePrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _themePrefix;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _themePrefix = value;
    }

    public object? Tag
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref readonly object? tagRef = ref _tag;
            ref readonly nuint versionRef = ref _tagVersion;
            object? tag = OptimisticLock.EnterWithObject(in tagRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithObject(in tagRef, in versionRef, ref tag, ref version)) ;
            return tag;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ref object? tagRef = ref _tag;
            ref nuint versionRef = ref _tagVersion;
            if (ReferenceEquals(InterlockedHelper.Exchange(ref _tag, value), value))
                return;
            OptimisticLock.Increase(ref versionRef);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetProperty(LayoutProperty property)
        => property switch
        {
            LayoutProperty.Left => Left,
            LayoutProperty.Top => Top,
            LayoutProperty.Right => Right,
            LayoutProperty.Bottom => Bottom,
            LayoutProperty.Height => Height,
            LayoutProperty.Width => Width,
            _ => throw new ArgumentOutOfRangeException(nameof(property)),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetProperty(LayoutProperty property, int value)
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
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }
    }
}
