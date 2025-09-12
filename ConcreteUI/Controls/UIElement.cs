using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Layout;
using ConcreteUI.Layout.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;
using WitherTorch.Common.Windows.Structures;

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

        private IContainerElement? _parent;
        private IThemeContext? _themeContext;
        private Rectangle _bounds;
        private string _themePrefix;
        private long _requestRedraw;

        public UIElement(IRenderer renderer, string themePrefix)
        {
            _renderer = renderer;
            _semaphore = new SemaphoreSlim(1, 1);
            _identifier = InterlockedHelper.GetAndIncrement(ref _identifierGenerator);
            _themePrefix = themePrefix.ToLowerAscii();
            _layoutReferences = CreateLayoutReferenceLazies();
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
            return UnsafeHelper.AddTypedOffset(ref _layoutReferences[0], (nuint)property).Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable? GetLayoutVariable(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return UnsafeHelper.AddTypedOffset(ref _layoutVariables[0], (nuint)property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLayoutVariable(LayoutProperty property, LayoutVariable? variable)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            UnsafeHelper.AddTypedOffset(ref _layoutVariables[0], (nuint)property) = variable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Update()
        {
            Interlocked.Exchange(ref _requestRedraw, Booleans.TrueLong);
            UpdateCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateCore()
        {
            IRenderer renderer = Renderer;
            if (renderer.IsInitializingElements())
                return;
            renderer.GetRenderingController()?.RequestUpdate(false);
        }

        public virtual void Render(DirtyAreaCollector collector) => Render(collector, markDirty: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Render(DirtyAreaCollector collector, bool markDirty)
        {
            SemaphoreSlim semaphore = _semaphore;
            semaphore.Wait();
            ResetNeedRefreshFlag();
            Rect bounds = Bounds;
            if (!bounds.IsValid)
            {
                semaphore.Release();
                return;
            }
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
            bool result;
            try
            {
                result = RenderCore(collector);
            }
            finally
            {
                context.PopAxisAlignedClip();
                if (markDirty)
                    collector.MarkAsDirty(bounds);
                semaphore.Release();
            }
            if (!result)
                Update();
        }

        protected void RenderBackground(D2D1DeviceContext context)
        {
            IContainerElement? parent = _parent;
            if (parent is null)
            {
                _renderer.RenderElementBackground(this, context);
                return;
            }
            parent.RenderChildBackground(this, context);
        }

        protected void RenderBackground(D2D1DeviceContext context, D2D1Brush backBrush)
        {
            if (backBrush is D2D1SolidColorBrush solidColorBrush)
            {
                if (GraphicsUtils.CheckBrushIsSolid(solidColorBrush))
                {
                    context.Clear(solidColorBrush.Color);
                    return;
                }
                RenderBackground(context);
                context.FillRectangle((RectF)Bounds, backBrush);
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
            context.FillRectangle((RectF)Bounds, backBrush);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool NeedRefresh()
        {
            if (_requestRedraw.IsTrue())
                return true;
            return Interlocked.Read(ref _requestRedraw).IsTrue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ResetNeedRefreshFlag() => Interlocked.Exchange(ref _requestRedraw, Booleans.FalseLong);

        protected abstract bool RenderCore(DirtyAreaCollector collector);

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

        public override int GetHashCode() => _identifier;

        public override bool Equals(object? obj) => ReferenceEquals(obj, this);
    }
}
