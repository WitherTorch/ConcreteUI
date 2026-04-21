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

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ComboBox : DisposableUIElementBase, IMouseInteractHandler, IMouseMoveHandler
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

        private DWriteTextLayout? _layout;
        private string? _fontName;
        private string _text;
        private ButtonTriState _state = ButtonTriState.None;
        private long _rawUpdateFlags;
        private float _fontSize;
        private int _selectedIndex, _dropDownListVisibleCount;
        private bool _isPressed, _hovered, _enabled;

        public ComboBox(IElementContainer parent) : base(parent, "app.comboBox")
        {
            _items = new ObservableList<string>();
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
            UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
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

        protected override bool IsBackgroundOpaqueCore()
        {
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            D2D1Brush backBrush;
            if (Enabled)
            {
                DebugHelper.ThrowUnless((nuint)Brush.BackBrush + 2 == (nuint)Brush.BackHoveredBrush);
                backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush + 2 * MathHelper.BooleanToNativeUnsigned(_hovered));
            }
            else
                backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackDisabledBrush);
            return GraphicsUtils.CheckBrushIsSolid(backBrush);
        }

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            DWriteTextLayout? layout = GetTextLayout(GetAndCleanRenderObjectUpdateFlags());
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            D2D1Brush backBrush;
            if (Enabled)
            {
                DebugHelper.ThrowUnless((nuint)Brush.BackBrush + 2 == (nuint)Brush.BackHoveredBrush);
                backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush + 2 * MathHelper.BooleanToNativeUnsigned(_hovered));
            }
            else
                backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackDisabledBrush);
            RenderBackground(context, backBrush);
            SizeF renderSize = context.Size;
            float borderWidth = context.DefaultBorderWidth;
            float buttonWidth = renderSize.Height - borderWidth * 2.0f;
            RectF buttonRect = RectF.FromXYWH(renderSize.Width - renderSize.Height, borderWidth, buttonWidth, buttonWidth);
            if (layout is not null)
            {
                float xOffset = borderWidth + 2;
                RectF layoutRect = new RectF(xOffset, 0, renderSize.Width - xOffset, renderSize.Height);
                if (layoutRect.IsValid)
                {
                    layout.MaxHeight = layoutRect.Height;
                    layout.MaxWidth = layoutRect.Width;
                    using RenderingClipScope scope = context.PushAxisAlignedClip(layoutRect, D2D1AntialiasMode.Aliased);
                    context.DrawTextLayout(layoutRect.Location, layout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextBrush), D2D1DrawTextOptions.Clip);
                }
                DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            }
            using (RenderingClipScope scope = context.PushAxisAlignedClip(buttonRect, D2D1AntialiasMode.Aliased))
            {
                RenderBackground(context, backBrush);
                ButtonTriState state = _state;
                D2D1Brush buttonBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.DropdownButtonBrush + (state > ButtonTriState.Pressed ? 0u : (nuint)state));
                FontIconResources.Instance.DrawDropDownButton(context, (RectangleF)buttonRect, buttonBrush);
            }
            context.DrawBorder(UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BorderBrush));
            return true;
        }

        void IMouseInteractHandler.OnMouseDown(ref HandleableMouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
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
                ComboBoxDropdownList dropdownList = new ComboBoxDropdownList(Parent, this);
                dropdownList.ItemClicked += ListControl_ItemClicked;
                eventHandler.Invoke(this, new DropdownListEventArgs(dropdownList));
            });
        }

        void IMouseInteractHandler.OnMouseUp(in MouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;

            _isPressed = false;
            if (_state != ButtonTriState.Pressed)
                return;
            Size size = Size;
            RectangleF buttonRect = new RectangleF(size.Width - size.Height + 1, 1, size.Height - 2, size.Height - 2);
            _state = buttonRect.Contains(args.Location) ? ButtonTriState.Hovered : ButtonTriState.None;
            Update();
        }

        void IMouseMoveHandler.OnMouseMove(in MouseEventArgs args)
        {
            if (!_enabled)
                return;

            bool newHovered;
            ButtonTriState newButtonState;
            (int width, int height) = Size;
            if (!args.IsInSpecificSize(width, height))
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

            RectangleF buttonRect = new RectangleF(width - height + 1, 1, height - 2, height - 2);
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

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
