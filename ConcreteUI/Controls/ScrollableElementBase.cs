using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Theme;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public abstract partial class ScrollableElementBase : DisposableUIElementBase, IMouseInteractEvents, IMouseScrollEvent
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
        private Rect _contentBounds, _scrollBarBounds;
        private RectF _scrollBarScrollButtonBounds, _scrollBarUpButtonBounds, _scrollBarDownButtonBounds;
        private ButtonTriState _scrollButtonState, _scrollUpButtonState, _scrollDownButtonState;
        private ScrollBarType _scrollBarType;
        private ulong _updateFlagsRaw;
        private float _pinY;
        private bool _enabled, _drawWhenDisabled, _hasScrollBar, _stickBottom;

        protected ScrollableElementBase(IRenderer renderer, string themePrefix) : this(renderer, themePrefix, DefaultPrefixForScrollBar) { }

        protected ScrollableElementBase(IRenderer renderer, string themePrefix, string scrollBarThemePrefix) : base(renderer, themePrefix)
        {
            _enabled = true;
            _drawWhenDisabled = false;
            _hasScrollBar = false;
            _updateFlagsRaw = (ulong)ScrollableElementUpdateFlags._NormalFlagAllTrue;
            _oldSurfaceSize = Size.Empty;
            _repeatingTimer = new Timer(RepeatingTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
            _scrollBarThemePrefix = scrollBarThemePrefix.ToLowerAscii();
        }

        protected abstract D2D1Brush GetBackBrush();

        protected abstract D2D1Brush GetBackDisabledBrush();

        protected virtual D2D1Brush? GetBorderBrush() => null;

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, _scrollBarThemePrefix, (int)Brush._Last);

        protected override void Update() => Update(ScrollableElementUpdateFlags.Content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Update(ScrollableElementUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _updateFlagsRaw, (ulong)flags);
            UpdateCore();
        }

        public override bool NeedRefresh()
        {
            if (_updateFlagsRaw != (long)ScrollableElementUpdateFlags.None)
                return true;
            return InterlockedHelper.Read(ref _updateFlagsRaw) != (ulong)ScrollableElementUpdateFlags.None;
        }

        [Inline(InlineBehavior.Remove)]
        private ScrollableElementUpdateFlags GetUpdateFlagsAndReset()
            => (ScrollableElementUpdateFlags)InterlockedHelper.Exchange(ref _updateFlagsRaw, (ulong)ScrollableElementUpdateFlags.None);

        protected abstract bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush);

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

        public override void Render(in RegionalRenderingContext context) => Render(context, markDirty: false);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            ScrollableElementUpdateFlags updateFlags = GetUpdateFlagsAndReset();
            if (!context.HasDirtyCollector)
                updateFlags |= ScrollableElementUpdateFlags.All;
            else if (updateFlags == ScrollableElementUpdateFlags.None)
                return true;

            Rect bounds = Bounds;
            D2D1Brush[] brushes = _brushes;
            bool enabled = _enabled;
            bool drawWhenDisabled = _drawWhenDisabled;

            if ((updateFlags & ScrollableElementUpdateFlags.RecalcLayout) == ScrollableElementUpdateFlags.RecalcLayout)
                updateFlags = (updateFlags & ~ScrollableElementUpdateFlags.RecalcLayout) | RecalculateLayout(bounds);

            bool hasScrollBar = _hasScrollBar;
            bool triggerViewportPointChanged = (updateFlags & ScrollableElementUpdateFlags.TriggerViewportPointChanged) == ScrollableElementUpdateFlags.TriggerViewportPointChanged;
            bool recalcScrollBar = (updateFlags & ScrollableElementUpdateFlags.RecalcScrollBar) == ScrollableElementUpdateFlags.RecalcScrollBar;
            bool redrawAll = (updateFlags & ScrollableElementUpdateFlags.All) == ScrollableElementUpdateFlags.All;
            bool redrawScrollBar = (updateFlags & ScrollableElementUpdateFlags.ScrollBar) == ScrollableElementUpdateFlags.ScrollBar;
            bool redrawContent = (updateFlags & ScrollableElementUpdateFlags.Content) == ScrollableElementUpdateFlags.Content;
            bool redrawContentResult = false;

            if (triggerViewportPointChanged)
                OnViewportPointChanged();
            if (redrawAll)
            {
                RenderBackground(context, enabled ? GetBackBrush() : GetBackDisabledBrush());
                context.MarkAsDirty();
            }
            if (redrawContent)
            {
                Rect contentBoundsRaw = _contentBounds;
                RectF contentBounds = new RectF(contentBoundsRaw.Left - bounds.Left, contentBoundsRaw.Top - bounds.Top,
                    contentBoundsRaw.Right - bounds.Left, contentBoundsRaw.Bottom - bounds.Top);
                if (contentBounds.IsValid)
                {
                    using RegionalRenderingContext clippedContext = context.WithPixelAlignedClip(ref contentBounds, D2D1AntialiasMode.Aliased);
                    if (enabled || drawWhenDisabled)
                    {
                        redrawContentResult = !RenderContent(
                            redrawAll ? clippedContext.WithEmptyDirtyCollector() : clippedContext,
                            enabled ? GetBackBrush() : GetBackDisabledBrush());
                    }
                    else if (!redrawAll)
                    {
                        RenderBackground(clippedContext, GetBackDisabledBrush());
                        clippedContext.MarkAsDirty();
                    }
                }
            }
            if (hasScrollBar && redrawScrollBar)
            {
                if (recalcScrollBar)
                    RecalculateScrollBarButton();
                Rect scrollBarBoundsRaw = _scrollBarBounds;
                RectF scrollButtonBoundsRaw = _scrollBarScrollButtonBounds;
                RectF upButtonBoundsRaw = _scrollBarUpButtonBounds;
                RectF downButtonBoundsRaw = _scrollBarDownButtonBounds;

                RectF scrollBarBounds = new RectF(scrollBarBoundsRaw.Left - bounds.Left, scrollBarBoundsRaw.Top - bounds.Top,
                    scrollBarBoundsRaw.Right - bounds.Left, scrollBarBoundsRaw.Bottom - bounds.Top);
                RectF scrollButtonBounds = new RectF(scrollButtonBoundsRaw.Left - bounds.Left, scrollButtonBoundsRaw.Top - bounds.Top,
                    scrollButtonBoundsRaw.Right - bounds.Left, scrollButtonBoundsRaw.Bottom - bounds.Top);
                RectF upButtonBounds = new RectF(upButtonBoundsRaw.Left - bounds.Left, upButtonBoundsRaw.Top - bounds.Top,
                    upButtonBoundsRaw.Right - bounds.Left, upButtonBoundsRaw.Bottom - bounds.Top);
                RectF downButtonBounds = new RectF(downButtonBoundsRaw.Left - bounds.Left, downButtonBoundsRaw.Top - bounds.Top,
                    downButtonBoundsRaw.Right - bounds.Left, downButtonBoundsRaw.Bottom - bounds.Top);

                if (scrollBarBounds.IsValid && scrollButtonBounds.IsValid && upButtonBounds.IsValid && downButtonBounds.IsValid)
                {
                    Vector2 pointsPerPixel = context.PointsPerPixel;
                    scrollBarBounds = RenderingHelper.RoundInPixel(scrollBarBounds, pointsPerPixel);
                    scrollButtonBounds = RenderingHelper.RoundInPixel(scrollButtonBounds, pointsPerPixel);
                    upButtonBounds = RenderingHelper.RoundInPixel(upButtonBounds, pointsPerPixel);
                    downButtonBounds = RenderingHelper.RoundInPixel(downButtonBounds, pointsPerPixel);

                    using RenderingClipToken clipToken = context.PushAxisAlignedClip(scrollBarBounds, D2D1AntialiasMode.Aliased);
                    RenderBackground(context, brushes[(int)Brush.ScrollBarBackBrush]);

                    D2D1DeviceContext deviceContext = context.DeviceContext;
                    (D2D1AntialiasMode antialiasModeBefore, deviceContext.AntialiasMode) = (deviceContext.AntialiasMode, D2D1AntialiasMode.PerPrimitive);
                    try
                    {
                        float gap = RenderingHelper.CeilingInPixel((UIConstantsPrivate.ScrollBarWidth - UIConstantsPrivate.ScrollBarScrollButtonWidth) * 0.5f, pointsPerPixel.X);
                        context.FillRoundedRectangle(new D2D1RoundedRectangle()
                        {
                            RadiusX = 3,
                            RadiusY = 3,
                            Rect = new RectF(scrollButtonBounds.Left + gap, scrollButtonBounds.Top, scrollButtonBounds.Right - gap, scrollButtonBounds.Bottom)
                        }, GetButtonStateBrush(_scrollButtonState));
                        FontIconResources resources = FontIconResources.Instance;
                        resources.DrawScrollBarUpButton(context, (RectangleF)upButtonBounds, GetButtonStateBrush(_scrollUpButtonState));
                        resources.DrawScrollBarDownButton(context, (RectangleF)downButtonBounds, GetButtonStateBrush(_scrollDownButtonState));
                    }
                    finally
                    {
                        deviceContext.AntialiasMode = antialiasModeBefore;
                    }
                    if (!redrawAll)
                        context.MarkAsDirty(scrollBarBounds);
                }
            }
            if (redrawContent || hasScrollBar && redrawScrollBar)
            {
                D2D1Brush? borderBrush = GetBorderBrush();
                if (borderBrush is not null)
                    context.DrawBorder(borderBrush);
            }
            if (redrawContentResult)
            {
                InterlockedHelper.Or(ref _updateFlagsRaw, (long)ScrollableElementUpdateFlags.Content);
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

        public override void OnSizeChanged() => Update(ScrollableElementUpdateFlags.RecalcLayout);

        public override void OnLocationChanged() => Update(ScrollableElementUpdateFlags.RecalcLayout);

        public virtual void OnViewportPointChanged() { }

        private ScrollableElementUpdateFlags RecalculateLayout(in Rect bounds)
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

            ScrollableElementUpdateFlags result = hasScrollBar ? (ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.All) : ScrollableElementUpdateFlags.Content;
            if (_viewportPoint != viewportPoint)
            {
                _viewportPoint = viewportPoint;
                result |= ScrollableElementUpdateFlags.TriggerViewportPointChanged;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateScrollBarButton()
        {
            Rect bounds = Bounds;
            Rect contentBounds = _contentBounds;
            Rect scrollBarBounds = new Rect(contentBounds.Right, contentBounds.Top, contentBounds.Right + UIConstantsPrivate.ScrollBarWidth, contentBounds.Bottom);
            int baseX = scrollBarBounds.X;
            _scrollBarUpButtonBounds = RectF.FromXYWH(baseX, scrollBarBounds.Y, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            _scrollBarDownButtonBounds = RectF.FromXYWH(baseX, scrollBarBounds.Bottom - UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            float scrollBarMaxHeight = _scrollBarDownButtonBounds.Top - _scrollBarUpButtonBounds.Bottom;
            int surfaceHeight = _surfaceSize.Height;
            if (surfaceHeight == 0) surfaceHeight = 1;
            double surfaceHeightPerBarHeight = scrollBarMaxHeight < surfaceHeight ? scrollBarMaxHeight * 1.0 / surfaceHeight : 1.0;
            float height = MathHelper.Max((float)(bounds.Height * surfaceHeightPerBarHeight), 10.0f);
            surfaceHeightPerBarHeight = (scrollBarMaxHeight - height) * 1.0 / (surfaceHeight - bounds.Height);
            float Y = _scrollBarUpButtonBounds.Bottom + (float)(_viewportPoint.Y * surfaceHeightPerBarHeight);
            _scrollBarScrollButtonBounds = RectF.FromXYWH(baseX, Y, UIConstantsPrivate.ScrollBarWidth, height);
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
            Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScrollToEnd()
        {
            _viewportPoint.Y = MathHelper.Max(_surfaceSize.Height - _contentBounds.Height, 0);
            Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
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
                Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
            }
        }

        public virtual void OnMouseScroll(ref MouseInteractEventArgs args)
        {
            if (!_enabled || !_hasScrollBar || _scrollButtonState == ButtonTriState.Pressed)
                return;
            args.Handle();
            Scrolling(-args.Delta);
            OnMouseMove(args);
        }

        private void RepeatingTimer_Tick(object? state) => _repeatingAction?.Invoke();

        public virtual void OnMouseDown(ref MouseInteractEventArgs args)
        {
            if (!_enabled || !_hasScrollBar || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            if (_scrollBarScrollButtonBounds.Contains(args.Location))
            {
                args.Handle();

                _scrollButtonState = ButtonTriState.Pressed;
                _pinY = args.Y;
                Update(ScrollableElementUpdateFlags.ScrollBar);
                return;
            }

            if (_scrollBarUpButtonBounds.Contains(args.Location))
            {
                args.Handle();

                _scrollUpButtonState = ButtonTriState.Pressed;
                Update(ScrollableElementUpdateFlags.ScrollBar);
                OnScrollBarUpButtonClicked();
                _repeatingAction = OnScrollBarUpButtonClicked;
                _repeatingTimer.Change(SystemParameters.KeyboardDelay, SystemParameters.KeyboardSpeed);
                return;
            }

            if (_scrollBarDownButtonBounds.Contains(args.Location))
            {
                args.Handle();

                _scrollDownButtonState = ButtonTriState.Pressed;
                Update(ScrollableElementUpdateFlags.ScrollBar);
                OnScrollBarDownButtonClicked();
                _repeatingAction = OnScrollBarDownButtonClicked;
                _repeatingTimer.Change(SystemParameters.KeyboardDelay, SystemParameters.KeyboardSpeed);
                return;
            }
        }

        public virtual void OnMouseUp(in MouseNotifyEventArgs args)
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
                    Update(ScrollableElementUpdateFlags.ScrollBar);
                if (_repeatingTimer is not null)
                {
                    _repeatingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _repeatingAction = null;
                }
            }
        }

        public virtual void OnMouseMove(in MouseNotifyEventArgs args)
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
                    Update(ScrollableElementUpdateFlags.ScrollBar);
            }
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
                DisposeHelper.DisposeAll(_brushes);
            _repeatingTimer.Dispose();
            SequenceHelper.Clear(_brushes);
        }
    }
}
