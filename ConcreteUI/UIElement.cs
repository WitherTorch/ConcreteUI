using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Layout.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Windows;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

namespace ConcreteUI;

public abstract partial class UIElement : ICheckableDisposable
{
    private static int _identifierGenerator = 0;

    private readonly LayoutNode?[] _layoutDefinitions = new LayoutNode?[(int)LayoutProperty._Last];
    private readonly LayoutNode?[] _layoutExpressions = new LayoutNode?[(int)LayoutProperty._Last];
    private readonly Lock _syncLock = new Lock(), _themeAccessLock = new Lock();
    private readonly int _identifier;

    private WeakReference<UIElement>? _reference;
    private IElementContainer _parent;
    private IThemeContext? _themeContext;
    private string _themePrefix;
    private object? _tag;
    private GCHandle _themeResourceProviderReference;
    private ulong _location, _size, _lastLayoutTimestamp;
    private nuint _requestRedraw, _shouldUpdateWhenUnfreeze, _freezeCount,
        _parentVersion, _boundsVersion, _tagVersion,
        _disposed;

    public UIElement(IElementContainer parent, string themePrefix)
    {
        _parent = parent;
        _identifier = InterlockedHelper.GetAndIncrement(ref _identifierGenerator);
        _themePrefix = themePrefix;
        _requestRedraw = UnsafeHelper.GetMaxValue<nuint>();
        _themeResourceProviderReference = GCHandle.Alloc(null, GCHandleType.Weak);
    }

