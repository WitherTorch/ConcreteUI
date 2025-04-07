using System;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
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
    public sealed partial class ComboBox : UIElement, IDisposable, IMouseEvents
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
        }.WithPrefix("app.comboBox.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly ObservableList<string> _items;
        private readonly ComboBoxDropdownList _dropdownList;

        private DWriteTextFormat _format;
        private DWriteTextLayout _layout;
        private string _text;
        private ButtonTriState _state = ButtonTriState.None;
        private long _rawUpdateFlags;
        private float _fontSize;
        private int _selectedIndex, _dropDownListVisibleCount;
        private bool _hovered, _enabled;

        public ComboBox(CoreWindow window) : base(window)
        {
            _dropdownList = new ComboBoxDropdownList(this, window);
            _dropdownList.ItemClicked += ListControl_ItemClicked;
            _items = new ObservableList<string>();
            _text = string.Empty;
            _selectedIndex = -1;
            _dropDownListVisibleCount = 10;
            _enabled = true;
            _fontSize = 13;
            _rawUpdateFlags = -1L;
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            _dropdownList.ApplyTheme(provider);
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            if (Renderer.IsInitializingElements())
                return;
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        [Inline(InlineBehavior.Remove)]
        private DWriteTextLayout GetTextLayout()
        {
            RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
            if ((flags & RenderObjectUpdateFlags.Layout) != RenderObjectUpdateFlags.Layout)
                return _layout;
            DWriteTextFormat format;
            if ((flags & RenderObjectUpdateFlags.FormatAndLayout) == RenderObjectUpdateFlags.FormatAndLayout)
            {
                format = TextFormatUtils.CreateTextFormat(TextAlignment.MiddleLeft, _fontSize);
                DisposeHelper.SwapDispose(ref _format, format);
            }
            else
            {
                format = _format;
            }
            string text = _text;
            DWriteTextLayout layout;
            if (string.IsNullOrEmpty(text))
                layout = null;
            else
                layout = SharedResources.DWriteFactory.CreateTextLayout(_text, format);
            DisposeHelper.SwapDispose(ref _layout, layout);
            return layout;
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            DWriteTextLayout layout = GetTextLayout();
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
                layout.MaxHeight = layoutRect.Height;
                layout.MaxWidth = layoutRect.Width;
                context.PushAxisAlignedClip(layoutRect, D2D1AntialiasMode.Aliased);
                context.DrawTextLayout(layoutRect.Location, layout, brushes[(int)Brush.TextBrush], D2D1DrawTextOptions.Clip);
                context.PopAxisAlignedClip();
            }
            RectF buttonRect = new RectF(bounds.Right - bounds.Bottom + bounds.Top + 1, bounds.Top + 1, bounds.Right - 1, bounds.Bottom - 1);
            context.PushAxisAlignedClip(buttonRect, D2D1AntialiasMode.Aliased);
            RenderBackground(context, backBrush);
            D2D1Brush buttonBrush = _state switch
            {
                ButtonTriState.None => brushes[(int)Brush.DropdownButtonBrush],
                ButtonTriState.Hovered => brushes[(int)Brush.DropdownButtonHoveredBrush],
                ButtonTriState.Pressed => brushes[(int)Brush.DropdownButtonPressedBrush],
                _ => throw new InvalidOperationException(),
            };
            FontIconResources.Instance.DrawDropDownButton(context, buttonRect.Location, buttonRect.Height, buttonBrush);
            context.PopAxisAlignedClip();
            return true;
        }

        public void OnMouseDown(in MouseInteractEventArgs args)
        {
            if (_enabled)
            {
                if (Bounds.Contains(args.Location))
                {
                    _state = ButtonTriState.Pressed;
                    Update();
                    if (_items.Count > 0)
                    {
                        ComboBoxDropdownList dropdownList = _dropdownList;
                        using DWriteTextFormat format = TextFormatUtils.CreateTextFormat(TextAlignment.MiddleLeft, _fontSize);
                        dropdownList.Prepare(format);
                        DropdownListOpened?.Invoke(this, new DropdownListEventArgs(dropdownList));
                    }
                }
                return;
            }
            if (_state == ButtonTriState.None)
                return;
            _state = ButtonTriState.None;
            Update();
        }

        public void OnMouseUp(in MouseInteractEventArgs args)
        {
            if (_enabled)
            {
                if (Bounds.Contains(args.Location))
                {
                    _state = ButtonTriState.Hovered;
                }
                else
                {
                    _state = ButtonTriState.None;
                }
                Update();
                return;
            }
            if (_state == ButtonTriState.None)
                return;
            _state = ButtonTriState.None;
            Update();
        }

        public void OnMouseMove(in MouseInteractEventArgs args)
        {
            if (Enabled)
            {
                Rectangle bounds = Bounds;
                RectangleF buttonRect = new RectangleF(bounds.Right - bounds.Height + 1, bounds.Top + 1, bounds.Height - 2, bounds.Height - 2);
                if (bounds.Contains(args.Location))
                {
                    if (!_hovered)
                    {
                        _hovered = true;
                        Update();
                    }
                    if (buttonRect.Contains(args.Location))
                    {
                        if (_state != ButtonTriState.Hovered)
                        {
                            _state = ButtonTriState.Hovered;
                            if (_hovered) Update();
                        }
                    }
                    else
                    {
                        if (_state == ButtonTriState.Hovered)
                        {
                            _state = ButtonTriState.None;
                            if (_hovered) Update();
                        }
                    }
                }
                else
                {
                    if (_hovered)
                    {
                        _hovered = false;
                        _state = ButtonTriState.None;
                        Update();
                    }
                    else if (_state != ButtonTriState.None)
                    {
                        _state = ButtonTriState.None;
                        Update();
                    }
                }
            }
            else
            {
                if (_state != ButtonTriState.None)
                {
                    _state = ButtonTriState.None;
                    Update();
                }
            }
        }

        private void ListControl_ItemClicked(object sender, EventArgs e)
        {
            if (sender is not ComboBoxDropdownList dropdownList)
                return;
            SelectedIndex = dropdownList.SelectedIndex;
            ItemClicked?.Invoke(this, e);
        }

        public void Dispose()
        {
            _format?.Dispose();
        }
    }
}
