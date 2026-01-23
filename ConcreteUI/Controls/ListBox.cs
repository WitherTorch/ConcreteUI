using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ListBox : ScrollableElementBase, IDpiAwareEvents
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "back.disabled",
            "border",
            "fore"
        };
        private static readonly string[] _checkBoxBrushNames = new string[(int)CheckBoxBrush._Last]
        {
            "border",
            "border.hovered" ,
            "border.pressed",
            "border.checked" ,
            "border.hovered.checked" ,
            "border.pressed.checked",
            "mark"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly D2D1Brush[] _checkBoxBrushes = new D2D1Brush[(int)CheckBoxBrush._Last];
        private readonly BitList _stateVectorList;
        private readonly ObservableList<string> _items;

        private DWriteTextFormat? _format;
        private string _checkBoxThemePrefix;
        private string? _fontName;
        private ListBoxMode _chooseMode;
        private ButtonTriState _buttonState;
        private long _recalcFormat;
        private float _fontSize, _itemHeight;
        private int _selectedIndex;

        public ListBox(IRenderer renderer) : base(renderer, "app.listBox")
        {
            _stateVectorList = new BitList();
            _items = new ObservableList<string>();
            _items.Updated += Items_Updated;
            _items.BeforeAdd += Item_BeforeAdd;
            ScrollBarType = ScrollBarType.AutoVertial;
            _fontSize = UIConstants.BoxFontSize;
            _selectedIndex = -1;
            _recalcFormat = Booleans.TrueLong;
            _checkBoxThemePrefix = "app.checkBox".ToLowerAscii();
        }

        public void CopySelectedItemsToBuffer(string[] destination, int startIndex, out int itemCopied)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            ObservableList<string> items = _items;
            int count = items.Count;
            if (count <= 0)
            {
                itemCopied = 0;
                return;
            }
            CopySelectedItemsToBufferCore(items, count, destination, startIndex, out itemCopied);
        }

        public void CopySelectedIndicesToBuffer(int[] destination, int startIndex, out int itemCopied)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            int count = _items.Count;
            if (count <= 0)
            {
                itemCopied = 0;
                return;
            }
            CopySelectedIndicesToBufferCore(count, destination, startIndex, out itemCopied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopySelectedItemsToBufferCore(ObservableList<string> items, int count, string[] destination, int startIndex, out int itemCopied)
        {
            BitList stateVectorList = _stateVectorList;
            itemCopied = 0;
            for (int i = 0; i < count; i++)
            {
                if (!stateVectorList[i])
                    continue;
                destination[startIndex++] = items[i];
                itemCopied++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopySelectedIndicesToBufferCore(int count, int[] destination, int startIndex, out int itemCopied)
        {
            BitList stateVectorList = _stateVectorList;
            itemCopied = 0;
            for (int i = 0; i < count; i++)
            {
                if (!stateVectorList[i])
                    continue;
                destination[startIndex++] = i;
                itemCopied++;
            }
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            string fontName = provider.FontName;
            _fontName = fontName;
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            UIElementHelper.ApplyTheme(provider, _checkBoxBrushes, _checkBoxBrushNames, _checkBoxThemePrefix, (int)CheckBoxBrush._Last);
            DisposeHelper.SwapDisposeInterlocked(ref _format);
            Interlocked.Exchange(ref _recalcFormat, Booleans.TrueLong);
            OnDpiChangedCore(fontName, Renderer.GetPointsPerPixel());
        }

        protected override D2D1Brush GetBackBrush() => _brushes[(int)Brush.BackBrush];
        protected override D2D1Brush GetBackDisabledBrush() => _brushes[(int)Brush.BackDisabledBrush];
        protected override D2D1Brush GetBorderBrush() => _brushes[(int)Brush.BorderBrush];

        private DWriteTextFormat BuildTextFormat()
        {
            DWriteTextFormat textFormat = SharedResources.DWriteFactory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
            textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
            return textFormat;
        }

        public void OnDpiChanged(in DpiChangedEventArgs args)
        {
            string? fontName = _fontName;
            if (fontName is null)
                return;
            OnDpiChangedCore(fontName, args.PointsPerPixel);
        }

        private void OnDpiChangedCore(string fontName, Vector2 pointsPerPixel)
        {
            Interlocked.Exchange(ref _itemHeight,
                RenderingHelper.CeilingInPixel(FontHeightHelper.GetFontHeight(fontName, _fontSize) + 2, pointsPerPixel.Y));
            RecalculateHeight();
        }

        private void Items_Updated(object? sender, EventArgs e) => RecalculateHeight();

        private void Item_BeforeAdd(object? sender, BeforeListAddOrRemoveEventArgs<string> e) => _stateVectorList.Add(false);

        public override void Scrolling(int rollStep) => base.Scrolling(rollStep / 4);

        [Inline(InlineBehavior.Remove)]
        private bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format)
        {
            if (Interlocked.Exchange(ref _recalcFormat, Booleans.FalseLong) != Booleans.FalseLong)
            {
                format?.Dispose();
                return true;
            }
            return format is null || format.IsDisposed;
        }

        protected override bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
        {
            if (context.HasDirtyCollector)
            {
                RenderBackground(context, backBrush);
                context.MarkAsDirty();
            }

            D2D1Brush[] brushes = _brushes;
            DWriteTextFormat? format = Interlocked.Exchange(ref _format, null);
            if (CheckFormatIsNotAvailable(format))
                format = BuildTextFormat();
            SizeF renderSize = context.Size;
            ListBoxMode mode = Mode;
            float itemHeight = _itemHeight;
            int currentTop = ViewportPoint.Y;
            int startIndex = (int)(currentTop / itemHeight);
            int endIndex = MathI.Ceiling((currentTop + renderSize.Height) / itemHeight);
            Vector2 pointsPerPixel = context.PointsPerPixel;
            float borderWidth = context.DefaultBorderWidth;

            float itemLeftEdge = borderWidth + 2;
            float textLeftEdge = mode == ListBoxMode.None ? itemLeftEdge : itemLeftEdge * 2 + itemHeight;
            float itemTopEdge = startIndex * itemHeight - currentTop + borderWidth + 2;
            float itemRightEdge = renderSize.Width - borderWidth;
            itemLeftEdge = RenderingHelper.RoundInPixel(itemLeftEdge, pointsPerPixel.X);
            textLeftEdge = RenderingHelper.RoundInPixel(textLeftEdge, pointsPerPixel.X) - itemLeftEdge;
            itemTopEdge = RenderingHelper.RoundInPixel(itemTopEdge, pointsPerPixel.Y);
            float itemWidth = itemRightEdge - itemLeftEdge;
            // itemRightEdge 無須做 round 操作，因為 renderSize.Width 與 borderWidth 均已對齊 pixel

            BitList stateVectorList = _stateVectorList;
            D2D1Brush textBrush = brushes[(int)Brush.TextBrush];
            IList<string> items = _items.GetUnderlyingList();
            for (int i = startIndex, count = items.Count, selectedIndex = _selectedIndex; i <= endIndex && i < count; i++)
            {
                string item = items[i];
                RectF itemBounds = new RectF(itemLeftEdge, itemTopEdge, itemRightEdge, itemTopEdge + itemHeight);
                using RegionalRenderingContext itemContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                switch (mode)
                {
                    case ListBoxMode.None:
                        if (StringHelper.IsNullOrWhiteSpace(item))
                            break;
                        itemContext.DrawText(item, format, RectF.FromXYWH(textLeftEdge, 0, itemWidth, itemHeight), textBrush, D2D1DrawTextOptions.Clip, DWriteMeasuringMode.Natural);
                        break;
                    case ListBoxMode.Any:
                        DrawRadioBox(in itemContext, itemHeight, stateVectorList[i], selectedIndex == i);
                        goto case ListBoxMode.None;
                    case ListBoxMode.Some:
                        DrawCheckBox(in itemContext, itemHeight, stateVectorList[i], selectedIndex == i);
                        goto case ListBoxMode.None;
                }
                itemTopEdge += itemHeight;
            }
            DisposeHelper.NullSwapOrDispose(ref _format, format);
            return true;
        }

        private void DrawCheckBox(in RegionalRenderingContext context, float itemHeight, bool checkState, bool isCurrentlyItem)
        {
            RectangleF renderingBounds = CheckBox.GetCheckBoxRenderingBounds(in context, itemHeight);
            CheckBox.DrawCheckBox(context, _checkBoxBrushes, renderingBounds, checkState,
                isCurrentlyItem ? _buttonState : ButtonTriState.None);
        }

        private void DrawRadioBox(in RegionalRenderingContext context, float itemHeight, bool isChecked, bool isCurrentlyItem)
        {
            D2D1Brush[] brushes = _checkBoxBrushes;
            D2D1Brush? backBrush = null;
            if (isCurrentlyItem)
            {
                switch (_buttonState)
                {
                    case ButtonTriState.None:
                        backBrush = brushes[(int)CheckBoxBrush.BorderBrush];
                        break;
                    case ButtonTriState.Hovered:
                        backBrush = brushes[(int)CheckBoxBrush.BorderHoveredBrush];
                        break;
                    case ButtonTriState.Pressed:
                        backBrush = brushes[(int)CheckBoxBrush.BorderPressedBrush];
                        break;
                }
            }
            else
            {
                backBrush = brushes[(int)CheckBoxBrush.BorderBrush];
            }
            if (backBrush is null)
                return;
            RectF renderingBounds = RectF.FromXYWH(PointF.Empty, new SizeF(itemHeight, itemHeight));
            D2D1DeviceContext deviceContext = context.DeviceContext;
            (D2D1AntialiasMode andtialiasModeBefore, deviceContext.AntialiasMode) = (deviceContext.AntialiasMode, D2D1AntialiasMode.PerPrimitive);
            try
            {
                using RenderingClipToken token = context.PushPixelAlignedClip(ref renderingBounds, D2D1AntialiasMode.Aliased);
                PointF centerPoint = new PointF(renderingBounds.Width * 0.5f, renderingBounds.Height * 0.5f);
                float borderWidth = context.DefaultBorderWidth;
                float width = centerPoint.X - borderWidth * 2.0f;
                context.DrawEllipse(new D2D1Ellipse(centerPoint, width, width), backBrush, borderWidth * 2.0f);
                if (isChecked)
                {
                    width -= borderWidth * 2.0f;
                    context.FillEllipse(new D2D1Ellipse(centerPoint, width, width), backBrush);
                }
            }
            finally
            {
                deviceContext.AntialiasMode = andtialiasModeBefore;
            }
        }

        private void RecalculateHeight()
        {
            SurfaceSize = new Size(0, MathI.Ceiling(Items.Count * _itemHeight) + UIConstants.ElementMargin);
            Update();
        }

        public override void OnMouseMove(in MouseNotifyEventArgs args)
        {
            base.OnMouseMove(args);
            if (!ContentBounds.Contains(args.Location))
            {
                if (_buttonState != ButtonTriState.None)
                {
                    _buttonState = ButtonTriState.None;
                    Update();
                }
                return;
            }
            if (Mode == ListBoxMode.None)
                return;
            int selectedIndex = (int)((args.Y + ViewportPoint.Y - Location.Y) / _itemHeight);
            if (selectedIndex >= Items.Count) selectedIndex = -1;
            if (_selectedIndex == selectedIndex)
                return;
            _selectedIndex = selectedIndex;
            ButtonTriState state = ButtonTriState.None;
            if (selectedIndex > -1)
            {
                if (_buttonState != ButtonTriState.Pressed)
                    state = ButtonTriState.Hovered;
            }
            else
            {
                state = ButtonTriState.None;
            }
            if (_buttonState != state)
                _buttonState = state;
            Update();
        }

        public override void OnMouseDown(ref MouseInteractEventArgs args)
        {
            base.OnMouseDown(ref args);
            if (args.Handled || Mode == ListBoxMode.None || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;
            args.Handle();
            if (_buttonState == ButtonTriState.Hovered)
                _buttonState = ButtonTriState.Pressed;
            Update();
        }

        public override void OnMouseUp(in MouseNotifyEventArgs args)
        {
            base.OnMouseUp(args);
            ListBoxMode mode = Mode;
            if (mode == ListBoxMode.None || _buttonState != ButtonTriState.Pressed)
                return;
            if (ContentBounds.Contains(args.Location))
                _buttonState = ButtonTriState.Hovered;
            else
                _buttonState = ButtonTriState.None;
            switch (Mode)
            {
                case ListBoxMode.Any:
                    ClearCheckStateCore();
                    goto case ListBoxMode.Some;
                case ListBoxMode.Some:
                    int selectedIndex = _selectedIndex;
                    RevertCheckStateCore(selectedIndex);
                    OnSelectedIndicesChanged();
                    break;
            }
            Update();
        }

        public bool IsChecked(int index) => Mode != ListBoxMode.None && GetCheckStateCore(index);

        public void SetChecked(int index, bool value)
        {
            ListBoxMode mode = Mode;
            switch (mode)
            {
                case ListBoxMode.Any:
                    if (IsChecked(index) == value)
                        return;
                    ClearCheckStateCore();
                    goto case ListBoxMode.Some;
                case ListBoxMode.Some:
                    SetCheckStateCore(index, value);
                    break;
            }
        }

        private void OnSelectedIndicesChanged() => SelectedIndicesChanged?.Invoke(this, EventArgs.Empty);

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _format);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        protected override void OnScrollBarUpButtonClicked() => Scrolling(-MathI.Ceiling(_itemHeight));

        protected override void OnScrollBarDownButtonClicked() => Scrolling(MathI.Ceiling(_itemHeight));

        [Inline(InlineBehavior.Remove)]
        private bool GetCheckStateCore(int index) => _stateVectorList[index];

        [Inline(InlineBehavior.Remove)]
        private void SetCheckStateCore(int index, bool state) => _stateVectorList[index] = state;

        [Inline(InlineBehavior.Remove)]
        private void RevertCheckStateCore(int index)
            => RevertCheckStateCore(_stateVectorList, index);

        [Inline(InlineBehavior.Remove)]
        private static void RevertCheckStateCore(BitList stateVectorList, int index)
            => stateVectorList[index] = !stateVectorList[index];

        [Inline(InlineBehavior.Remove)]
        private void ClearCheckStateCore() => _stateVectorList.SetAllBitsAsFalse();
    }
}
