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
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    public abstract partial class ScrollableElementBase : DisposableUIElementBase,
        IMouseInteractHandler, IMouseMoveHandler, IGlobalMouseMoveHandler, IMouseScrollHandler
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
        private Point _oldViewportPoint;
        private Size _oldSurfaceSize;
        private Rect _scrollBarBounds;
        private RectF _scrollBarScrollButtonBounds, _scrollBarUpButtonBounds, _scrollBarDownButtonBounds;
        private ButtonTriState _scrollButtonState, _scrollUpButtonState, _scrollDownButtonState;
        private ScrollBarType _scrollBarType;
        private ulong _updateFlagsRaw, _contentLocationRaw, _contentSizeRaw, _viewportPointRaw, _surfaceSizeRaw;
        private nuint _surfaceSizeVersion, _viewportPointVersion, _contentBoundsVersion;
        private float _pinY;
        private bool _enabled, _drawWhenDisabled, _hasScrollBar, _stickBottom;

        protected ScrollableElementBase(IElementContainer parent, string themePrefix) : this(parent, themePrefix, DefaultPrefixForScrollBar) { }

        protected ScrollableElementBase(IElementContainer parent, string themePrefix, string scrollBarThemePrefix) : base(parent, themePrefix)
        {
            _enabled = true;
            _drawWhenDisabled = false;
            _hasScrollBar = false;
            _updateFlagsRaw = (ulong)ScrollableElementUpdateFlags._NormalFlagAllTrue;
            _oldSurfaceSize = Size.Empty;
            _repeatingTimer = new Timer(RepeatingTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
            _scrollBarThemePrefix = scrollBarThemePrefix;
        }

        protected abstract D2D1Brush GetBackBrush();

        protected abstract D2D1Brush GetBackDisabledBrush();

        protected virtual D2D1Brush? GetBorderBrush() => null;

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, _scrollBarThemePrefix, (nuint)Brush._Last);

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

        protected virtual void OnContentBoundsChanging(ref Rectangle bounds) { }

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

        protected override bool IsBackgroundOpaqueCore()
        {
            D2D1Brush brush = Enabled ? GetBackBrush() : GetBackDisabledBrush();
            bool result = GraphicsUtils.CheckBrushIsSolid(brush);
            if (!result || !_hasScrollBar)
                return result;
            return GraphicsUtils.CheckBrushIsSolid(UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.ScrollBarBackBrush));
        }

        public override void Render(in RegionalRenderingContext context) => Render(context, markDirty: false);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            ScrollableElementUpdateFlags updateFlags = GetUpdateFlagsAndReset();
            if (!context.HasDirtyCollector)
                updateFlags |= ScrollableElementUpdateFlags.All;
            else if (updateFlags == ScrollableElementUpdateFlags.None)
                return true;

            Rectangle bounds = Bounds, contentBounds;
            Point viewportPoint;
            Size surfaceSize;
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            bool enabled = _enabled;
            bool drawWhenDisabled = _drawWhenDisabled;

            if (updateFlags.HasFlagFast(ScrollableElementUpdateFlags.RecalcLayout))
            {
                updateFlags = (updateFlags & ~ScrollableElementUpdateFlags.RecalcLayout) | RecalculateLayout(bounds, out contentBounds, out surfaceSize, out viewportPoint);
            }
            else
            {
                contentBounds = ContentBounds;
                viewportPoint = ViewportPoint;
                surfaceSize = SurfaceSize;
                if (updateFlags.HasFlagFast(ScrollableElementUpdateFlags.TriggerViewportPointChanged) && !AdjustViewportPoint(surfaceSize, contentBounds.Size, ref viewportPoint))
                    updateFlags &= ~ScrollableElementUpdateFlags.TriggerViewportPointChanged;
            }

            bool hasScrollBar = _hasScrollBar;
            bool triggerViewportPointChanged = updateFlags.HasFlagFast(ScrollableElementUpdateFlags.TriggerViewportPointChanged);
            bool recalcScrollBar = updateFlags.HasFlagFast(ScrollableElementUpdateFlags.RecalcScrollBar);
            bool redrawAll = updateFlags.HasFlagFast(ScrollableElementUpdateFlags.All);
            bool redrawScrollBar = updateFlags.HasFlagFast(ScrollableElementUpdateFlags.ScrollBar);
            bool redrawContent = updateFlags.HasFlagFast(ScrollableElementUpdateFlags.Content);
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
                Size contentSize = contentBounds.Size;
                if (contentSize.Width >= 0 && contentSize.Height >= 0)
                {
                    int x = bounds.X;
                    int y = bounds.Y;
                    RectF clippedBounds = new RectF(contentBounds.Left - x, contentBounds.Top - y, contentBounds.Right - x, contentBounds.Bottom - y);
                    using RegionalRenderingContext clippedContext = context.WithPixelAlignedClip(ref clippedBounds, D2D1AntialiasMode.Aliased);
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
                    RecalculateScrollBarButton(surfaceSize, contentBounds.Size, viewportPoint);
                RectF scrollBarBounds = (RectF)_scrollBarBounds;
                RectF scrollButtonBounds = _scrollBarScrollButtonBounds;
                RectF upButtonBounds = _scrollBarUpButtonBounds;
                RectF downButtonBounds = _scrollBarDownButtonBounds;

                if (scrollBarBounds.IsValid && scrollButtonBounds.IsValid && upButtonBounds.IsValid && downButtonBounds.IsValid)
                {
                    Vector2 pointsPerPixel = context.PixelsPerPoint;
                    scrollBarBounds = RenderingHelper.RoundInPixel(scrollBarBounds, pointsPerPixel);
                    scrollButtonBounds = RenderingHelper.RoundInPixel(scrollButtonBounds, pointsPerPixel);
                    upButtonBounds = RenderingHelper.RoundInPixel(upButtonBounds, pointsPerPixel);
                    downButtonBounds = RenderingHelper.RoundInPixel(downButtonBounds, pointsPerPixel);

                    using RenderingClipScope scope = context.PushAxisAlignedClip(scrollBarBounds, D2D1AntialiasMode.Aliased);
                    RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.ScrollBarBackBrush));

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
            => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), state > ButtonTriState.Pressed ? (nuint)ButtonTriState.None : (nuint)state);

        public override void OnSizeChanged() => Update(ScrollableElementUpdateFlags.RecalcLayout);

        public override void OnLocationChanged() => Update(ScrollableElementUpdateFlags.RecalcLayout);

        private bool AdjustViewportPoint(Size surfaceSize, Size contentSize, ref Point viewportPoint)
        {
            Point originalViewportPoint = viewportPoint;
            if (viewportPoint.X < 0)
                viewportPoint.X = 0;
            else
            {
                int maxX = MathHelper.Max(surfaceSize.Width - contentSize.Width, 0);
                if (viewportPoint.X > maxX)
                    viewportPoint.X = maxX;
            }
            if (viewportPoint.Y < 0)
                viewportPoint.Y = 0;
            else
            {
                int maxY = MathHelper.Max(surfaceSize.Height - contentSize.Height, 0);
                if (viewportPoint.Y > maxY)
                    viewportPoint.Y = maxY;
            }
            if (originalViewportPoint != viewportPoint)
            {
                ulong originalViewportPointAsUInt64 = BoundsHelper.ConvertPointToUInt64(originalViewportPoint);
                if (InterlockedHelper.CompareExchange(ref _viewportPointRaw,
                    BoundsHelper.ConvertPointToUInt64(viewportPoint), originalViewportPointAsUInt64) == originalViewportPointAsUInt64)
                    OptimisticLock.Increase(ref _viewportPointVersion);
            }
            if (_oldViewportPoint == viewportPoint)
                return false;
            _oldViewportPoint = viewportPoint;
            return true;
        }

        public virtual void OnViewportPointChanged() { }

        private ScrollableElementUpdateFlags RecalculateLayout(in Rectangle bounds, out Rectangle contentBounds, out Size surfaceSize, out Point viewportPoint)
        {
            Rectangle oldContentBounds = ContentBounds;
            Size oldSurfaceSize = _oldSurfaceSize;
            viewportPoint = ViewportPoint;
            _oldSurfaceSize = surfaceSize = SurfaceSize;

            bool hasScrollBar = _hasScrollBar;
            contentBounds = bounds;
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
            bool isStick = _stickBottom && (!_hasScrollBar || viewportPoint.Y >= oldSurfaceSize.Height ||
                viewportPoint.Y + oldContentBounds.Height >= oldSurfaceSize.Height);
            _hasScrollBar = hasScrollBar;
            OnContentBoundsChanging(ref contentBounds);
            if (oldContentBounds != contentBounds)
            {
                InterlockedHelper.Write(ref _contentLocationRaw, BoundsHelper.ConvertPointToUInt64(contentBounds.Location));
                InterlockedHelper.Write(ref _contentSizeRaw, BoundsHelper.ConvertSizeToUInt64(contentBounds.Size));
                OptimisticLock.Increase(ref _contentBoundsVersion);
                OnContentBoundsChanged();
            }
            int maxX = MathHelper.Max(surfaceSize.Width - contentBounds.Width, 0);
            int maxY = MathHelper.Max(surfaceSize.Height - contentBounds.Height, 0);
            if (isStick)
                viewportPoint = new Point(MathHelper.Clamp(viewportPoint.X, 0, maxX), maxY);
            else
                viewportPoint = new Point(MathHelper.Clamp(viewportPoint.X, 0, maxX), MathHelper.Clamp(viewportPoint.Y, 0, maxY));

            ScrollableElementUpdateFlags result = hasScrollBar ? (ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.All) : ScrollableElementUpdateFlags.Content;
            ulong viewportPointAsUInt64 = BoundsHelper.ConvertPointToUInt64(viewportPoint);
            if (InterlockedHelper.Exchange(ref _viewportPointRaw, viewportPointAsUInt64) == viewportPointAsUInt64)
                OptimisticLock.Increase(ref _viewportPointVersion);
            if (_oldViewportPoint != viewportPoint)
            {
                _oldViewportPoint = viewportPoint;

                result |= ScrollableElementUpdateFlags.TriggerViewportPointChanged;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateScrollBarButton(Size surfaceSize, Size contentSize, Point viewportPoint)
        {
            Rect scrollBarBounds = Rect.FromXYWH(contentSize.Width, 0, UIConstantsPrivate.ScrollBarWidth, contentSize.Height);
            int baseX = scrollBarBounds.X;
            _scrollBarUpButtonBounds = RectF.FromXYWH(baseX, scrollBarBounds.Y, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            _scrollBarDownButtonBounds = RectF.FromXYWH(baseX, scrollBarBounds.Bottom - UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth, UIConstantsPrivate.ScrollBarWidth);
            float scrollBarMaxHeight = _scrollBarDownButtonBounds.Top - _scrollBarUpButtonBounds.Bottom;
            int surfaceHeight = surfaceSize.Height;
            if (surfaceHeight == 0) surfaceHeight = 1;
            double surfaceHeightPerBarHeight = scrollBarMaxHeight < surfaceHeight ? scrollBarMaxHeight * 1.0 / surfaceHeight : 1.0;
            float height = MathHelper.Max((float)(contentSize.Height * surfaceHeightPerBarHeight), 10.0f);
            surfaceHeightPerBarHeight = (scrollBarMaxHeight - height) * 1.0 / (surfaceHeight - contentSize.Height);
            float Y = _scrollBarUpButtonBounds.Bottom + (float)(viewportPoint.Y * surfaceHeightPerBarHeight);
            _scrollBarScrollButtonBounds = RectF.FromXYWH(baseX, Y, UIConstantsPrivate.ScrollBarWidth, height);
            _scrollBarBounds = scrollBarBounds;
        }

        public virtual void Scrolling(int scrollStep)
        {
            Point oldPoint = ViewportPoint;
            ViewportPoint = new Point(oldPoint.X, oldPoint.Y + scrollStep);
        }

        public virtual void ScrollingTo(int viewportY) => ViewportPoint = new Point(ViewportPoint.X, viewportY);

        public virtual void ScrollingX(int scrollStep)
        {
            Point oldPoint = ViewportPoint;
            ViewportPoint = new Point(oldPoint.X + scrollStep, oldPoint.Y);
        }

        public virtual void ScrollingXTo(int viewportX) => ViewportPoint = new Point(viewportX, ViewportPoint.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsScrolledToStart() => ViewportPoint.Y <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsScrolledToEnd() => ViewportPoint.Y >= SurfaceSize.Height - ContentSize.Height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScrollToStart() => ViewportPoint = ViewportPoint with { Y = int.MinValue };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScrollToEnd() => ViewportPoint = ViewportPoint with { Y = int.MaxValue };

        private void MoveScrollBarButtonY(int movementY)
        {
            if (IsScrolledToStart() && movementY < 0 || IsScrolledToEnd() && movementY > 0)
                return;
            if (movementY != 0)
            {
                int scrollBarMaxHeight = _scrollBarBounds.Height - 4;
                int surfaceHeight = SurfaceSize.Height;
                int maxY = surfaceHeight - Bounds.Height;
                int viewPortY = ViewportPoint.Y + MathI.Ceiling(movementY * 1.0 * surfaceHeight / scrollBarMaxHeight);
                if (float.IsNaN(viewPortY) || viewPortY > maxY)
                {
                    viewPortY = maxY;
                }
                else if (viewPortY < 0)
                {
                    viewPortY = 0;
                }
                ViewportPoint = ViewportPoint with { Y = viewPortY };
            }
        }

        private void RepeatingTimer_Tick(object? state) => _repeatingAction?.Invoke();

        void IMouseInteractHandler.OnMouseDown(ref HandleableMouseEventArgs args) => OnMouseDown(ref args);
        void IMouseInteractHandler.OnMouseUp(in MouseEventArgs args) => OnMouseUp(in args);
        void IMouseMoveHandler.OnMouseMove(in MouseEventArgs args) => OnMouseMove(in args);
        void IMouseScrollHandler.OnMouseScroll(ref HandleableMouseEventArgs args) => OnMouseScroll(ref args);
        void IGlobalMouseMoveHandler.OnMouseMoveGlobally(in MouseEventArgs args) => OnMouseMoveGlobally(in args);

        protected virtual void OnMouseDown(ref HandleableMouseEventArgs args)
        {
            if (!_enabled || !_hasScrollBar || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;

            PointF location = args.Location;

            if (_scrollBarScrollButtonBounds.Contains(location))
            {
                args.Handle();

                _scrollButtonState = ButtonTriState.Pressed;
                _pinY = Y + args.Y;
                Update(ScrollableElementUpdateFlags.ScrollBar);
                return;
            }

            if (_scrollBarUpButtonBounds.Contains(location))
            {
                args.Handle();

                _scrollUpButtonState = ButtonTriState.Pressed;
                Update(ScrollableElementUpdateFlags.ScrollBar);
                OnScrollBarUpButtonClicked();
                _repeatingAction = OnScrollBarUpButtonClicked;
                _repeatingTimer.Change(SystemParameters.KeyboardDelay, SystemParameters.KeyboardSpeed);
                return;
            }

            if (_scrollBarDownButtonBounds.Contains(location))
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

        protected virtual void OnMouseUp(in MouseEventArgs args)
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

        protected virtual void OnMouseMove(in MouseEventArgs args)
        {
            if (!_enabled || !_hasScrollBar)
                return;
            bool updateScrollBar = false;
            if (_scrollButtonState != ButtonTriState.Pressed)
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

        protected virtual void OnMouseMoveGlobally(in MouseEventArgs args)
        {
            if (!_enabled || !_hasScrollBar || _scrollButtonState != ButtonTriState.Pressed)
                return;
            float currentY = args.Y;
            float oldY = ReferenceHelper.Exchange(ref _pinY, currentY);
            MoveScrollBarButtonY(MathI.Ceiling(currentY - oldY));
        }

        protected virtual void OnMouseScroll(ref HandleableMouseEventArgs args)
        {
            if (!_enabled || !_hasScrollBar || _scrollButtonState == ButtonTriState.Pressed)
                return;
            args.Handle();
            Scrolling(-args.Delta);
            CallMouseMove(args);
        }

        protected virtual void CallMouseMove(in MouseEventArgs args)
        {
            OnMouseMoveGlobally(new MouseEventArgs(PointToGlobal(args.Location), args.Buttons, args.Delta));
            OnMouseMove(in args);
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            _repeatingTimer.Dispose();
            SequenceHelper.Clear(_brushes);
        }
    }
}
