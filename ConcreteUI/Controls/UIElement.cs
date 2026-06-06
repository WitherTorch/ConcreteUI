using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Layout.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    public abstract partial class UIElement
    {
        private static int _identifierGenerator = 0;

        private readonly LayoutNode?[] _layoutDefinitions = new LayoutNode?[(int)LayoutProperty._Last];
        private readonly LayoutNode?[] _layoutExpressions = new LayoutNode?[(int)LayoutProperty._Last];
        private readonly object _syncLock;
        private readonly int _identifier;

        private WeakReference<UIElement>? _reference;
        private IElementContainer _parent;
        private IThemeContext? _themeContext;
        private string _themePrefix;
        private ulong _location, _size;
        private nuint _requestRedraw, _shouldUpdateWhenUnfreeze, _parentVersion, _boundsVersion;
        private nint _freezeCount;

        public UIElement(IElementContainer parent, string themePrefix)
        {
            _parent = parent;
            _identifier = InterlockedHelper.GetAndIncrement(ref _identifierGenerator);
            _themePrefix = themePrefix;
            _requestRedraw = UnsafeHelper.GetMaxValue<nuint>();
            _syncLock = _layoutDefinitions; // 物件重用
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
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property);

        [Inline(InlineBehavior.Remove)]
        private void SetLayoutExpressionCore(nuint property, LayoutNode? variable)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property) = variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBackgroundOpaque() => IsBackgroundOpaqueCore() || _parent.IsBackgroundOpaque(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MonitorLockScope EnterSyncScope() => MonitorLockScope.Enter(_syncLock);

        protected virtual bool IsBackgroundOpaqueCore() => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FreezeUpdate()
        {
            if (InterlockedHelper.Increment(ref _freezeCount) != 1)
                return;
            InterlockedHelper.Exchange(ref _shouldUpdateWhenUnfreeze, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UnfreezeUpdate(bool forceUpdate)
        {
            switch (InterlockedHelper.GetAndDecrement(ref _freezeCount))
            {
                case 1:
                    if (forceUpdate || InterlockedHelper.Exchange(ref _shouldUpdateWhenUnfreeze, default) != default)
                        Update();
                    break;
                case 0:
                    InterlockedHelper.CompareExchange(ref _freezeCount, 0, -1);
                    break;
                default:
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Update()
        {
            const nuint RequestRedrawBit = 0b01;

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
            if (_themeContext is not null)
                return;
            lock (_syncLock)
                ApplyThemeCore(provider);
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyThemeContext(IThemeContext? value)
        {
            if (value is not null)
            {
                ApplyThemeContextCore(value);
                return;
            }
            IThemeResourceProvider? provider = Parent.GetRenderer().GetThemeResourceProvider();
            if (provider is not null)
                ApplyThemeCore(provider);
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyThemeContextCore(IThemeContext themeContext)
        {
            IElementContainer parent = Parent;
            IRenderer renderer = parent.GetRenderer();
            CoreWindow window = parent.GetWindow();
            lock (_syncLock)
            {
                IThemeResourceProvider provider = ThemeResourceProvider.CreateResourceProvider(
                    renderer.GetDeviceContext(), themeContext, window.ActualWindowMaterial);
                try
                {
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
        internal void SetBoundsInternal(in Rectangle bounds) => SetBoundsCore_Pure(bounds);

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
}
