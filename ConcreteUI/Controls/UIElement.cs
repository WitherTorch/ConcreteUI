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

using InlineIL;

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

        private readonly LazyTiny<LayoutVariable>[] _layoutReferences;
        private readonly LayoutVariable?[] _layoutVariables = new LayoutVariable?[(int)LayoutProperty._Last];
        private readonly SemaphoreSlim _semaphore;
        private readonly IRenderer _renderer;
        private readonly int _identifier;

        private IElementContainer? _parent;
        private IThemeContext? _themeContext;
        private string _themePrefix;
        private ulong _location, _size;
        private nuint _requestRedraw, _shouldUpdateWhenUnfreeze, _version;
        private nint _freezeCount;

        public UIElement(IRenderer renderer, string themePrefix)
        {
            _renderer = renderer;
            _semaphore = new SemaphoreSlim(1, 1);
            _identifier = InterlockedHelper.GetAndIncrement(ref _identifierGenerator);
            _themePrefix = themePrefix.ToLowerAscii();
            _layoutReferences = CreateLayoutReferenceLazies();
            _requestRedraw = UnsafeHelper.GetMaxValue<nuint>();
        }

        private LazyTiny<LayoutVariable>[] CreateLayoutReferenceLazies()
        {
            LazyTiny<LayoutVariable>[] result = new LazyTiny<LayoutVariable>[(int)LayoutProperty._Last];
            for (int i = 0; i < (int)LayoutProperty._Last; i++)
            {
                LayoutProperty prop = (LayoutProperty)i;
                result[i] = new LazyTiny<LayoutVariable>(() => new UIElementLayoutVariable(this, prop), LazyThreadSafetyMode.PublicationOnly);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable GetLayoutReference(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return GetLayoutReferenceCore(property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable? GetLayoutVariable(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return GetLayoutVariableCore(property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLayoutVariable(LayoutProperty property, LayoutVariable? variable)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            SetLayoutVariableCore(property, variable);
        }

        [Inline(InlineBehavior.Remove)]
        public LayoutVariable GetLayoutReferenceCore(LayoutProperty property)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutReferences), (nuint)property).Value;

        [Inline(InlineBehavior.Remove)]
        public LayoutVariable? GetLayoutVariableCore(LayoutProperty property)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutVariables), (nuint)property);

        [Inline(InlineBehavior.Remove)]
        public void SetLayoutVariableCore(LayoutProperty property, LayoutVariable? variable)
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutVariables), (nuint)property) = variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBackgroundOpaque() => IsBackgroundOpaqueCore() || (_parent ?? _renderer).IsBackgroundOpaque(this);

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
            if (InterlockedHelper.Read(ref _freezeCount) != 0U ||
                !CheckIsRenderedOnce(InterlockedHelper.Or(ref _requestRedraw, RequestRedrawBit)))
                return;
            UpdateCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateCore() => _renderer.GetRenderingController()?.RequestUpdate(false);

        public virtual void Render(in RegionalRenderingContext context) => Render(in context, markDirty: true);

        public void Render(in RegionalRenderingContext context, bool markDirty)
        {
            SemaphoreSlim semaphore = _semaphore;
            semaphore.Wait();
            try
            {
                ResetNeedRefreshFlag();
                if (!RenderCore(in context))
                    Update();
            }
            finally
            {
                semaphore.Release();
                if (markDirty)
                    context.MarkAsDirty();
            }
        }

        protected void RenderBackground(in RegionalRenderingContext context)
        {
            IElementContainer parent = InterlockedHelper.Read(ref _parent) ?? _renderer;
            parent.RenderBackground(this, in context);
        }

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
            SemaphoreSlim semaphore = _semaphore;
            semaphore.Wait();
            try
            {
                ApplyThemeCore(provider);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
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
            IThemeResourceProvider? provider = Renderer.GetThemeResourceProvider();
            if (provider is not null)
                ApplyThemeCore(provider);
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyThemeContextCore(IThemeContext themeContext)
        {
            IRenderer renderer = Renderer;
            SemaphoreSlim semaphore = _semaphore;
            semaphore.Wait();
            IThemeResourceProvider provider = ThemeResourceProvider.CreateResourceProvider(renderer.GetDeviceContext(), themeContext,
                (renderer as CoreWindow)?.WindowMaterial ?? WindowMaterial.Default);
            try
            {
                ApplyThemeCore(provider);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                semaphore.Release();
                (provider as IDisposable)?.Dispose();
            }
            Update();
        }

        protected abstract void ApplyThemeCore(IThemeResourceProvider provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToGlobal(Point point) => (_parent ?? _renderer).PointToGlobal(this, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToGlobal(PointF point) => (_parent ?? _renderer).PointToGlobal(this, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointToLocal(Point point) => (_parent ?? _renderer).PointToLocal(this, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF PointToLocal(PointF point) => (_parent ?? _renderer).PointToLocal(this, point);

        public override int GetHashCode() => _identifier;

        public override bool Equals(object? obj) => ReferenceEquals(obj, this);

        [Inline(InlineBehavior.Remove)]
        private Point GetLocationCore()
        {
            ref readonly ulong valRef = ref _location;
            ref readonly nuint versionRef = ref _version;
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
                OptimisticLock.Increase(ref _version);
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
            ref readonly nuint versionRef = ref _version;
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
                OptimisticLock.Increase(ref _version);
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
