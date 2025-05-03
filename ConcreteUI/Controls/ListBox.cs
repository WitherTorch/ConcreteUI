using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
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
        }.WithPrefix("app.listBox.").ToLowerAscii();
        private static readonly string[] _checkBoxBrushNames = new string[(int)CheckBoxBrush._Last]
        {
            "border",
            "border.hovered" ,
            "border.pressed",
            "border.checked" ,
            "border.hovered.checked" ,
            "border.pressed.checked",
            "mark"
        }.WithPrefix("app.checkBox.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly D2D1Brush[] _checkBoxBrushes = new D2D1Brush[(int)CheckBoxBrush._Last];
        private readonly List<BitVector64> _stateVectorList;
        private readonly ObservableList<string> _items;

        private D2D1StrokeStyle? _strokeStyle;
        private D2D1Resource? _checkSign;
        private DWriteTextFormat? _format;
        private string? _fontName;
        private Rectangle _checkBoxBounds;
        private ListBoxMode _chooseMode;
        private ButtonTriState _buttonState;
        private long _recalcFormat;
        private float _fontSize;
        private int _itemHeight, _selectedIndex;

        public ListBox(IRenderer renderer) : base(renderer)
        {
            _stateVectorList = new List<BitVector64>(1);
            _items = new ObservableList<string>();
            _items.Updated += Items_Updated;
            ScrollBarType = ScrollBarType.AutoVertial;
            _fontSize = UIConstants.BoxFontSize;
            _selectedIndex = -1;
            _recalcFormat = Booleans.TrueLong;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            _fontName = provider.FontName;
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            UIElementHelper.ApplyTheme(provider, _checkBoxBrushes, _checkBoxBrushNames, (int)CheckBoxBrush._Last);
            DisposeHelper.SwapDisposeInterlocked(ref _format);
            Interlocked.Exchange(ref _recalcFormat, Booleans.TrueLong);
            using DWriteTextFormat textFormat = SharedResources.DWriteFactory.CreateTextFormat(_fontName, _fontSize);
            textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
            Interlocked.Exchange(ref _itemHeight, MathI.Ceiling(GraphicsUtils.MeasureTextHeight("Ty", textFormat)) + 2);
            RecalculateCheckBoxBounds();
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

        protected override void Scrolling(int rollStep, bool update = true) => base.Scrolling(rollStep / 4, update);

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
            RectangleF checkBoxBounds = _checkBoxBounds;
            float lineWidth = Renderer.GetBaseLineWidth();
            int itemHeight = _itemHeight;
            int currentTop = ViewportPoint.Y;
            int startIndex = currentTop / itemHeight;
            int yOffset = startIndex * itemHeight - currentTop;
            int pointY = Location.Y + yOffset;
            int endIndex = MathHelper.CeilDiv(currentTop + Size.Height, itemHeight);
            float xOffset = 2f;
            if (mode == ListBoxMode.None)
                xOffset += bounds.X;
            else
                xOffset += checkBoxBounds.Right;

            List<BitVector64> stateVectorList = _stateVectorList;
            D2D1Brush textBrush = brushes[(int)Brush.TextBrush];
            IList<string> items = _items.GetUnderlyingList();
            for (int i = startIndex, count = items.Count, selectedIndex = _selectedIndex; i <= endIndex && i < count; i++)
            {
                string item = items[i];

                switch (mode)
                {
                    case ListBoxMode.None:
                        if (StringHelper.IsNullOrWhiteSpace(item))
                            break;
                        context.DrawText(item, format, new RectF(xOffset, pointY, bounds.Right - lineWidth, pointY + itemHeight),
                            textBrush, D2D1DrawTextOptions.Clip, DWriteMeasuringMode.Natural);
                        break;
                    case ListBoxMode.Any:
                        DrawRadioBox(context, pointY + 2, GetCheckStateCore(stateVectorList, i), selectedIndex == i, lineWidth);
                        goto case ListBoxMode.None;
                    case ListBoxMode.Some:
                        DrawCheckBox(context, pointY + 2, GetCheckStateCore(stateVectorList, i), selectedIndex == i, lineWidth);
                        goto case ListBoxMode.None;
                }
                pointY += itemHeight;
            }
            collector.MarkAsDirty(bounds);

            DisposeHelper.NullSwapOrDispose(ref _format, format);
            return true;
        }

        private void DrawCheckBox(in D2D1DeviceContext context, int offsetY, bool isChecked, bool isCurrentlyItem, float lineWidth)
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
            Rectangle drawingBounds = _checkBoxBounds;
            drawingBounds.Y = offsetY;
            if (isChecked)
            {
                context.PushAxisAlignedClip((RectF)drawingBounds, D2D1AntialiasMode.Aliased);
                RenderBackground(context, backBrush);
                context.Transform = new Matrix3x2() { Translation = new Vector2(drawingBounds.X, drawingBounds.Y), M11 = 1, M22 = 1 };
                D2D1StrokeStyle strokeStyle = _strokeStyle;
                D2D1Resource checkSign = UIElementHelper.GetOrCreateCheckSign(ref _checkSign, context, strokeStyle, drawingBounds);
                if (checkSign is D2D1GeometryRealization geometryRealization && context is D2D1DeviceContext1 context1)
                    context1.DrawGeometryRealization(geometryRealization, brushes[(int)CheckBoxBrush.MarkBrush]);
                else if (checkSign is D2D1Geometry geometry)
                    context.DrawGeometry(geometry, brushes[(int)CheckBoxBrush.MarkBrush], 2.0f, strokeStyle);
                context.Transform = Matrix3x2.Identity;
                context.PopAxisAlignedClip();
            }
            else
            {
                context.DrawRectangle(GraphicsUtils.AdjustRectangleFAsBorderBounds(drawingBounds, lineWidth), backBrush, lineWidth);
            }
        }

        private void DrawRadioBox(in D2D1DeviceContext context, int offsetY, bool isChecked, bool isCurrentlyItem, float lineWidth)
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
            Rectangle drawingBounds = _checkBoxBounds;
            drawingBounds.Y = offsetY;
            context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
            context.PushAxisAlignedClip((RectF)drawingBounds, D2D1AntialiasMode.Aliased);
            context.Transform = new Matrix3x2() { Translation = new Vector2(drawingBounds.X, drawingBounds.Y), M11 = 1, M22 = 1 };
            PointF centerPoint = new PointF(drawingBounds.Width * 0.5f, drawingBounds.Height * 0.5f);
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

        private void RecalculateCheckBoxBounds()
        {
            int itemHeight = _itemHeight;
            if (itemHeight <= 4)
                return;
            Rect bounds = ContentBounds;
            int buttonSize = itemHeight - 4;
            _checkBoxBounds = new Rectangle(bounds.X + 2, 0, buttonSize, buttonSize);
            DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
        }

        private void RecalculateHeight()
        {
            SurfaceSize = new Size(0, Items.Count * _itemHeight);
            Update();
        }

        public override void OnSizeChanged()
        {
            RecalculateCheckBoxBounds();
            RecalculateHeight();
            base.OnSizeChanged();
        }

        public override void OnLocationChanged()
        {
            RecalculateCheckBoxBounds();
            RecalculateHeight();
            base.OnLocationChanged();
        }

        public override void OnMouseMove(in MouseInteractEventArgs args)
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

        public override void OnMouseDown(in MouseInteractEventArgs args)
        {
            base.OnMouseDown(args);
            if (Mode != ListBoxMode.None)
            {
                if (_buttonState == ButtonTriState.Hovered)
                    _buttonState = ButtonTriState.Pressed;
                Update();
            }
        }

        public override void OnMouseUp(in MouseInteractEventArgs args)
        {
            base.OnMouseUp(args);
            ListBoxMode mode = Mode;
            if (mode == ListBoxMode.None || _buttonState != ButtonTriState.Pressed)
                return;
            if (ContentBounds.Contains(args.Location))
                _buttonState = ButtonTriState.Hovered;
            else
                _buttonState = ButtonTriState.None;
            if (args.Button != MouseButtons.Left)
            {
                Update();
                return;
            }
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
        private bool GetCheckStateCore(int index)
            => GetCheckStateCore(_stateVectorList, index);

        [Inline(InlineBehavior.Remove)]
        private static bool GetCheckStateCore(List<BitVector64> stateVectorList, int index)
        {
            int vectorIndex = index >> 6;
            if (stateVectorList.Count <= vectorIndex)
                return false;
            int vectorOffset = index & 63;
            return stateVectorList[vectorIndex][vectorOffset];
        }

        [Inline(InlineBehavior.Remove)]
        private void SetCheckStateCore(int index, bool state)
            => SetCheckStateCore(_stateVectorList, index, state);

        [Inline(InlineBehavior.Remove)]
        private static void SetCheckStateCore(List<BitVector64> stateVectorList, int index, bool state)
        {
            int vectorIndex = index >> 6;
            for (int i = stateVectorList.Count; i <= vectorIndex; i++)
                stateVectorList.Add(new BitVector64());
            int vectorOffset = index & 63;
            BitVector64 vector = stateVectorList[vectorIndex];
            vector[vectorOffset] = state;
            stateVectorList[vectorIndex] = vector;
        }

        [Inline(InlineBehavior.Remove)]
        private void RevertCheckStateCore(int index)
            => RevertCheckStateCore(_stateVectorList, index);

        [Inline(InlineBehavior.Remove)]
        private static void RevertCheckStateCore(List<BitVector64> stateVectorList, int index)
        {
            int vectorIndex = index >> 6;
            for (int i = stateVectorList.Count; i <= vectorIndex; i++)
                stateVectorList.Add(new BitVector64());
            int vectorOffset = index & 63;
            BitVector64 vector = stateVectorList[vectorIndex];
            vector[vectorOffset] = !vector[vectorOffset];
            stateVectorList[vectorIndex] = vector;
        }

        [Inline(InlineBehavior.Remove)]
        private void ClearCheckStateCore()
            => ClearCheckStateCore(_stateVectorList);

        [Inline(InlineBehavior.Remove)]
        private static void ClearCheckStateCore(List<BitVector64> stateVectorList)
        {
            int count = stateVectorList.Count;
            for (int i = 0; i < count; i++)
            {
                stateVectorList[i] = new BitVector64();
            }
        }
    }
}
