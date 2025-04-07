using System;
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

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
	public sealed partial class CheckBox : UIElement, IDisposable, IMouseEvents
	{
		public event EventHandler CheckedChanged;

		private static readonly string[] _brushNames = new string[(int)Brush._Last]
		{
			"border",
			"border.hovered" ,
			"border.pressed",
			"border.checked" ,
			"border.hovered.checked" ,
			"border.pressed.checked",
			"mark",
			"fore"
		}.WithPrefix("app.checkBox.").ToLowerAscii();

		private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

		private string _text;
		private D2D1StrokeStyle _strokeStyle;
		private DWriteTextLayout _textLayout;
		private D2D1Resource _checkSign;

		private Rect _checkBoxBounds;
		private ButtonTriState _buttonState;
		private bool _checkState, _generateLayout;
		private long _drawTypeRaw;

		public CheckBox(IRenderer renderer) : base(renderer) { }

		protected override void ApplyThemeCore(ThemeResourceProvider provider)
			=> UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);

		public override void OnSizeChanged()
		{
			DWriteTextLayout layout = _textLayout;
			if (layout is not null)
			{
				RectF bounds = GraphicsUtils.AdjustRectangleF(Bounds);
				layout.MaxHeight = bounds.Height;
				layout.MaxWidth = bounds.Width - _checkBoxBounds.Right - 4 - Renderer.GetBaseLineWidth();
			}
			RecalculateCheckBoxBounds();
			Update();
		}

		public override void OnLocationChanged()
		{
			RecalculateCheckBoxBounds();
			Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void Update() => Update(CheckBoxDrawType.RedrawAllContent);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Update(CheckBoxDrawType drawType)
		{
			if (drawType == CheckBoxDrawType.NoRedraw)
				return;
			InterlockedHelper.Or(ref _drawTypeRaw, (long)drawType);
			UpdateCore();
		}

		public override bool NeedRefresh()
		{
			if (_drawTypeRaw > (long)CheckBoxDrawType.NoRedraw)
				return true;
			return Interlocked.Read(ref _drawTypeRaw) > (long)CheckBoxDrawType.NoRedraw;
		}

		[Inline(InlineBehavior.Remove)]
		private CheckBoxDrawType GetCheckBoxDrawTypeAndReset()
			=> (CheckBoxDrawType)Interlocked.Exchange(ref _drawTypeRaw, (long)CheckBoxDrawType.NoRedraw);

		private DWriteTextLayout UpdateTextLayout()
		{
			string text = _text;
			if (text is null)
			{
				DisposeHelper.SwapDisposeInterlocked(ref _textLayout);
				return null;
			}
			Rect bounds = Bounds;
			if (bounds.IsEmpty)
			{
				DisposeHelper.SwapDisposeInterlocked(ref _textLayout);
				return null;
			}
			DWriteFactory writeFactory = SharedResources.DWriteFactory;
			using DWriteTextFormat textFormat = writeFactory.CreateTextFormat(StaticResources.CaptionFontFamilyName, 14);
			textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
			DWriteTextLayout textLayout = writeFactory.CreateTextLayout(text, textFormat, bounds.Right - _checkBoxBounds.Right - 4 - Renderer.GetBaseLineWidth(), bounds.Height);
			DisposeHelper.SwapDisposeInterlocked(ref _textLayout, textLayout);
			return textLayout;
		}

		public override void Render(DirtyAreaCollector collector) => Render(collector, markDirty: false);

		protected override bool RenderCore(DirtyAreaCollector collector)
		{
			CheckBoxDrawType drawType = GetCheckBoxDrawTypeAndReset();
			if (collector.IsEmptyInstance) //Force redraw
				drawType = CheckBoxDrawType.RedrawAllContent;
			else if (drawType == CheckBoxDrawType.NoRedraw)
				return true;
			D2D1DeviceContext context = Renderer.GetDeviceContext();
			float lineWidth = Renderer.GetBaseLineWidth();
			switch (drawType)
			{
				case CheckBoxDrawType.RedrawAllContent:
					Rect bounds = Bounds;
					D2D1Brush textBrush = _brushes[(int)Brush.TextBrush];
					context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
					RenderBackground(context);
					DrawCheckBox(context, lineWidth);
					DWriteTextLayout textLayout = _generateLayout ? UpdateTextLayout() : _textLayout;
					if (textLayout is not null)
					{
						float xOffset = lineWidth + 3;
						context.DrawTextLayout(new PointF(_checkBoxBounds.Right + xOffset, bounds.Top), textLayout, textBrush);
					}
					context.PopAxisAlignedClip();
					collector.MarkAsDirty(bounds);
					break;
				case CheckBoxDrawType.RedrawCheckBox:
					DrawCheckBox(context, lineWidth);
					collector.MarkAsDirty(GetCheckBoxBounds());
					break;
			}
			return true;
		}

		private void DrawCheckBox(in D2D1DeviceContext context, float lineWidth)
		{
			if (_strokeStyle is null)
			{
				context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
				_strokeStyle = context.GetFactory().CreateStrokeStyle(new D2D1StrokeStyleProperties() { DashCap = D2D1CapStyle.Round, StartCap = D2D1CapStyle.Round, EndCap = D2D1CapStyle.Round });
				context.AntialiasMode = D2D1AntialiasMode.Aliased;
			}
			bool checkState = _checkState;
			D2D1Brush[] brushes = _brushes;
			D2D1Brush backBrush = null;
			if (checkState)
			{
				switch (_buttonState)
				{
					case ButtonTriState.None:
						backBrush = brushes[(int)Brush.BorderCheckedBrush];
						break;
					case ButtonTriState.Hovered:
						backBrush = brushes[(int)Brush.BorderHoveredCheckedBrush];
						break;
					case ButtonTriState.Pressed:
						backBrush = brushes[(int)Brush.BorderPressedCheckedBrush];
						break;
				}
			}
			else
			{
				switch (_buttonState)
				{
					case ButtonTriState.None:
						backBrush = brushes[(int)Brush.BorderBrush];
						break;
					case ButtonTriState.Hovered:
						backBrush = brushes[(int)Brush.BorderHoveredBrush];
						break;
					case ButtonTriState.Pressed:
						backBrush = brushes[(int)Brush.BorderPressedBrush];
						break;
				}
			}
			if (backBrush is null)
				return;
			Rect drawingBounds = _checkBoxBounds;
			if (checkState)
			{
				context.PushAxisAlignedClip((RectF)drawingBounds, D2D1AntialiasMode.Aliased);
				RenderBackground(context, backBrush);
				context.Transform = new Matrix3x2() { Translation = new Vector2(drawingBounds.X, drawingBounds.Y), M11 = 1f, M22 = 1f };
				D2D1StrokeStyle strokeStyle = _strokeStyle;
				D2D1Resource checkSign = UIElementHelper.GetOrCreateCheckSign(ref _checkSign, context, strokeStyle, drawingBounds);
				if (checkSign is D2D1GeometryRealization geometryRealization && context is D2D1DeviceContext1 context1)
					context1.DrawGeometryRealization(geometryRealization, brushes[(int)Brush.MarkBrush]);
				else if (checkSign is D2D1Geometry geometry)
					context.DrawGeometry(geometry, brushes[(int)Brush.MarkBrush], 2.0f, strokeStyle);
				context.Transform = Matrix3x2.Identity;
			}
			else
			{
				context.PushAxisAlignedClip((RectF)drawingBounds, D2D1AntialiasMode.Aliased);
				RenderBackground(context);
				context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(drawingBounds, lineWidth), backBrush, lineWidth);
			}
			context.PopAxisAlignedClip();
		}

		public void OnMouseMove(in MouseInteractEventArgs args)
		{
			if (_checkBoxBounds.IsEmpty)
				return;
			ButtonTriState oldButtonState = _buttonState;
			ButtonTriState newButtonState = ButtonTriState.None;
			if (Bounds.Contains(args.Location))
				newButtonState = ButtonTriState.Hovered;
			if (oldButtonState == newButtonState)
				return;
			if (oldButtonState != ButtonTriState.Pressed)
			{
				_buttonState = newButtonState;
				Update(CheckBoxDrawType.RedrawCheckBox);
			}
		}

		public void OnMouseDown(in MouseInteractEventArgs args)
		{
			if (_buttonState == ButtonTriState.Hovered)
			{
				_buttonState = ButtonTriState.Pressed;
				Checked = !Checked;
			}
		}

		public void OnMouseUp(in MouseInteractEventArgs args)
		{
			if (_buttonState == ButtonTriState.Pressed)
			{
				if (Bounds.Contains(args.Location))
				{
					_buttonState = ButtonTriState.Hovered;
				}
				else
				{
					_buttonState = ButtonTriState.None;
				}
				Update(CheckBoxDrawType.RedrawCheckBox);
			}
		}

		private void RecalculateCheckBoxBounds()
		{
			Rect bounds = Bounds;
			bounds.Width = bounds.Height;
			_checkBoxBounds = bounds;
			DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
		}

		public Rect GetCheckBoxBounds()
		{
			return _checkBoxBounds;
		}

		public void Dispose()
		{
			_checkSign?.Dispose();
			_strokeStyle?.Dispose();
			_textLayout?.Dispose();
		}
	}
}
