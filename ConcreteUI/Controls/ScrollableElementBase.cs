using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public abstract partial class ScrollableElementBase : UIElement, IDisposable, IMouseEvents, IMouseScrollEvent
    {
        protected const string DefaultPrefixForScrollBar = "app.scrollBar";

        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "fore",
            "fore.hovered",
            "fore.pressed",
        };

        private readonly Timer _repeatingTimer;
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private string _scrollBarThemePrefix;
        private Action? _repeatingAction;
        private Point _viewportPoint;
        private Size _surfaceSize, _oldSurfaceSize;
        private Rect _contentBounds, _scrollBarBounds, _scrollBarScrollButtonBounds, _scrollBarUpButtonBounds, _scrollBarDownButtonBounds;
        private ButtonTriState _scrollButtonState, _scrollUpButtonState, _scrollDownButtonState;
        private ScrollBarType _scrollBarType;
        private ulong _updateFlagsRaw;
        private float _pinY;
        private bool _enabled, _drawWhenDisabled, _hasScrollBar, _stickBottom, _disposed;

        protected ScrollableElementBase(IRenderer renderer, string themePrefix) : this(renderer, themePrefix, DefaultPrefixForScrollBar) { }

        protected ScrollableElementBase(IRenderer renderer, string themePrefix, string scrollBarThemePrefix) : base(renderer, themePrefix)
        {
            _enabled = true;
            _drawWhenDisabled = false;
            _hasScrollBar = false;
            _disposed = false;
            _updateFlagsRaw = (ulong)UpdateFlags._NormalFlagAllTrue;
            _oldSurfaceSize = Size.Empty;
            _repeatingTimer = new Timer(RepeatingTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
            _scrollBarThemePrefix = scrollBarThemePrefix.ToLowerAscii();
        }

        protected abstract D2D1Brush GetBackBrush();

        protected abstract D2D1Brush GetBackDisabledBrush();

        protected virtual D2D1Brush? GetBorderBrush() => null;

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, _scrollBarThemePrefix, (int)Brush._Last);

        protected override void Update() => Update(UpdateFlags.All);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update(UpdateFlags flags)
        {
            if (flags == UpdateFlags.None)
                return;
            InterlockedHelper.Or(ref _updateFlagsRaw, (ulong)flags);
            UpdateCore();
        }

        public override bool NeedRefresh()
        {
            if (_updateFlagsRaw != (long)UpdateFlags.None)
                return true;
            return InterlockedHelper.Read(ref _updateFlagsRaw) != (ulong)UpdateFlags.None;
        }

        [Inline(InlineBehavior.Remove)]
        private UpdateFlags GetUpdateFlagsAndReset()
            => (UpdateFlags)InterlockedHelper.Exchange(ref _updateFlagsRaw, (ulong)UpdateFlags.None);

        protected abstract bool RenderContent(DirtyAreaCollector collector);

        protected virtual void OnScrollBarUpButtonClicked() => Scrolling(-60);

        protected virtual void OnScrollBarDownButtonClicked() => Scrolling(60);

        protected virtual Rect OnContentBoundsChanging(in Rect bounds) => bounds;

        protected virtual void OnContentBoundsChanged() { }

        protected virtual void OnEnableChanged(bool enable) { }

        public Rect ScrollBarBounds()
        {
            if (_hasScrollBar)
            {
                return _scrollBarBounds;
            }
            else
            {
                return default;
            }
        }

        public override void Render(DirtyAreaCollector collector) => Render(collector, markDirty: false);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            UpdateFlags updateFlags = GetUpdateFlagsAndReset();
            if (collector.IsEmptyInstance)
                updateFlags |= UpdateFlags.All;
            else if (updateFlags == UpdateFlags.None)
                return true;

            D2D1DeviceContext context = Renderer.GetDeviceContext();
            Rect bounds = Bounds;
            D2D1Brush[] brushes = _brushes;
            bool enabled = _enabled;
            bool drawWhenDisabled = _drawWhenDisabled;

            if ((updateFlags & UpdateFlags.RecalcLayout) == UpdateFlags.RecalcLayout)
                updateFlags = (updateFlags & ~UpdateFlags.RecalcLayout) | RecalculateLayout(bounds);

            Rect contentBounds = _contentBounds;
            bool hasScrollBar = _hasScrollBar;
            bool triggerViewportPointChanged = (updateFlags & UpdateFlags.TriggerViewportPointChanged) == UpdateFlags.TriggerViewportPointChanged;
            bool recalcScrollBar = (updateFlags & UpdateFlags.RecalcScrollBar) == UpdateFlags.RecalcScrollBar;
            bool redrawAll = (updateFlags & UpdateFlags.All) == UpdateFlags.All;
            bool redrawScrollBar = (updateFlags & UpdateFlags.ScrollBar) == UpdateFlags.ScrollBar;
            bool redrawContent = (updateFlags & UpdateFlags.Content) == UpdateFlags.Content;
            bool redrawContentResult = false;
            if (redrawContent)
            {
                if (enabled || drawWhenDisabled)
                {
                    RenderBackground(context, enabled ? GetBackBrush() : GetBackDisabledBrush());
                    if (contentBounds.IsValid)
                    {
                        context.PushAxisAlignedClip((RectF)contentBounds, D2D1AntialiasMode.Aliased);
                        redrawContentResult = !RenderContent(redrawAll ? DirtyAreaCollector.Empty : collector);
                        context.PopAxisAlignedClip();
                    }
                }
                else
                {
                    RenderBackground(context, GetBackDisabledBrush());
                    if (!redrawAll && contentBounds.IsValid)
                        collector.MarkAsDirty(contentBounds);
                }
            }
            if (hasScrollBar && redrawScrollBar)
            {
                if (recalcScrollBar)
                    RecalculateScrollBarButton();
                Rect scrollBarBounds = _scrollBarBounds;
                context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
                context.PushAxisAlignedClip((RectF)scrollBarBounds, D2D1AntialiasMode.Aliased);
                RenderBackground(context, brushes[(int)Brush.ScrollBarBackBrush]);
                context.FillRoundedRectangle(new D2D1RoundedRectangle() { RadiusX = 3, RadiusY = 3, Rect = (RectF)_scrollBarScrollButtonBounds },
                    GetButtonStateBrush(_scrollButtonState));
                FontIconResources resources = FontIconResources.Instance;
                resources.DrawScrollBarUpButton(context, (RectangleF)_scrollBarUpButtonBounds, GetButtonStateBrush(_scrollUpButtonState));
                resources.DrawScrollBarDownButton(context, (RectangleF)_scrollBarDownButtonBounds, GetButtonStateBrush(_scrollDownButtonState));
                context.PopAxisAlignedClip();
                context.AntialiasMode = D2D1AntialiasMode.Aliased;
                if (!redrawAll)
                    collector.MarkAsDirty(scrollBarBounds);
            }
            if (redrawContent || hasScrollBar && redrawScrollBar)
            {
                D2D1Brush? borderBrush = GetBorderBrush();
                if (borderBrush is not null)
                {
                    context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
                    float lineWidth = Renderer.GetBaseLineWidth();
                    context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(bounds, lineWidth), borderBrush, lineWidth);
                    context.PopAxisAlignedClip();
                }
            }
            if (redrawAll)
                collector.MarkAsDirty(bounds);
            if (triggerViewportPointChanged)
                OnViewportPointChanged();
            if (redrawContentResult)
            {
                InterlockedHelper.Or(ref _updateFlagsRaw, (long)UpdateFlags.Content);
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private D2D1Brush GetButtonStateBrush(ButtonTriState state)
        {
            D2D1Brush[] brushes = _brushes;
            return state switch
            {
                ButtonTriState.None => brushes[(int)Brush.ScrollBarForeBrush],
                ButtonTriState.Hovered => brushes[(int)Brush.ScrollBarForeBrushHovered],
                ButtonTriState.Pressed => brushes[(int)Brush.ScrollBarForeBrushPressed],
                _ => brushes[(int)Brush.ScrollBarForeBrush],
            };
        }

        public override void OnSizeChanged() => Update(UpdateFlags.RecalcLayout);

        public override void OnLocationChanged() => Update(UpdateFlags.RecalcLayout);

        public virtual void OnViewportPointChanged() { }

        private UpdateFlags RecalculateLayout(in Rect bounds)
        {
            Rect contentBounds = bounds;
            Size oldSurfaceSize = _oldSurfaceSize;
            Size surfaceSize = _surfaceSize;
            _oldSurfaceSize = surfaceSize;

            bool hasScrollBar = _hasScrollBar;
            switch (_scrollBarType)
            {
                case ScrollBarType.None:
                    hasScrollBar = false;
                    break;
                case ScrollBarType.Vertical:
                    contentBounds.Width -= UIConstantsPrivate.ScrollBarWidth + 1;
                    hasScrollBar = true;
                    break;
                case ScrollBarType.AutoVertial:
                    if (bounds.Height < surfaceSize.Height && _enabled)
                    {
                        goto case ScrollBarType.Vertical;
                    }
                    else
                    {
                        goto case ScrollBarType.None;
                    }
            }
            Point viewportPoint = _viewportPoint;
            bool isStick = StickBottom && (!_hasScrollBar || viewportPoint.Y + _contentBounds.Height >= oldSurfaceSize.Height);
            _hasScrollBar = hasScrollBar;
            contentBounds = OnContentBoundsChanging(contentBounds);
            if (_contentBounds != contentBounds)
            {
                _contentBounds = contentBounds;
                OnContentBoundsChanged();
            }
            int maxX = MathHelper.Max(surfaceSize.Width - contentBounds.Width, 0);
            int maxY = MathHelper.Max(surfaceSize.Height - contentBounds.Height, 0);
            if (isStick)
                viewportPoint = new Point(MathHelper.Clamp(viewportPoint.X, 0, maxX), maxY);
            else
                viewportPoint = new Point(MathHelper.Clamp(viewportPoint.X, 0, maxX), MathHelper.Clamp(viewportPoint.Y, 0, maxY));

            UpdateFlags result = hasScrollBar ? (UpdateFlags.RecalcScrollBar | UpdateFlags.All) : UpdateFlags.Content;
            if (_viewportPoint != viewportPoint)
            {
                _viewportPoint = viewportPoint;
                result |= UpdateFlags.TriggerViewportPointChanged;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateScrollBarButton()
        {
            Rect bounds = Bounds;
            Rect contentBounds = _contentBounds;
            Rect scrollBarBounds = new Rect(contentBounds.Right, contentBounds.Top, contentBounds.Right + UIConstantsPrivate.ScrollBarWidth, contentBounds.Bottom);
            int X = scrollBarBounds.X;
            int X2 = X + (UIConstantsPrivate.ScrollBarWidth - UIConstantsPrivate.ScrollBarScrollButtonWidth) / 2;
            _scrollBarUpButtonBounds = Rect.FromXYWH(X, scrollBarBounds.Y, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            _scrollBarDownButtonBounds = Rect.FromXYWH(X, scrollBarBounds.Bottom - UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            int scrollBarMaxHeight = _scrollBarDownButtonBounds.Top - _scrollBarUpButtonBounds.Bottom;
            int surfaceHeight = _surfaceSize.Height;
            if (surfaceHeight == 0) surfaceHeight = 1;
            double surfaceHeightPerBarHeight = scrollBarMaxHeight < surfaceHeight ? scrollBarMaxHeight * 1.0 / surfaceHeight : 1.0;
            int height = MathHelper.Max(MathI.Ceiling(bounds.Height * surfaceHeightPerBarHeight), 10);
            surfaceHeightPerBarHeight = (scrollBarMaxHeight - height) * 1.0 / (surfaceHeight - bounds.Height);
            int Y = _scrollBarUpButtonBounds.Bottom + MathI.Ceiling(_viewportPoint.Y * surfaceHeightPerBarHeight);
            _scrollBarScrollButtonBounds = Rect.FromXYWH(X2, Y, UIConstantsPrivate.ScrollBarScrollButtonWidth, height);
            _scrollBarBounds = scrollBarBounds;
        }

        public virtual void Scrolling(int scrollStep)
        {
            Point oldPoint = _viewportPoint;
            ViewportPoint = new Point(oldPoint.X, oldPoint.Y + scrollStep);
        }

        public virtual void ScrollingTo(int viewportY) => ViewportPoint = new Point(_viewportPoint.X, viewportY);

        public virtual void ScrollingX(int scrollStep)
        {
            Point oldPoint = _viewportPoint;
            ViewportPoint = new Point(oldPoint.X + scrollStep, oldPoint.Y);
        }

        public virtual void ScrollingXTo(int viewportX) => ViewportPoint = new Point(viewportX, _viewportPoint.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsScrolledToStart() => _viewportPoint.Y <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsScrolledToEnd() => _viewportPoint.Y >= _surfaceSize.Height - _contentBounds.Height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScrollToStart()
        {
            _viewportPoint.Y = 0;
            Update(UpdateFlags.RecalcScrollBar | UpdateFlags.All);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScrollToEnd()
        {
            _viewportPoint.Y = MathHelper.Max(_surfaceSize.Height - _contentBounds.Height, 0);
            Update(UpdateFlags.RecalcScrollBar | UpdateFlags.All);
        }

        private void MoveScrollBarButtonY(int movementY)
        {
            if (IsScrolledToStart() && movementY < 0 || IsScrolledToEnd() && movementY > 0)
                return;
            if (movementY != 0)
            {
                int scrollBarMaxHeight = _scrollBarBounds.Height - 4;
                int surfaceHeight = _surfaceSize.Height;
                int maxY = surfaceHeight - Bounds.Height;
                int viewPortY = _viewportPoint.Y + MathI.Ceiling(movementY * 1.0 * surfaceHeight / scrollBarMaxHeight);
                if (float.IsNaN(viewPortY) || viewPortY > maxY)
                {
                    viewPortY = maxY;
                }
                else if (viewPortY < 0)
                {
                    viewPortY = 0;
                }
                _viewportPoint.Y = viewPortY;
                Update(UpdateFlags.RecalcScrollBar | UpdateFlags.All);
            }
        }

        public virtual void OnMouseScroll(in MouseInteractEventArgs args)
        {
            if (_enabled && _hasScrollBar && _scrollButtonState != ButtonTriState.Pressed && Bounds.Contains(args.Location))
            {
                Scrolling(-args.Delta);
                OnMouseMove(args);
            }
        }

        private void RepeatingTimer_Tick(object? state) => _repeatingAction?.Invoke();

        public virtual void OnMouseDown(in MouseInteractEventArgs args)
        {
            if (_enabled && _hasScrollBar)
            {
                if (_scrollBarScrollButtonBounds.Contains(args.Location))
                {
                    _scrollButtonState = ButtonTriState.Pressed;
                    _pinY = args.Y;
                    Update(UpdateFlags.ScrollBar);
                }
                else if (_scrollBarUpButtonBounds.Contains(args.Location))
                {
                    _scrollUpButtonState = ButtonTriState.Pressed;
                    Update(UpdateFlags.ScrollBar);
                    OnScrollBarUpButtonClicked();
                    _repeatingAction = OnScrollBarUpButtonClicked;
                    _repeatingTimer.Change(SystemParameters.KeyboardDelay, SystemParameters.KeyboardSpeed);
                }
                else if (_scrollBarDownButtonBounds.Contains(args.Location))
                {
                    _scrollDownButtonState = ButtonTriState.Pressed;
                    Update(UpdateFlags.ScrollBar);
                    OnScrollBarDownButtonClicked();
                    _repeatingAction = OnScrollBarDownButtonClicked;
                    _repeatingTimer.Change(SystemParameters.KeyboardDelay, SystemParameters.KeyboardSpeed);
                }
            }
        }

        public virtual void OnMouseUp(in MouseInteractEventArgs args)
        {
            if (_enabled && _hasScrollBar)
            {
                bool updateScrollBar = false;
                if (_scrollButtonState == ButtonTriState.Pressed)
                {
                    _scrollButtonState = _scrollBarScrollButtonBounds.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;
                    updateScrollBar = true;
                }
                if (_scrollUpButtonState == ButtonTriState.Pressed)
                {
                    _scrollUpButtonState = _scrollBarUpButtonBounds.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;
                    updateScrollBar = true;
                }
                if (_scrollDownButtonState == ButtonTriState.Pressed)
                {
                    _scrollDownButtonState = _scrollBarDownButtonBounds.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;
                    updateScrollBar = true;
                }
                if (updateScrollBar)
                    Update(UpdateFlags.ScrollBar);
                if (_repeatingTimer is not null)
                {
                    _repeatingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _repeatingAction = null;
                }
            }
        }

        public virtual void OnMouseMove(in MouseInteractEventArgs args)
        {
            if (!_enabled) return;
            if (_hasScrollBar)
            {
                bool updateScrollBar = false;
                if (_scrollButtonState == ButtonTriState.Pressed)
                {
                    MoveScrollBarButtonY(MathI.Ceiling(args.Y - _pinY));
                    _pinY = args.Y;
                }
                else
                {
                    ButtonTriState newState;
                    if (_scrollBarScrollButtonBounds.Contains(args.Location))
                    {
                        newState = ButtonTriState.Hovered;
                    }
                    else
                    {
                        newState = ButtonTriState.None;
                    }
                    if (_scrollButtonState != newState)
                    {
                        _scrollButtonState = newState;
                        updateScrollBar = true;
                    }
                }
                if (_scrollUpButtonState != ButtonTriState.Pressed)
                {
                    ButtonTriState newState;
                    if (_scrollBarUpButtonBounds.Contains(args.Location))
                    {
                        newState = ButtonTriState.Hovered;
                    }
                    else
                    {
                        newState = ButtonTriState.None;
                    }
                    if (_scrollUpButtonState != newState)
                    {
                        _scrollUpButtonState = newState;
                        updateScrollBar = true;
                    }
                }
                if (_scrollDownButtonState != ButtonTriState.Pressed)
                {
                    ButtonTriState newState;
                    if (_scrollBarDownButtonBounds.Contains(args.Location))
                    {
                        newState = ButtonTriState.Hovered;
                    }
                    else
                    {
                        newState = ButtonTriState.None;
                    }
                    if (_scrollDownButtonState != newState)
                    {
                        _scrollDownButtonState = newState;
                        updateScrollBar = true;
                    }
                }
                if (updateScrollBar)
                    Update(UpdateFlags.ScrollBar);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeCore(disposing);
        }

        protected virtual void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.DisposeAll(_brushes);
            }
            _repeatingTimer.Dispose();
            SequenceHelper.Clear(_brushes);
        }

        ~ScrollableElementBase()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
