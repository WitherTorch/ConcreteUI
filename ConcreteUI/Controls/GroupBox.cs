using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
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
	public sealed partial class GroupBox : UIElement, IDisposable, IContainerElement
	{
		private static readonly string[] _brushNames = new string[(int)Brush._Last]
		{
			"back",
			"border",
			"fore"
		}.WithPrefix("app.groupBox.").ToLowerAscii();

		private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

		private readonly ObservableList<UIElement> _children;

		private DWriteTextLayout _titleLayout, _textLayout;
		private string _title, _text, _fontName;
		private long _updateFlags;
		private int _titleHeight;
		private bool _disposed;

		public GroupBox(IRenderer renderer) : base(renderer)
		{
			_children = new ObservableList<UIElement>(new UnwrappableList<UIElement>(capacity: 0));
			_children.BeforeAdd += Children_BeforeAdded;

			_title = string.Empty;
			_text = string.Empty;
			_updateFlags = Booleans.FalseLong;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddChild(UIElement element) => _children.Add(element);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveChild(UIElement element) => _children.Remove(element);

		protected override void ApplyThemeCore(ThemeResourceProvider provider)
		{
			UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
			foreach (UIElement child in _children)
				child.ApplyTheme(provider);
			_fontName = provider.FontName;
			using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(_fontName, 14);
			format.ParagraphAlignment = DWriteParagraphAlignment.Center;
			format.TextAlignment = DWriteTextAlignment.Center;
			format.WordWrapping = DWriteWordWrapping.NoWrap;
			Interlocked.Exchange(ref _titleHeight, GraphicsUtils.MeasureTextHeightAsInt("Ty", format));
		}

		public void RenderChildBackground(UIElement child, D2D1DeviceContext context)
			=> RenderBackground(context, _brushes[(int)Brush.BackBrush]);

		private void Children_BeforeAdded(object sender, BeforeListAddOrRemoveEventArgs<UIElement> e)
		{
			UIElement child = e.Item;
			if (child.Parent is null)
			{
				child.Parent = this;
				return;
			}
			e.Cancel = true;
			throw new InvalidOperationException();
		}

		protected override void Update()
		{
			Interlocked.Exchange(ref _updateFlags, -1L);
			base.Update();
		}

		[Inline(InlineBehavior.Remove)]
		private void Update(RenderObjectUpdateFlags flags)
		{
			InterlockedHelper.Or(ref _updateFlags, (long)flags);
			base.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void GetLayouts(RenderObjectUpdateFlags flags, out DWriteTextLayout titleLayout, out DWriteTextLayout textLayout)
		{
			titleLayout = Interlocked.Exchange(ref _titleLayout, null);
			textLayout = Interlocked.Exchange(ref _textLayout, null);

			DWriteFactory factory = SharedResources.DWriteFactory;
			if ((flags & RenderObjectUpdateFlags.Title) == RenderObjectUpdateFlags.Title)
			{
				DWriteTextFormat format = titleLayout;
				if (format is null || format.IsDisposed)
				{
					format = factory.CreateTextFormat(_fontName, 14);
					format.ParagraphAlignment = DWriteParagraphAlignment.Center;
					format.TextAlignment = DWriteTextAlignment.Center;
					format.WordWrapping = DWriteWordWrapping.NoWrap;
				}
				titleLayout = factory.CreateTextLayout(_title ?? string.Empty, format);
				format.Dispose();
				titleLayout.MaxWidth = titleLayout.GetMetrics().Width + UIConstants.ElementMarginDouble;
				titleLayout.MaxHeight = InterlockedHelper.Read(ref _titleHeight);
			}
			if ((flags & RenderObjectUpdateFlags.Text) == RenderObjectUpdateFlags.Text)
			{
				DWriteTextFormat format = textLayout;
				if (format is null || format.IsDisposed)
				{
					format = factory.CreateTextFormat(_fontName, 14);
					format.SetLineSpacing(DWriteLineSpacingMethod.Uniform, 20, 16);
					format.ParagraphAlignment = DWriteParagraphAlignment.Near;
					format.TextAlignment = DWriteTextAlignment.Leading;
					format.WordWrapping = DWriteWordWrapping.EmergencyBreak;
				}
				textLayout = factory.CreateTextLayout(_text ?? string.Empty, format);
				format.Dispose();
			}
		}

		public override void Render(DirtyAreaCollector collector) => Render(collector, markDirty: false);

		protected override bool RenderCore(DirtyAreaCollector collector)
		{
			IRenderer renderer = Renderer;
			RenderObjectUpdateFlags flags = (RenderObjectUpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L);
			GetLayouts(flags, out DWriteTextLayout titleLayout, out DWriteTextLayout textLayout);
			D2D1DeviceContext context = renderer.GetDeviceContext();
			D2D1Brush[] brushes = _brushes;
			D2D1Brush backBrush = brushes[(int)Brush.BackBrush];
			D2D1Brush textBrush = brushes[(int)Brush.TextBrush];
			if (flags == RenderObjectUpdateFlags.Text)
			{
				RenderText(context, collector, backBrush, textBrush, textLayout, justText: true);
				DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
				DisposeHelper.NullSwapOrDispose(ref _textLayout, textLayout);
				return true;
			}
			Rect bounds = Bounds;
			float lineWidth = renderer.GetBaseLineWidth();
			context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(new Rect(bounds.X, bounds.Y + MathI.Floor(titleLayout.MaxHeight * 0.5f),
				bounds.Right, bounds.Bottom), lineWidth), brushes[(int)Brush.BorderBrush], lineWidth);
			RenderTitle(context, backBrush, textBrush, titleLayout, bounds);
			RenderText(context, collector, backBrush, textBrush, textLayout, justText: false);
			DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
			DisposeHelper.NullSwapOrDispose(ref _textLayout, textLayout);
			return true;
		}

		[Inline(InlineBehavior.Remove)]
		private void RenderTitle(D2D1DeviceContext context, D2D1Brush backBrush, D2D1Brush textBrush, DWriteTextLayout layout, in Rect bounds)
		{
			if (layout is null)
				return;
			PointF titleLoc = new PointF(bounds.X + UIConstants.ElementMargin, bounds.Y);
			context.PushAxisAlignedClip(RectF.FromXYWH(titleLoc.X, titleLoc.Y, layout.MaxWidth, layout.MaxHeight), D2D1AntialiasMode.Aliased);
			RenderBackground(context, backBrush);
			context.DrawTextLayout(titleLoc, layout, textBrush, D2D1DrawTextOptions.Clip);
			context.PopAxisAlignedClip();
		}

		[Inline(InlineBehavior.Remove)]
		private void RenderText(D2D1DeviceContext context, DirtyAreaCollector collector, D2D1Brush backBrush, D2D1Brush textBrush,
			DWriteTextLayout layout, [InlineParameter] bool justText)
		{
			if (layout is null)
				return;
			Rect bounds = Bounds;
			Point textLoc = GetTextLocation();
			int textBoundRight = bounds.Right - UIConstants.ElementMarginDouble;
			int textBoundBottom = bounds.Bottom - UIConstants.ElementMarginDouble;
			Rect textRect = Rect.FromXYWH(textLoc.X, textLoc.Y, textBoundRight, textBoundBottom);
			context.PushAxisAlignedClip((RectF)textRect, D2D1AntialiasMode.Aliased);
			layout.MaxWidth = textBoundRight - textLoc.X;
			if (justText)
			{
				RenderBackground(context, backBrush);
				collector.MarkAsDirty(textRect);
			}
			context.DrawTextLayout(textLoc, layout, textBrush, D2D1DrawTextOptions.None);
			context.PopAxisAlignedClip();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetContentBaseX() => GetContentBaseXCore(Location.X);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetContentBaseY() => GetContentBaseYCore(Location.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Point GetContentLocation()
		{
			Point location = Location;
			return new Point(GetContentBaseXCore(location.X), GetContentBaseYCore(location.Y));
		}

		[Inline(InlineBehavior.Remove)]
		private Point GetTextLocation()
		{
			Point location = Location;
			return new Point(GetContentBaseXCore(location.X), GetTextBaseYCore(location.Y));
		}

		[Inline(InlineBehavior.Remove)]
		private static int GetContentBaseXCore(int x) => x + UIConstants.ElementMarginDouble;

		[Inline(InlineBehavior.Remove)]
		private int GetContentBaseYCore(int y) => GetTextBaseYCore(y) + UIConstants.ElementMargin;

		[Inline(InlineBehavior.Remove)]
		private int GetTextBaseYCore(int y) => y + InterlockedHelper.Read(ref _titleHeight);

		~GroupBox()
		{
			DisposeCore(disposing: false);
		}

		public void Dispose()
		{
			DisposeCore(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void DisposeCore(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;
			if (disposing)
			{
				DisposeHelper.SwapDispose(ref _titleLayout);
				DisposeHelper.SwapDispose(ref _textLayout);
			}
			DisposeChildren(disposing);
		}

		[Inline(InlineBehavior.Remove)]
		private void DisposeChildren(bool disposing)
		{
			IList<UIElement> children = _children.GetUnderlyingList();
			if (disposing)
			{
				int count = children.Count;
				if (children is UnwrappableList<UIElement> castedList)
				{
					UIElement[] childrenArray = castedList.Unwrap();
					for (int i = 0; i < count; i++)
						(childrenArray[i] as IDisposable)?.Dispose();
				}
				else
				{
					foreach (UIElement child in children)
						(child as IDisposable)?.Dispose();
				}
			}
			children.Clear();
		}
	}
}
