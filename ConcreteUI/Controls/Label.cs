using System;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
	public sealed partial class Label : UIElement, IDisposable
	{
		private static readonly string[] _brushNames = new string[(int)Brush._Last]
		{
			"fore"
		}.WithPrefix("app.label.").ToLowerAscii();

		private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

		private DWriteTextFormat _format;
		private DWriteTextLayout _layout;
		private string _fontName, _text;
		private TextAlignment _alignment;
		private long _rawUpdateFlags;
		private float _fontSize;
		private bool _wordWrap;

		public Label(IRenderer renderer) : base(renderer)
		{
			_fontSize = 14;
			_alignment = TextAlignment.MiddleLeft;
			_rawUpdateFlags = -1L;
			_layout = null;
		}

		protected override void ApplyThemeCore(ThemeResourceProvider provider)
		{
			UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
			_fontName = provider.FontName;
			Update(RenderObjectUpdateFlags.FormatAndLayout);
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
				format = TextFormatUtils.CreateTextFormat(_alignment, _fontName, _fontSize);
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
			IRenderer renderer = Renderer;
			D2D1DeviceContext deviceContext = renderer.GetDeviceContext();
			D2D1Brush foreBrush = _brushes[(int)Brush.ForeBrush];
			RenderBackground(deviceContext);
			Rect bounds = Bounds;
			DWriteTextLayout layout = GetTextLayout();
			if (layout is null)
				return true;
			layout.MaxWidth = bounds.Width;
			layout.MaxHeight = bounds.Height;
			layout.WordWrapping = _wordWrap ? DWriteWordWrapping.EmergencyBreak : DWriteWordWrapping.NoWrap;
			deviceContext.DrawTextLayout(bounds.Location, layout, foreBrush);
			return true;
		}

		public void Dispose()
		{
			_format?.Dispose();
		}
	}
}
