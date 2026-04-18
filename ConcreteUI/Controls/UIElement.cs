using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Layout;
using ConcreteUI.Layout.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    public abstract partial class UIElement
    {
        private static int _identifierGenerator = 0;

        private readonly LayoutVariable?[] _layoutReferences = new LayoutVariable?[(int)LayoutProperty._Last];
        private readonly LayoutVariable?[] _layoutVariables = new LayoutVariable?[(int)LayoutProperty._Last];
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
            _syncLock = _layoutReferences; // 物件重用
        }

        [Inline(InlineBehavior.Remove)]
        private WeakReference<UIElement> GetWeakReference() => _reference ??= new WeakReference<UIElement>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable GetLayoutReference(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return GetLayoutReferenceCore((nuint)property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable? GetLayoutVariable(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return GetLayoutVariableCore((nuint)property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLayoutVariable(LayoutProperty property, LayoutVariable? variable)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            SetLayoutVariableCore((nuint)property, variable);
        }

        [Inline(InlineBehavior.Remove)]
        private LayoutVariable GetLayoutReferenceCore(nuint property)
        {
            ref LayoutVariable? variable = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutReferences), property);
            return variable ??= new UIElementLayoutVariable(GetWeakReference(), (LayoutProperty)property);
        }

        [Inline(InlineBehavior.Remove)]
        private LayoutVariable? GetLayoutVariableCore(nuint property)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutVariables), property);

        [Inline(InlineBehavior.Remove)]
        private void SetLayoutVariableCore(nuint property, LayoutVariable? variable)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutVariables), property) = variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBackgroundOpaque() => IsBackgroundOpaqueCore() || _parent.IsBackgroundOpaque(this);

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
        protected void UpdateCore() => Parent.GetRenderer().Refresh();

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
            IRenderer renderer = Parent.GetRenderer();
            lock (_syncLock)
            {
                IThemeResourceProvider provider = ThemeResourceProvider.CreateResourceProvider(renderer.GetDeviceContext(), themeContext,
                (renderer as CoreWindow)?.WindowMaterial ?? WindowMaterial.Default);
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
        public Point PointToGlobal(Point point) => GraphicsUtils.PointToGlobal(GetLocationCore(), point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToGlobal(PointF point) => GraphicsUtils.PointToGlobal(GetLocationCore(), point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToLocal(Point point) => GraphicsUtils.PointToLocal(GetLocationCore(), point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToLocal(PointF point) => GraphicsUtils.PointToLocal(GetLocationCore(), point);

        public override int GetHashCode() => _identifier;

        public override bool Equals(object? obj) => ReferenceEquals(obj, this);

        [Inline(InlineBehavior.Remove)]
        private Point GetLocationCore()
        {
            ref readonly ulong valRef = ref _location;
            ref readonly nuint versionRef = ref _boundsVersion;
            ulong val = OptimisticLock.EnterWithPrimitive(in valRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in valRef, in versionRef, ref val, ref version)) ;
            return ConvertUInt64ToPoint(in val);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetLocationCore(in Point value)
        {
            if (SetLocationCore_Pure(in value))
            {
                OnLocationChanged();
                OptimisticLock.Increase(ref _boundsVersion);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private bool SetLocationCore_Pure(in Point value)
        {
            ulong val = ConvertPointToUInt64(value);
            return InterlockedHelper.Exchange(ref _location, val) != val;
        }

        [Inline(InlineBehavior.Remove)]
        private Size GetSizeCore()
        {
            ref readonly ulong valRef = ref _size;
            ref readonly nuint versionRef = ref _boundsVersion;
            ulong val = OptimisticLock.EnterWithPrimitive(in valRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in valRef, in versionRef, ref val, ref version)) ;
            return ConvertUInt64ToSize(in val);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetSizeCore(in Size value)
        {
            if (SetSizeCore_Pure(in value))
            {
                OnSizeChanged();
                OptimisticLock.Increase(ref _boundsVersion);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private bool SetSizeCore_Pure(in Size value)
        {
            ulong val = ConvertSizeToUInt64(value);
            return InterlockedHelper.Exchange(ref _size, val) != val;
        }

        [Inline(InlineBehavior.Remove)]
        private static ref readonly ulong ConvertPointToUInt64(in Point value) => ref UnsafeHelper.As<Point, ulong>(ref UnsafeHelper.AsRefIn(in value));

        [Inline(InlineBehavior.Remove)]
        private static ref readonly ulong ConvertSizeToUInt64(in Size value) => ref UnsafeHelper.As<Size, ulong>(ref UnsafeHelper.AsRefIn(in value));

        [Inline(InlineBehavior.Remove)]
        private static ref readonly Point ConvertUInt64ToPoint(in ulong value) => ref UnsafeHelper.As<ulong, Point>(ref UnsafeHelper.AsRefIn(in value));

        [Inline(InlineBehavior.Remove)]
        private static ref readonly Size ConvertUInt64ToSize(in ulong value) => ref UnsafeHelper.As<ulong, Size>(ref UnsafeHelper.AsRefIn(in value));
    }
}