    [Inline(InlineBehavior.Remove)]
    private WeakReference<UIElement> GetWeakReference() => _reference ??= new WeakReference<UIElement>(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode GetLayoutDefinition(LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return Throw();
        return GetLayoutDefinitionCore((nuint)property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static LayoutNode Throw() => throw new ArgumentOutOfRangeException(nameof(property));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode? GetLayoutExpression(LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return Throw();
        return GetLayoutExpressionCore((nuint)property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static LayoutNode Throw() => throw new ArgumentOutOfRangeException(nameof(property));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayoutExpression(LayoutProperty property, LayoutNode? variable)
    {
        if (property >= LayoutProperty._Last)
        {
            Throw();
            return;
        }
        SetLayoutExpressionCore((nuint)property, variable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Throw() => throw new ArgumentOutOfRangeException(nameof(property));
    }

    [Inline(InlineBehavior.Remove)]
    private LayoutNode GetLayoutDefinitionCore(nuint property)
    {
        ref LayoutNode? variable = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutDefinitions), property);
        return variable ??= new UIElementLayoutNode(GetWeakReference(), (LayoutProperty)property);
    }

    [Inline(InlineBehavior.Remove)]
    private LayoutNode? GetLayoutExpressionCore(nuint property)
        => InterlockedHelper.Read(ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property));

    [Inline(InlineBehavior.Remove)]
    private void SetLayoutExpressionCore(nuint property, LayoutNode? variable)
    {
        InterlockedHelper.Write(ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property), variable);
        ResetLastLayoutTimestamp();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureThemeIsApplied()
    {
        if (InterlockedHelper.Read(ref _themeContext) is not null)
            return;
        IThemeResourceProvider? provider = Window.GetThemeResourceProvider();
        if (provider is null)
            return;
        lock (_themeAccessLock)
        {
            if (ReferenceEquals(_themeResourceProviderReference.Target, provider))
                return;
            _themeResourceProviderReference.Target = provider;
            lock (_syncLock)
                ApplyThemeCore(provider);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ResetLastLayoutTimestamp() => InterlockedHelper.Write(ref _lastLayoutTimestamp, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLastLayoutTimestampUnsafe(ulong timestamp) => InterlockedHelper.Write(ref _lastLayoutTimestamp, timestamp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CheckLayoutOutdated()
    {
        CoreWindow window = Window;
        if (window.CheckLayoutTimestampOutdated(InterlockedHelper.Read(ref _lastLayoutTimestamp)) || InterlockedHelper.Read(ref _themeContext) is not null)
            return true;
        IThemeResourceProvider? provider = window.GetThemeResourceProvider();
        if (provider is null)
            return false;
        lock (_themeAccessLock)
            return !ReferenceEquals(_themeResourceProviderReference.Target, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Lock.Scope EnterSyncScope() => _syncLock.EnterScope();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBackgroundOpaque() => IsBackgroundOpaqueCore() || Parent.IsBackgroundOpaque(this);

    protected virtual bool IsBackgroundOpaqueCore() => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void FreezeUpdate()
    {
        if (InterlockedHelper.Read(ref _disposed) != default || InterlockedHelper.LimitedIncrement(ref _freezeCount, UnsafeHelper.GetMaxValue<nuint>()) != 1)
            return;
        InterlockedHelper.Exchange(ref _shouldUpdateWhenUnfreeze, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UnfreezeUpdate(bool forceUpdate)
    {
        if (InterlockedHelper.Read(ref _disposed) != default ||
            InterlockedHelper.LimitedDecrement(ref _freezeCount, 0) > 0 ||
            (!forceUpdate && InterlockedHelper.Exchange(ref _shouldUpdateWhenUnfreeze, default) == default))
            return;
        Update();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void Update()
    {
        const nuint RequestRedrawBit = 0b01;

        if (InterlockedHelper.Read(ref _disposed) != default)
            return;

        InterlockedHelper.CompareExchange(ref _shouldUpdateWhenUnfreeze, UnsafeHelper.GetMaxValue<nuint>(), 0);
        if (InterlockedHelper.Read(ref _freezeCount) != default ||
            !CheckIsRenderedOnce(InterlockedHelper.Or(ref _requestRedraw, RequestRedrawBit)))
            return;
        UpdateCore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UpdateCore() => Renderer.Refresh();

    public virtual void Render(in RegionalRenderingContext context) => Render(in context, markDirty: true);

    public void Render(in RegionalRenderingContext context, bool markDirty)
    {
        lock (_syncLock)
        {
            try
            {
                ResetNeedRefreshFlag();
                if (!RenderCore(in context))
                    Update();
            }
            finally
            {
                if (markDirty)
                    context.MarkAsDirty();
            }
        }
    }

    protected void RenderBackground(in RegionalRenderingContext context) => Parent.RenderBackground(this, in context);

    protected void RenderBackground(in RegionalRenderingContext context, D2D1Brush backBrush)
    {
        if (backBrush is D2D1SolidColorBrush solidColorBrush)
        {
            if (GraphicsUtils.CheckBrushIsSolid(solidColorBrush))
            {
                context.Clear(solidColorBrush.Color);
                return;
            }
            RenderBackground(context);
            context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), backBrush);
            return;
        }
        bool isSolidBrush = backBrush switch
        {
            D2D1LinearGradientBrush linearGradientBrush => GraphicsUtils.CheckBrushIsSolid(linearGradientBrush),
            D2D1RadialGradientBrush radialGradientBrush => GraphicsUtils.CheckBrushIsSolid(radialGradientBrush),
            _ => false
        };
        if (!isSolidBrush)
            RenderBackground(context);
        context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), backBrush);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool NeedRefresh() => InterlockedHelper.Read(ref _requestRedraw) != default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ResetNeedRefreshFlag() => InterlockedHelper.Exchange(ref _requestRedraw, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsRenderedOnce(ulong requestRedraw)
    {
        const ulong FirstTimeRenderBit = 0b10;
        return (requestRedraw & FirstTimeRenderBit) == 0UL;
    }

    protected abstract bool RenderCore(in RegionalRenderingContext context);

    public virtual void OnLocationChanged() { }

    public virtual void OnSizeChanged() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyTheme(IThemeResourceProvider provider)
    {
        if (InterlockedHelper.Read(ref _themeContext) is not null)
            return;

        lock (_themeAccessLock)
        {
            _themeResourceProviderReference.Target = provider;
            lock (_syncLock)
                ApplyThemeCore(provider);
        }
        Update();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyThemeContext(IThemeContext? value)
    {
        IElementContainer parent = Parent;
        CoreWindow window = parent.GetWindow();

        if (value is null)
        {
            IThemeResourceProvider? provider = window.GetThemeResourceProvider();
            _themeResourceProviderReference.Target = provider;
            if (provider is not null)
            {
                lock (_syncLock)
                    ApplyThemeCore(provider);
            }
        }
        else
        {
            _themeResourceProviderReference.Target = null;

            IRenderer renderer = parent.GetRenderer();
            IThemeResourceProvider provider = ThemeResourceProvider.CreateResourceProvider(
                renderer.GetDeviceContext(), value, window.ActualWindowMaterial);
            try
            {
                lock (_syncLock)
                    ApplyThemeCore(provider);
            }
            finally
            {
                (provider as IDisposable)?.Dispose();
            }
        }
        Update();
    }

    protected abstract void ApplyThemeCore(IThemeResourceProvider provider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBoundsInternal(in Rectangle bounds, ulong timestamp)
    {
        SetBoundsCore_Pure(bounds);
        SetLastLayoutTimestampUnsafe(timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point LocalToPage(Point point) => GraphicsUtils.PointToPage(GetLocationCore(), point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PointF LocalToPage(PointF point) => GraphicsUtils.PointToPage(GetLocationCore(), point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point PageToLocal(Point point) => GraphicsUtils.PointToLocal(GetLocationCore(), point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PointF PageToLocal(PointF point) => GraphicsUtils.PointToLocal(GetLocationCore(), point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point PageToWindow(Point point)
    {
        UIElement element = this;
        IElementContainer container = Parent;
        while (container is UIElement containerElement)
        {
            element = containerElement;
            container = containerElement.Parent;
        }
        if (container is ICoordinateTranslator translator)
            return translator.PageToWindow(element, point);
        else
            return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PointF PageToWindow(PointF point)
    {
        UIElement element = this;
        IElementContainer container = Parent;
        while (container is UIElement containerElement)
        {
            element = containerElement;
            container = containerElement.Parent;
        }
        if (container is ICoordinateTranslator translator)
            return translator.PageToWindow(element, point);
        else
            return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point WindowToPage(Point point)
    {
        UIElement element = this;
        IElementContainer container = Parent;
        while (container is UIElement containerElement)
        {
            element = containerElement;
            container = containerElement.Parent;
        }
        if (container is ICoordinateTranslator translator)
            return translator.WindowToPage(element, point);
        else
            return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PointF WindowToPage(PointF point)
    {
        UIElement element = this;
        IElementContainer container = Parent;
        while (container is UIElement containerElement)
        {
            element = containerElement;
            container = containerElement.Parent;
        }
        if (container is ICoordinateTranslator translator)
            return translator.WindowToPage(element, point);
        else
            return point;
    }

    public override int GetHashCode() => _identifier;

    public override bool Equals(object? obj) => ReferenceEquals(obj, this);

    protected virtual void DisposeCore(bool disposing)
    {
        lock (_themeAccessLock)
            _themeResourceProviderReference.Free();
    }

    public void Dispose()
    {
        lock (_syncLock)
            Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~UIElement() => Dispose(disposing: false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose(bool disposing)
    {
        if (InterlockedHelper.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != default)
            return;
        DisposeCore(disposing);
    }

    [Inline(InlineBehavior.Remove)]
    private Point GetLocationCore()
    {
        ref readonly ulong valRef = ref _location;
        ref readonly nuint versionRef = ref _boundsVersion;
        ulong val = OptimisticLock.EnterWithPrimitive(in valRef, in versionRef, out nuint version);
        while (!OptimisticLock.TryLeaveWithPrimitive(in valRef, in versionRef, ref val, ref version)) ;
        return BoundsHelper.ConvertUInt64ToPoint(val);
    }

    [Inline(InlineBehavior.Remove)]
    private void SetLocationCore(in Point value)
    {
        if (!SetLocationCore_Pure(in value))
            return;
        OnLocationChanged();
        OptimisticLock.Increase(ref _boundsVersion);
        Renderer.Update();
    }

    [Inline(InlineBehavior.Remove)]
    private bool SetLocationCore_Pure(in Point value)
    {
        ulong val = BoundsHelper.ConvertPointToUInt64(value);
        return InterlockedHelper.Exchange(ref _location, val) != val;
    }

    [Inline(InlineBehavior.Remove)]
    private Size GetSizeCore()
    {
        ref readonly ulong valRef = ref _size;
        ref readonly nuint versionRef = ref _boundsVersion;
        ulong val = OptimisticLock.EnterWithPrimitive(in valRef, in versionRef, out nuint version);
        while (!OptimisticLock.TryLeaveWithPrimitive(in valRef, in versionRef, ref val, ref version)) ;
        return BoundsHelper.ConvertUInt64ToSize(val);
    }

    [Inline(InlineBehavior.Remove)]
    private void SetSizeCore(in Size value)
    {
        if (!SetSizeCore_Pure(in value))
            return;
        OnSizeChanged();
        OptimisticLock.Increase(ref _boundsVersion);
        Renderer.Update();
    }

    [Inline(InlineBehavior.Remove)]
    private bool SetSizeCore_Pure(in Size value)
    {
        ulong val = BoundsHelper.ConvertSizeToUInt64(value);
        return InterlockedHelper.Exchange(ref _size, val) != val;
    }

    [Inline(InlineBehavior.Remove)]
    private void SetBoundsCore(in Rectangle value)
    {
        if (!SetBoundsCore_Pure(value))
            return;
        Renderer.Update();
    }

    [Inline(InlineBehavior.Remove)]
    private bool SetBoundsCore_Pure(in Rectangle value)
    {
        bool locationChanged = SetLocationCore_Pure(value.Location);
        bool sizeChanged = SetSizeCore_Pure(value.Size);
        if (locationChanged)
        {
            OnLocationChanged();
            if (sizeChanged)
                OnSizeChanged();
            OptimisticLock.Increase(ref _boundsVersion);
            return true;
        }
        if (sizeChanged)
        {
            OnSizeChanged();
            OptimisticLock.Increase(ref _boundsVersion);
            return true;
        }
        return false;
    }
}
