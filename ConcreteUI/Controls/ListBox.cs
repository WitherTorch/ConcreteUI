using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ListBox : ScrollableElementBase
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

        private D2D1StrokeStyle? _strokeStyle;
        private D2D1Resource? _checkSign;
        private DWriteTextFormat? _format;
        private string _checkBoxThemePrefix;
        private string? _fontName;
        private ListBoxMode _chooseMode;
        private ButtonTriState _buttonState;
        private long _recalcFormat;
        private float _fontSize;
        private int _itemHeight, _selectedIndex;

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
            _fontName = provider.FontName;
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            UIElementHelper.ApplyTheme(provider, _checkBoxBrushes, _checkBoxBrushNames, _checkBoxThemePrefix, (int)CheckBoxBrush._Last);
            DisposeHelper.SwapDisposeInterlocked(ref _format);
            Interlocked.Exchange(ref _recalcFormat, Booleans.TrueLong);
            using DWriteTextFormat textFormat = SharedResources.DWriteFactory.CreateTextFormat(_fontName, _fontSize);
            textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
            Interlocked.Exchange(ref _itemHeight, MathI.Ceiling(GraphicsUtils.MeasureTextHeight("Ty", textFormat)) + 2);
            DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
            RecalculateHeight();
            Update();
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

        protected override bool RenderContent(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            D2D1Brush[] brushes = _brushes;
            DWriteTextFormat? format = Interlocked.Exchange(ref _format, null);
            if (CheckFormatIsNotAvailable(format))
                format = BuildTextFormat();
            ListBoxMode mode = Mode;
            Rect bounds = ContentBounds;
            float lineWidth = Renderer.GetBaseLineWidth();
            float doubledLineWidth = 2.0f * lineWidth;
            int itemHeight = _itemHeight;
            int currentTop = ViewportPoint.Y;
            int startIndex = currentTop / itemHeight;
            int yOffset = startIndex * itemHeight - currentTop;
            int pointY = Location.Y + yOffset;
            int endIndex = MathHelper.CeilDiv(currentTop + Size.Height, itemHeight);
            float xOffset = bounds.X + doubledLineWidth;
            if (mode != ListBoxMode.None)
                xOffset += itemHeight + doubledLineWidth;

            BitList stateVectorList = _stateVectorList;
            D2D1Brush textBrush = brushes[(int)Brush.TextBrush];
            IList<string> items = _items.GetUnderlyingList();
            for (int i = startIndex, count = items.Count, selectedIndex = _selectedIndex; i <= endIndex && i < count; i++)
            {
                string item = items[i];
                RectF itemBounds = new RectF(bounds.X + doubledLineWidth, pointY, bounds.Right - doubledLineWidth, pointY + itemHeight);
                switch (mode)
                {
                    case ListBoxMode.None:
                        if (StringHelper.IsNullOrWhiteSpace(item))
                            break;
                        itemBounds.X = xOffset;
                        context.DrawText(item, format, itemBounds, textBrush, D2D1DrawTextOptions.Clip, DWriteMeasuringMode.Natural);
                        break;
                    case ListBoxMode.Any:
                        DrawRadioBox(context, itemBounds, stateVectorList[i], selectedIndex == i, doubledLineWidth, lineWidth, itemHeight);
                        goto case ListBoxMode.None;
                    case ListBoxMode.Some:
                        DrawCheckBox(context, itemBounds, stateVectorList[i], selectedIndex == i, doubledLineWidth, lineWidth, itemHeight);
                        goto case ListBoxMode.None;
                }
                pointY += itemHeight;
            }
            collector.MarkAsDirty(bounds);

            DisposeHelper.NullSwapOrDispose(ref _format, format);
            return true;
        }

        private void DrawCheckBox(in D2D1DeviceContext context, in RectF bounds, bool isChecked, bool isCurrentlyItem, float gap, float lineWidth, float itemHeight)
        {
            if (_strokeStyle == null)
            {
                context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
                _strokeStyle = context.GetFactory()!.CreateStrokeStyle(new D2D1StrokeStyleProperties() { DashCap = D2D1CapStyle.Round, StartCap = D2D1CapStyle.Round, EndCap = D2D1CapStyle.Round });
                context.AntialiasMode = D2D1AntialiasMode.Aliased;
            }
            D2D1Brush[] brushes = _checkBoxBrushes;
            D2D1Brush? backBrush = null;
            if (isChecked)
            {
                if (isCurrentlyItem)
                {
                    switch (_buttonState)
                    {
                        case ButtonTriState.None:
                            backBrush = brushes[(int)CheckBoxBrush.BorderCheckedBrush];
                            break;
                        case ButtonTriState.Hovered:
                            backBrush = brushes[(int)CheckBoxBrush.BorderHoveredCheckedBrush];
                            break;
                        case ButtonTriState.Pressed:
                            backBrush = brushes[(int)CheckBoxBrush.BorderPressedCheckedBrush];
                            break;
                    }
                }
                else
                {
                    backBrush = brushes[(int)CheckBoxBrush.BorderCheckedBrush];
                }
            }
            else
            {
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
            }
            if (backBrush is null)
                return;
            RectF renderingBounds = RectF.FromXYWH(bounds.X + gap, bounds.Y + gap, itemHeight - gap, itemHeight - gap);
            context.PushAxisAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased);
            RenderBackground(context);
            if (isChecked)
            {
                RenderBackground(context, backBrush);
                context.Transform = new Matrix3x2() { Translation = new Vector2(renderingBounds.X, renderingBounds.Y), M11 = 1f, M22 = 1f };
                D2D1StrokeStyle strokeStyle = _strokeStyle;
                D2D1Resource checkSign = UIElementHelper.GetOrCreateCheckSign(ref _checkSign, context, strokeStyle, renderingBounds);
                if (checkSign is D2D1GeometryRealization geometryRealization && context is D2D1DeviceContext1 context1)
                    context1.DrawGeometryRealization(geometryRealization, brushes[(int)CheckBoxBrush.MarkBrush]);
                else if (checkSign is D2D1Geometry geometry)
                    context.DrawGeometry(geometry, brushes[(int)CheckBoxBrush.MarkBrush], 2.0f, strokeStyle);
                context.Transform = Matrix3x2.Identity;
            }
            else
            {
                RenderBackground(context);
                context.DrawRectangle(GraphicsUtils.AdjustRectangleFAsBorderBounds(renderingBounds, lineWidth), backBrush, lineWidth);
            }
            context.PopAxisAlignedClip();
        }

        private void DrawRadioBox(in D2D1DeviceContext context, in RectF bounds, bool isChecked, bool isCurrentlyItem, float gap, float lineWidth, float itemHeight)
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
            RectF renderingBounds = RectF.FromXYWH(bounds.X + gap, bounds.Y + gap, itemHeight - gap * 2, itemHeight - gap * 2);
            context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
            context.PushAxisAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased);
            context.Transform = new Matrix3x2() { Translation = new Vector2(renderingBounds.X, renderingBounds.Y), M11 = 1, M22 = 1 };
            PointF centerPoint = new PointF(renderingBounds.Width * 0.5f, renderingBounds.Height * 0.5f);
            float width = centerPoint.X - lineWidth * 2.0f;
            context.DrawEllipse(new D2D1Ellipse(centerPoint, width, width), backBrush, lineWidth * 2.0f);
            if (isChecked)
            {
                width -= lineWidth * 2.0f;
                context.FillEllipse(new D2D1Ellipse(centerPoint, width, width), backBrush);
            }
            context.Transform = Matrix3x2.Identity;
            context.AntialiasMode = D2D1AntialiasMode.Aliased;
            context.PopAxisAlignedClip();
        }

        private void RecalculateHeight()
        {
            SurfaceSize = new Size(0, Items.Count * _itemHeight);
            Update();
        }

        public override void OnSizeChanged()
        {
            DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
            base.OnSizeChanged();
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
                DisposeHelper.SwapDisposeInterlocked(ref _strokeStyle);
                DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
                DisposeHelper.SwapDisposeInterlocked(ref _format);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        protected override void OnScrollBarUpButtonClicked() => Scrolling(-_itemHeight);

        protected override void OnScrollBarDownButtonClicked() => Scrolling(_itemHeight);

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
