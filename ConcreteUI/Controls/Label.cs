using System;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
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

		private DWriteTextLayout _layout;
		private string _fontName, _text;
		private TextAlignment _alignment;
		private long _rawUpdateFlags;
		private float _fontSize;
		private bool _wordWrap;

		public Label(IRenderer renderer) : base(renderer)
		{
			_fontSize = UIConstants.DefaultFontSize;
			_alignment = TextAlignment.MiddleLeft;
			_rawUpdateFlags = -1L;
			_layout = null;
		}

        [Inline(InlineBehavior.Keep, export: true)]
        public Label WithAutoWidthCalculation(int minHeight = -1, int maxHeight = -1)
        {
            WidthCalculation = new AutoWidthCalculation(this, minHeight, maxHeight);
            return this;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public Label WithAutoHeightCalculation(int minHeight = -1, int maxHeight = -1)
        {
            HeightCalculation = new AutoHeightCalculation(this, minHeight, maxHeight);
            return this;
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
		{
			UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
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
        private DWriteTextLayout GetTextLayout(RenderObjectUpdateFlags flags)
        {
            DWriteTextLayout layout = Interlocked.Exchange(ref _layout, null);

            if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
            {
                DWriteTextFormat format = layout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatUtils.CreateTextFormat(_alignment, _fontName, _fontSize);
                string text = _text;
                if (string.IsNullOrEmpty(text))
                    layout = null;
                else
                    layout = SharedResources.DWriteFactory.CreateTextLayout(text, format);
                format.Dispose();
            }
            return layout;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckFormatIsNotAvailable(DWriteTextFormat format, RenderObjectUpdateFlags flags)
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
			IRenderer renderer = Renderer;
            DWriteTextLayout layout = GetTextLayout(GetAndCleanRenderObjectUpdateFlags());
            D2D1DeviceContext deviceContext = renderer.GetDeviceContext();
			D2D1Brush foreBrush = _brushes[(int)Brush.ForeBrush];
			RenderBackground(deviceContext);
			Rect bounds = Bounds;
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
            DisposeHelper.SwapDisposeInterlocked(ref _layout);
        }
    }
}
