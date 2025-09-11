using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Controls;
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
        private string _title, _titleDescription;
        private long _updateFlags = -1L;
        private D2D1ColorF _wizardBaseColor;
        private PointF _titleLocation, _titleDescriptionLocation;
        #endregion

        #region Constructor
        protected WizardWindow(CoreWindow parent, bool passParentToUnderlyingWindow = true) : base(parent, passParentToUnderlyingWindow)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            ShowTitle = WindowMaterial == WindowMaterial.Integrated;
            _title = string.Empty;
            _titleDescription = string.Empty;
        }
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
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            _wizardBaseColor = provider.TryGetColor(ThemeConstants.WizardWindowBaseColor, out D2D1ColorF color) ? color : default;
            DisposeHelper.SwapDisposeInterlocked(ref _titleLayout, null);
            DisposeHelper.SwapDisposeInterlocked(ref _titleDescriptionLayout, null);
            Interlocked.Exchange(ref _updateFlags, -1L);
        }

        public override void RenderElementBackground(UIElement element, D2D1DeviceContext context)
        {
            ClearDC(context);
        }

        protected override void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect)
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
                rect = new RectF(pageRect.Left, titleBarRect.Bottom, pageRect.Right, pageRect.Top);
            }
            deviceContext.PushAxisAlignedClip(rect, D2D1AntialiasMode.Aliased);
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
            deviceContext.PopAxisAlignedClip();
            collector.MarkAsDirty(GraphicsUtils.ConvertRectangle(rect));
        }

        protected override void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            RectF pageRect = _pageRect;
            pageRect.Left += UIConstantsPrivate.WizardLeftPadding;
            pageRect.Top += UIConstantsPrivate.WizardPadding;
            pageRect.Right -= UIConstantsPrivate.WizardPadding;
            pageRect.Bottom -= UIConstantsPrivate.WizardPadding;
            _titleLocation = pageRect.Location;
            GetLayouts((UpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L), out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout);
            if (titleLayout is not null)
            {
                titleLayout.MaxWidth = pageRect.Width;
                float descriptionLocY = MathF.Ceiling(pageRect.Y + titleLayout.GetMetrics().Height + UIConstantsPrivate.WizardSubtitleMargin);
                if (titleDescriptionLayout is null)
                    pageRect.Top = descriptionLocY;
                else
                {
                    _titleDescriptionLocation = new PointF(pageRect.X + UIConstantsPrivate.WizardSubtitleLeftMargin, descriptionLocY);
                    titleDescriptionLayout.MaxWidth = pageRect.Width - UIConstantsPrivate.WizardSubtitleLeftMargin;
                    pageRect.Top = descriptionLocY + titleDescriptionLayout.GetMetrics().Height;
                    DisposeHelper.NullSwapOrDispose(ref _titleDescriptionLayout, titleDescriptionLayout);
                }
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            }

            _pageRect = pageRect = GraphicsUtils.AdjustRectangleF(pageRect);
            if (callRecalculatePageLayout && pageRect.IsValid)
            {
                RecalculatePageLayout((Rect)pageRect);
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
