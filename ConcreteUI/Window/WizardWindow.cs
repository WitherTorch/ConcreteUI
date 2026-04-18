using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Window
{
    public abstract class WizardWindow : PagedWindow
    {
        #region Enums
        [Flags]
        private enum UpdateFlags : long
        {
            None = 0,
            UpdateTitle = 0b01,
            UpdateTitleDescription = 0b10,
            All = UpdateTitle | UpdateTitleDescription,
        }

        protected new enum Brush
        {
            WizardTitleBrush,
            WizardTitleDescriptionBrush,
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
        private DWriteTextLayout? _titleLayout, _titleDescriptionLayout;
        private string _title = string.Empty, _titleDescription = string.Empty;
        private long _updateFlags = -1L;
        private D2D1ColorF _wizardBaseColor;
        private Point _titleLocation, _titleDescriptionLocation;
        private Rect _widePageRect;
        #endregion

        #region Constructor
        protected WizardWindow() : base() => Initialize();

        protected WizardWindow(GraphicsDeviceProvider? deviceProvider) : base(deviceProvider) => Initialize();

        protected WizardWindow(CoreWindow parent, bool passParentToUnderlyingWindow = true) : base(parent, passParentToUnderlyingWindow) => Initialize();
        #endregion

        #region Properties
        public string? Title
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [return: NotNull]
            get => _title;
            set
            {
                _title = value ?? string.Empty;
                InterlockedHelper.Or(ref _updateFlags, (long)UpdateFlags.UpdateTitle);
                TriggerResize();
            }
        }

        public string? TitleDescription
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [return: NotNull]
            get => _titleDescription;
            protected set
            {
                _titleDescription = value ?? string.Empty;
                InterlockedHelper.Or(ref _updateFlags, (long)UpdateFlags.UpdateTitleDescription);
                TriggerResize();
            }
        }
        #endregion

        #region Override Methods

        protected override CreateWindowInfo GetCreateWindowInfo()
        {
            CreateWindowInfo windowInfo = base.GetCreateWindowInfo();
            windowInfo.Styles = (windowInfo.Styles & WindowStyles.SystemMenu) | WindowStyles.DialogFrame;
            return windowInfo;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
            _wizardBaseColor = provider.TryGetColor(ThemeConstants.WizardWindowBaseColor, out D2D1ColorF color) ? color : default;
            DisposeHelper.SwapDisposeInterlocked(ref _titleLayout, null);
            DisposeHelper.SwapDisposeInterlocked(ref _titleDescriptionLayout, null);
            Interlocked.Exchange(ref _updateFlags, -1L);
        }

        protected override void RenderPageCore(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in Rect pageRect, bool force)
        {
            if (force)
            {
                using RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, RenderingHelper.CeilingInPixel((RectF)_widePageRect, PixelsPerPoint), D2D1AntialiasMode.Aliased);
                ClearDC(deviceContext);
            }
            base.RenderPageCore(deviceContext, collector, pageRect, force);
        }

        public override void RenderBackground(UIElement element, in RegionalRenderingContext context)
        {
            ClearDC(context.DeviceContext);
        }

        protected override void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in Rect pageRect)
        {
            ClearDC(deviceContext);
        }

        private void GetLayouts(UpdateFlags flags, out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout)
        {
            titleLayout = Interlocked.Exchange(ref _titleLayout, null);
            titleDescriptionLayout = Interlocked.Exchange(ref _titleDescriptionLayout, null);
            if ((flags & UpdateFlags.UpdateTitle) == UpdateFlags.UpdateTitle)
            {
                DWriteFactory factory = SharedResources.DWriteFactory;
                DWriteTextFormat? format = titleLayout;
                if (format is null)
                {
                    string fontName = NullSafetyHelper.ThrowIfNull(CurrentTheme).FontName;
                    format = factory.CreateTextFormat(fontName, UIConstants.WizardWindowTitleFontSize);
                    format.WordWrapping = DWriteWordWrapping.Wrap;
                }
                titleLayout = factory.CreateTextLayout(_title, format);
                format.Dispose();
            }
            if ((flags & UpdateFlags.UpdateTitleDescription) == UpdateFlags.UpdateTitleDescription)
            {
                DWriteFactory factory = SharedResources.DWriteFactory;
                DWriteTextFormat? format = titleDescriptionLayout;
                if (format is null)
                {
                    string fontName = NullSafetyHelper.ThrowIfNull(CurrentTheme).FontName;
                    format = factory.CreateTextFormat(fontName, UIConstants.WizardWindowTitleDescriptionFontSize);
                    format.WordWrapping = DWriteWordWrapping.Wrap;
                }
                titleDescriptionLayout = factory.CreateTextLayout(_titleDescription, format);
                format.Dispose();
            }
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            base.RenderTitle(deviceContext, collector, force);
            UpdateFlags flags = (UpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L);
            if (flags == 0L && !force)
                return;
            GetLayouts(flags, out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout);
            D2D1Brush[] brushes = _brushes;
            Rect rect = _widePageRect;
            if (WindowMaterial == WindowMaterial.Integrated)
                rect = new Rect(0, 0, ClientSize.Width, rect.Top);
            else
                rect = new Rect(rect.Left, _titleBarRect.Bottom, rect.Right, rect.Top);
            using RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, RenderingHelper.RoundInPixel(rect, PixelsPerPoint));
            ClearDC(deviceContext);
            if (titleLayout is not null)
            {
                deviceContext.DrawTextLayout(_titleLocation, titleLayout, brushes[(int)Brush.WizardTitleBrush], D2D1DrawTextOptions.None);
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            }
            if (titleDescriptionLayout is not null)
            {
                deviceContext.DrawTextLayout(_titleDescriptionLocation, titleDescriptionLayout, brushes[(int)Brush.WizardTitleDescriptionBrush], D2D1DrawTextOptions.None);
                DisposeHelper.NullSwapOrDispose(ref _titleDescriptionLayout, titleDescriptionLayout);
            }
            collector.MarkAsDirty(scope.ClipRect);
        }

        protected override void RecalculateLayout(Size windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            Rect pageRect = _pageRect;
            Rect widePageRect = pageRect;
            pageRect.Left += UIConstantsPrivate.WizardLeftPadding;
            pageRect.Top += UIConstantsPrivate.WizardPadding;
            pageRect.Right -= UIConstantsPrivate.WizardPadding;
            pageRect.Bottom -= UIConstantsPrivate.WizardPadding;
            _titleLocation = pageRect.Location;
            GetLayouts((UpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L), out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout);
            if (titleLayout is not null)
            {
                titleLayout.MaxWidth = pageRect.Width;
                int descriptionLocY = MathI.Ceiling(pageRect.Y + titleLayout.GetMetrics().Height + UIConstantsPrivate.WizardSubtitleMargin);
                if (titleDescriptionLayout is null)
                    pageRect.Top = descriptionLocY;
                else
                {
                    _titleDescriptionLocation = new Point(pageRect.X + UIConstantsPrivate.WizardSubtitleLeftMargin, descriptionLocY);
                    titleDescriptionLayout.MaxWidth = pageRect.Width - UIConstantsPrivate.WizardSubtitleLeftMargin;
                    pageRect.Top = descriptionLocY + MathI.Ceiling(titleDescriptionLayout.GetMetrics().Height);
                    DisposeHelper.NullSwapOrDispose(ref _titleDescriptionLayout, titleDescriptionLayout);
                }
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            }

            widePageRect.Top = pageRect.Top;
            widePageRect.Bottom = pageRect.Bottom;
            _widePageRect = widePageRect;
            _pageRect = pageRect;
            if (callRecalculatePageLayout && pageRect.IsValid)
            {
                RecalculatePageLayout(pageRect);
            }
        }

        protected override HitTestValue CustomHitTest(PointF clientPoint)
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

        private void Initialize()
        {
            MinimizeBox = false;
            MaximizeBox = false;
            ShowTitle = WindowMaterial == WindowMaterial.Integrated;
        }

        protected D2D1Brush GetBrush(Brush brush) => _brushes[(int)brush];

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
                DisposeHelper.SwapDisposeInterlocked(ref _titleDescriptionLayout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
