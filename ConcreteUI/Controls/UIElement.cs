using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public abstract partial class UIElement
    {
        public delegate void MouseInteractEventHandler(UIElement sender, in MouseInteractEventArgs args);

        private static int _identifierGenerator = 0;

        private readonly AbstractCalculation[] _calculations = new AbstractCalculation[(int)LayoutProperty._Last];
        private readonly IRenderer _renderer;
        private readonly int _identifier;

        private IContainerElement _parent;
        private IThemeContext _themeContext;
        private Rectangle _bounds;
        private long _requestRedraw;

        public UIElement(IRenderer renderer)
        {
            _renderer = renderer;
            _identifier = Interlocked.Increment(ref _identifierGenerator) - 1;
        }

        public AbstractCalculation GetLayoutCalculation(LayoutProperty property)
            => _calculations[(int)property];

        public void SetLayoutCalculation(LayoutProperty property, AbstractCalculation calculation)
            => _calculations[(int)property] = calculation;

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
            ResetNeedRefreshFlag();
            Rect bounds = Bounds;
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
            bool result = RenderCore(collector);
            context.PopAxisAlignedClip();
            if (markDirty)
                collector.MarkAsDirty(bounds);
            if (result)
                return;
            Update();
        }

        protected void RenderBackground(D2D1DeviceContext context)
        {
            IContainerElement parent = _parent;
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
        public void ApplyTheme(ThemeResourceProvider provider)
        {
            if (_themeContext is not null)
                return;
            ApplyThemeCore(provider);
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyThemeContext(IThemeContext themeContext)
        {
            IRenderer renderer = Renderer;
            using ThemeResourceProvider provider = new ThemeResourceProvider(renderer.GetDeviceContext(), themeContext,
                (renderer as CoreWindow)?.WindowMaterial ?? WindowMaterial.None);
            ApplyThemeCore(provider);
            Update();
        }

        protected abstract void ApplyThemeCore(ThemeResourceProvider provider);

        public override int GetHashCode() => _identifier;

        public override bool Equals(object obj) => ReferenceEquals(obj, this);
    }
}
