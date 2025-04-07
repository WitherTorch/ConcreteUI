using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Native;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

using static ConcreteUI.Utils.StaticResources;

namespace ConcreteUI.Window
{
    public abstract class WizardWindow : PagedWindow
    {
        #region Enums
        protected new enum Brush
        {
            WizardTitleBrush,
            WizardSubtitleBrush,
            _Last
        }
        #endregion

        #region Static Fields
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "app.control.fore",
            "app.control.fore.description"
        }.ToLowerAscii();
        #endregion

        #region Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly DWriteTextFormat titleFormat, secondTitleFormat;
        private DWriteTextLayout _titleLayout;
        private DWriteTextLayout _secondTitleLayout;
        private string _title, _secondTitle;
        private D2D1ColorF _wizardBaseColor;
        private PointF titleLocation, secondTitleLocation;
        private bool _titleChanged;
        #endregion

        #region Constructor
        protected WizardWindow(CoreWindow parent) : base(parent)
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowTitle = WindowMaterial == WindowMaterial.Integrated;
            string capFontName = CaptionFontFamilyName;
            DWriteFactory writeFactory = SharedResources.DWriteFactory;
            DWriteTextFormat format;
            titleFormat = format = writeFactory.CreateTextFormat(capFontName, 22);
            format.WordWrapping = DWriteWordWrapping.Wrap;
            secondTitleFormat = format = writeFactory.CreateTextFormat(capFontName, 14);
            format.WordWrapping = DWriteWordWrapping.Wrap;
        }
        #endregion

        #region Properties
        public string Title
        {
            get => _title;
            protected set
            {
                _title = value;
                _titleChanged = true;
                DisposeHelper.SwapDispose(ref _titleLayout,
                    SharedResources.DWriteFactory.CreateTextLayout(value, titleFormat, MathHelper.Max(_pageRect.Width, 0), float.PositiveInfinity));
                TriggerResize();
                Update();
            }
        }

        public string TitleDescription
        {
            get => _secondTitle;
            protected set
            {
                _secondTitle = value;
                _titleChanged = true;
                DisposeHelper.SwapDispose(ref _secondTitleLayout,
                    SharedResources.DWriteFactory.CreateTextLayout(value, secondTitleFormat, MathHelper.Max(_pageRect.Width - UIConstants.WizardSubtitleLeftMargin, 0), float.PositiveInfinity));
                TriggerResize();
                Update();
            }
        }
        #endregion

        #region Override Methods

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            _wizardBaseColor = provider.TryGetColor(ThemeConstants.WizardWindowBaseColor, out D2D1ColorF color) ? color : default;
        }

        public override void RenderElementBackground(UIElement element, D2D1DeviceContext context)
        {
            ClearDC(context);
        }

        protected override void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect)
        {
            ClearDC(deviceContext);
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            base.RenderTitle(deviceContext, collector, force);
            if (_titleChanged || force)
            {
                _titleChanged = false;
                D2D1Brush[] brushes = _brushes;
                RectF rect;
                if (WindowMaterial == WindowMaterial.Integrated)
                {
                    SizeF clientSize = ClientSize;
                    RectF pageRect = _pageRect;
                    rect = new RectF(0, 0, clientSize.Width, pageRect.Top);
                }
                else
                {
                    RectF titleBarRect = _titleBarRect;
                    RectF pageRect = _pageRect;
                    rect = new RectF(titleBarRect.Left, titleBarRect.Bottom, titleBarRect.Right, pageRect.Top);
                }
                rect = GraphicsUtils.AdjustRectangleF(rect);
                deviceContext.PushAxisAlignedClip(rect, D2D1AntialiasMode.Aliased);
                ClearDC(deviceContext);
                DWriteTextLayout layout = Interlocked.Exchange(ref _titleLayout, null);
                if (layout is not null && !layout.IsDisposed)
                {
                    deviceContext.DrawTextLayout(titleLocation, layout, brushes[(int)Brush.WizardTitleBrush], D2D1DrawTextOptions.None);
                    DWriteTextLayout oldLayout = Interlocked.CompareExchange(ref _titleLayout, layout, null);
                    if (oldLayout is not null && !ReferenceEquals(oldLayout, layout))
                        layout.Dispose();
                }
                layout = Interlocked.Exchange(ref _secondTitleLayout, null);
                if (layout is not null && !layout.IsDisposed)
                {
                    deviceContext.DrawTextLayout(secondTitleLocation, layout, brushes[(int)Brush.WizardSubtitleBrush], D2D1DrawTextOptions.None);
                    DWriteTextLayout oldLayout = Interlocked.CompareExchange(ref _secondTitleLayout, layout, null);
                    if (oldLayout is not null && !ReferenceEquals(oldLayout, layout))
                        layout.Dispose();
                }
                deviceContext.PopAxisAlignedClip();
                collector.MarkAsDirty(GraphicsUtils.ConvertRectangle(rect));
            }
        }

        protected override void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            RectF pageRect = _pageRect;
            pageRect.X += UIConstants.WizardLeftPadding;
            pageRect.Y += UIConstants.WizardPadding;
            pageRect.Right -= UIConstants.WizardPadding;
            pageRect.Bottom -= UIConstants.WizardPadding;
            titleLocation = pageRect.Location;
            DWriteTextLayout layout = _titleLayout;
            if (layout is not null)
            {
                layout.MaxWidth = pageRect.Width;
                secondTitleLocation = new PointF(pageRect.X + UIConstants.WizardSubtitleLeftMargin,
                    MathF.Ceiling(pageRect.Y + layout.GetMetrics().Height + UIConstants.WizardSubtitleMargin));
                layout = _secondTitleLayout;
                if (layout is not null)
                {
                    _secondTitleLayout.MaxWidth = pageRect.Width - UIConstants.WizardSubtitleLeftMargin;
                    pageRect.Top = secondTitleLocation.Y + _secondTitleLayout.GetMetrics().Height;
                }
                else
                {
                    pageRect.Top = secondTitleLocation.Y;
                }
            }
            _pageRect = pageRect = GraphicsUtils.AdjustRectangleF(pageRect);
            if (callRecalculatePageLayout && pageRect.IsValid)
            {
                RecalculatePageLayout((Rect)pageRect);
            }
        }

        protected override HitTestValue CustomHitTest(in PointF clientPoint)
        {
            HitTestValue result = base.CustomHitTest(clientPoint);
            if (result == HitTestValue.NoWhere)
                return _pageRect.Contains(clientPoint) ? HitTestValue.Client : HitTestValue.Caption;
            return result;
        }
        #endregion

        #region Inline Methods
        [Inline(InlineBehavior.Keep, export: true)]
        public void ClearDC(D2D1DeviceContext deviceContext)
        {
            deviceContext.Clear(_wizardBaseColor);
        }

        protected override void ClearDCForTitle(D2D1DeviceContext deviceContext)
        {
            if (WindowMaterial == WindowMaterial.Integrated)
                ClearDC(deviceContext);
            else
                deviceContext.Clear(_clearDCColor);
        }
        #endregion

        protected D2D1Brush GetBrush(Brush brush) => _brushes[(int)brush];

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            titleFormat?.Dispose();
            secondTitleFormat?.Dispose();
        }
    }
}
