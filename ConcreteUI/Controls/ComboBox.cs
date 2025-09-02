using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ComboBox : UIElement, IDisposable, IMouseInteractEvents
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "back.disabled",
            "back.hovered",
            "border",
            "fore",
            "button",
            "button.hovered",
            "button.pressed",
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly LayoutVariable?[] _autoLayoutVariableCache = new LayoutVariable?[1];
        private readonly ObservableList<string> _items;
        private readonly CoreWindow _window;

        private DWriteTextLayout? _layout;
        private string? _fontName;
        private string _text;
        private ButtonTriState _state = ButtonTriState.None;
        private long _rawUpdateFlags;
        private float _fontSize;
        private int _selectedIndex, _dropDownListVisibleCount;
        private bool _isPressed, _hovered, _enabled, _disposed;

        public ComboBox(CoreWindow window) : base(window, "app.comboBox")
        {
            _items = new ObservableList<string>();
            _window = window;
            _text = string.Empty;
            _selectedIndex = -1;
            _dropDownListVisibleCount = 10;
            _enabled = true;
            _fontSize = UIConstants.BoxFontSize;
            _rawUpdateFlags = -1L;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComboBox WithAutoHeight()
        {
            HeightVariable = AutoHeightReference;
            return this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            _fontName = provider.FontName;
            DisposeHelper.SwapDisposeInterlocked(ref _layout);
            Update(RenderObjectUpdateFlags.Format);
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        [Inline(InlineBehavior.Remove)]
        private DWriteTextLayout? GetTextLayout(RenderObjectUpdateFlags flags)
        {
            DWriteTextLayout? layout = Interlocked.Exchange(ref _layout, null);

            if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
            {
                DWriteTextFormat? format = layout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleLeft, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
                string text = _text;
                if (StringHelper.IsNullOrEmpty(text))
                    layout = null;
                else
                    layout = SharedResources.DWriteFactory.CreateTextLayout(text, format);
                format.Dispose();
            }
            return layout;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
        {
            if (format is null || format.IsDisposed)
                return true;
            if ((flags & RenderObjectUpdateFlags.Format) == RenderObjectUpdateFlags.Format)
            {
                format.Dispose();
                return true;
            }
            return false;
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            DWriteTextLayout? layout = GetTextLayout(GetAndCleanRenderObjectUpdateFlags());
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            float lineWidth = Renderer.GetBaseLineWidth();
            Rect bounds = Bounds;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush backBrush;
            if (Enabled)
                backBrush = _hovered ? brushes[(int)Brush.BackHoveredBrush] : brushes[(int)Brush.BackBrush];
            else
                backBrush = brushes[(int)Brush.BackDisabledBrush];
            RenderBackground(context, backBrush);
            context.DrawRectangle(GraphicsUtils.AdjustRectangleFAsBorderBounds((RectF)bounds, lineWidth), brushes[(int)Brush.BorderBrush], lineWidth);
            if (layout is not null)
            {
                float xOffset = lineWidth + 2;
                float boxLeft = bounds.X + xOffset;
                float boxTop = bounds.Y;
                float boxRight = bounds.Right - xOffset - bounds.Height;
                float boxBottom = bounds.Bottom;
                RectF layoutRect = new RectF(boxLeft, boxTop, boxRight, boxBottom);
                if (layoutRect.IsValid)
                {
                    layout.MaxHeight = layoutRect.Height;
                    layout.MaxWidth = layoutRect.Width;
                    context.PushAxisAlignedClip(layoutRect, D2D1AntialiasMode.Aliased);
                    context.DrawTextLayout(layoutRect.Location, layout, brushes[(int)Brush.TextBrush], D2D1DrawTextOptions.Clip);
                    context.PopAxisAlignedClip();
                }
                DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            }

            RectF buttonRect = new RectF(0, bounds.Top + lineWidth, bounds.Right - lineWidth, bounds.Bottom - lineWidth);
            buttonRect.Left = buttonRect.Right - buttonRect.Height;
            context.PushAxisAlignedClip(buttonRect, D2D1AntialiasMode.Aliased);
            RenderBackground(context, backBrush);
            D2D1Brush buttonBrush = _state switch
            {
                ButtonTriState.None => brushes[(int)Brush.DropdownButtonBrush],
                ButtonTriState.Hovered => brushes[(int)Brush.DropdownButtonHoveredBrush],
                ButtonTriState.Pressed => brushes[(int)Brush.DropdownButtonPressedBrush],
                _ => throw new InvalidOperationException(),
            };
            FontIconResources.Instance.DrawDropDownButton(context, (RectangleF)buttonRect, buttonBrush);
            context.PopAxisAlignedClip();
            return true;
        }

        public void OnMouseDown(ref MouseInteractEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            _isPressed = true;
            _state = ButtonTriState.Pressed;
            Update();

            if (_items.Count <= 0)
                return;
            WindowMessageLoop.InvokeAsync(() =>
            {
                EventHandler<DropdownListEventArgs>? eventHandler = RequestDropdownListOpening;
                if (eventHandler is null)
                    return;
                ComboBoxDropdownList dropdownList = new ComboBoxDropdownList(this, _window);
                dropdownList.ItemClicked += ListControl_ItemClicked;
                eventHandler.Invoke(this, new DropdownListEventArgs(dropdownList));
            });
        }

        public void OnMouseUp(in MouseNotifyEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            _isPressed = false;
            if (_state != ButtonTriState.Pressed)
                return;
            Rectangle bounds = Bounds;
            RectangleF buttonRect = new RectangleF(bounds.Right - bounds.Height + 1, bounds.Top + 1, bounds.Height - 2, bounds.Height - 2);
            _state = buttonRect.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;
            Update();
        }

        public void OnMouseMove(in MouseNotifyEventArgs args)
        {
            if (!_enabled)
                return;
            Rectangle bounds = Bounds;

            bool newHovered;
            ButtonTriState newButtonState;
            if (!bounds.Contains(args.Location))
            {
                newButtonState = ButtonTriState.None;
                newHovered = false;
                goto Update;
            }

            newHovered = true;
            if (_isPressed)
            {
                newButtonState = ButtonTriState.Pressed;
                goto Update;
            }

            RectangleF buttonRect = new RectangleF(bounds.Right - bounds.Height + 1, bounds.Top + 1, bounds.Height - 2, bounds.Height - 2);
            newButtonState = buttonRect.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;

        Update:
            if (_state == newButtonState && _hovered == newHovered)
                return;
            _state = newButtonState;
            _hovered = newHovered;
            Update();
        }

        private void ListControl_ItemClicked(object? sender, int selectedIndex)
        {
            if (sender is not ComboBoxDropdownList dropdownList)
                return;
            if (_state != ButtonTriState.None)
            {
                _state = ButtonTriState.None;
                Update();
            }
            SelectedIndex = selectedIndex;
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        ~ComboBox()
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
