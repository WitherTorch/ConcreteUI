using System;
using System.Drawing;
using System.Numerics;
using System.Threading;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Window
{
    public abstract class TabbedWindow : PagedWindow
    {
        #region Enums
        protected new enum Brush : uint
        {
            MenuBackBrush,
            MenuForeBrush,
            MenuSelectBrush,
            MenuHoverBrush,
            MenuHoverForeBrush,
            _Last
        }
        #endregion

        #region Static Fields
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "app.menu.back",
            "app.menu.fore",
            "app.menu.itemSelected.back",
            "app.menu.itemHovered.back",
            "app.menu.fore.active",
        }.ToLowerAscii();
        #endregion

        #region Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly string[] _menuTitles;
        private DWriteTextLayout[]? menuBarButtonLayouts;
        private float menuBarButtonLastRight;
        private Rect[]? _menuBarButtonRects;
        #endregion

        #region Rendering Fields
        protected BitVector64 MenuBarButtonStatus, MenuBarButtonChangedStatus;
        #endregion

        #region Properties
        public override int PageCount => _menuTitles.Length;

        public Rect[] MenuBarButtonBounds => NullSafetyHelper.ThrowIfNull(InterlockedHelper.Read(ref _menuBarButtonRects));
        #endregion

        #region Constructor
        protected TabbedWindow(string[] menuTitles) : base() => _menuTitles = menuTitles;

        protected TabbedWindow(GraphicsDeviceProvider? deviceProvider, string[] menuTitles) : base(deviceProvider) => _menuTitles = menuTitles;

        protected TabbedWindow(CoreWindow? parent, string[] menuTitles, bool passParentToUnderlyingWindow = false) : base(parent, passParentToUnderlyingWindow) => _menuTitles = menuTitles;
        #endregion

        #region Override Methods
        protected override HitTestValue CustomHitTest(PointF clientPoint)
        {
            HitTestValue result = base.CustomHitTest(clientPoint);
            if (result != HitTestValue.NoWhere && result != HitTestValue.Client)
            {
                ulong val = MenuBarButtonStatus.Exchange(0UL);
                if (val > 0UL)
                {
                    MenuBarButtonChangedStatus |= val;
                    Update();
                }
                return result;
            }
            if (MousePositionChangedForMenuBar(clientPoint, false))
                return HitTestValue.Client;
            float clientY = clientPoint.Y;
            Rect[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null)
                return HitTestValue.NoWhere;
            Rect templateMenuBarButtonRect = menuBarButtonRects.LastOrDefault();
            if (clientY <= templateMenuBarButtonRect.Bottom && clientY > _titleBarRect.Bottom)
                return HitTestValue.Caption;
            return HitTestValue.NoWhere;
        }

        protected override void RecalculateLayout(Size windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            Rect[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null)
                return;
            Rect pageRect = _pageRect;
            int x, y;
            Vector2 pixelsPerPoint = PixelsPerPoint;
            if (WindowMaterial == WindowMaterial.Integrated)
            {
                x = 0;
                y = 0;
            }
            else
            {
                x = pageRect.Left;
                y = _titleBarRect.Height + _drawingOffsetY;
            }
            for (int i = 0, count = menuBarButtonRects.Length; i < count; i++)
            {
                Rect rect = menuBarButtonRects[i];
                int width = rect.Width;
                int height = rect.Height;
                rect.Left = x;
                rect.Top = y;
                x = rect.Right = x + width;
                rect.Bottom = y + height;
                menuBarButtonRects[i] = rect;
            }
            menuBarButtonLastRight = x;
            pageRect.Top = y + menuBarButtonRects.FirstOrDefault().Height;
            _pageRect = pageRect;
            if (callRecalculatePageLayout && pageRect.IsValid)
            {
                RecalculatePageLayout(pageRect);
            }
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
            GenerateMenu(_menuTitles, baseX: 0, baseY: 27, menuExtraWidth: UIConstants.ElementMarginDouble,
                out Rect[] menuBarButtonRects, out DWriteTextLayout[] menuBarButtonLayouts);
            _menuBarButtonRects = menuBarButtonRects;
            DisposeHelper.SwapDispose(ref this.menuBarButtonLayouts, menuBarButtonLayouts);
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            BitVector64 MenuBarButtonStatus = this.MenuBarButtonStatus;
            BitVector64 MenuBarButtonChangedStatus = this.MenuBarButtonChangedStatus;
            base.RenderTitle(deviceContext, collector, force);
            #region 繪製主選單
            Rect[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null)
                return;
            DWriteTextLayout[]? menuBarButtonLayouts = Interlocked.Exchange(ref this.menuBarButtonLayouts, null);
            if (menuBarButtonLayouts is null)
                return;
            D2D1ColorF clearDCColor = _clearDCColor;
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            D2D1Brush titleBackBrush = GetBrush(CoreWindow.Brush.TitleBackBrush);
            D2D1Brush menuBackBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuBackBrush);
            D2D1Brush menuForeBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuForeBrush);
            D2D1Brush menuSelectBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuSelectBrush);
            D2D1Brush menuHoverBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuHoverBrush);
            D2D1Brush menuHoverForeBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuHoverForeBrush);
            Vector2 pixelsPerPoint = PixelsPerPoint;
            if (force)
            {
                Rect firstRect = menuBarButtonRects.FirstOrDefault();
                Rect lastRect = menuBarButtonRects.LastOrDefault();
                RectF menuBarRect = RenderingHelper.RoundInPixel(new RectF(firstRect.X, firstRect.Top, lastRect.Right, lastRect.Bottom), pixelsPerPoint);
                deviceContext.PushAxisAlignedClip(menuBarRect, D2D1AntialiasMode.Aliased);
                if (WindowMaterial != WindowMaterial.Integrated)
                    GraphicsUtils.ClearAndFill(deviceContext, menuBackBrush, clearDCColor);
                else
                {
                    GraphicsUtils.ClearAndFill(deviceContext, titleBackBrush, clearDCColor);
                    menuBarRect.Right = menuBarButtonLastRight;
                    deviceContext.FillRectangle(menuBarRect, menuBackBrush);
                }
                deviceContext.PopAxisAlignedClip();
            }
            bool menuRedraw = isPageChanged || force;
            for (int i = 0, currentPageIndex = CurrentPage, count = menuBarButtonRects.Length; i < count; i++)
            {
                RectF rect = RenderingHelper.RoundInPixel(menuBarButtonRects[i], pixelsPerPoint);
                DWriteTextLayout layout = menuBarButtonLayouts[i];
                bool isSelected = currentPageIndex == i;
                if (isSelected)
                {
                    deviceContext.PushAxisAlignedClip(rect, D2D1AntialiasMode.Aliased);
                    if (!force)
                        GraphicsUtils.ClearAndFill(deviceContext, menuBackBrush, clearDCColor);
                    deviceContext.FillRectangle(rect, menuSelectBrush);
                    deviceContext.DrawTextLayout(rect.Location, layout, menuHoverForeBrush, D2D1DrawTextOptions.Clip);
                    deviceContext.PopAxisAlignedClip();
                    collector.MarkAsDirty(rect);
                }
                else if (MenuBarButtonChangedStatus[i] || menuRedraw)
                {
                    deviceContext.PushAxisAlignedClip(rect, D2D1AntialiasMode.Aliased);
                    if (!force)
                    {
                        GraphicsUtils.ClearAndFill(deviceContext, menuBackBrush, clearDCColor);
                    }
                    if (MenuBarButtonStatus[i])
                    {
                        deviceContext.FillRectangle(rect, menuHoverBrush);
                        deviceContext.DrawTextLayout(rect.Location, layout, menuHoverForeBrush, D2D1DrawTextOptions.Clip);
                    }
                    else
                    {
                        deviceContext.DrawTextLayout(rect.Location, layout, menuForeBrush, D2D1DrawTextOptions.Clip);
                    }
                    deviceContext.PopAxisAlignedClip();
                    collector.MarkAsDirty(rect);
                }
            }
            DisposeHelper.NullSwapOrDispose(ref this.menuBarButtonLayouts, menuBarButtonLayouts);
            #endregion
        }

        protected override void OnMouseDown(ref HandleableMouseEventArgs args)
        {
            MouseButtons buttons = args.Buttons;
            if (buttons.HasFlagFast(MouseButtons.LeftButton))
            {
                Rect[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
                if (menuBarButtonRects is null)
                    return;
                for (int i = 0, count = menuBarButtonRects.Length; i < count; i++)
                {
                    if (menuBarButtonRects[i].Contains(args.Location))
                    {
                        CurrentPage = i;
                        return;
                    }
                }
            }
            base.OnMouseDown(ref args);
            if (args.Handled)
                return;
            if (buttons.HasFlagFast(MouseButtons.XButton2))
            {
                if (!buttons.HasFlagFast(MouseButtons.XButton1))
                {
                    args.Handle();
                    NavigateBackPage(args.Location);
                }
            }
            if (buttons.HasFlagFast(MouseButtons.XButton1))
            {
                if (!buttons.HasFlagFast(MouseButtons.XButton2))
                {
                    args.Handle();
                    NavigateForwardPage(args.Location);
                }
            }
        }

        protected virtual void NavigateBackPage(PointF location)
        {
            int page = CurrentPage - 1;
            CurrentPage = (page < 0) ? _menuTitles.Length - 1 : page;
            return;
        }

        protected virtual void NavigateForwardPage(PointF location)
        {
            int page = CurrentPage + 1;
            int length = _menuTitles.Length;
            CurrentPage = (page >= length) ? 0 : page;
            return;
        }

        protected virtual bool MousePositionChangedForMenuBar(PointF point, bool requireUpdate)
        {
            Rect[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null)
                return false;
            bool result = false;
            int length = menuBarButtonRects.Length;
            Rect firstRect = menuBarButtonRects.FirstOrDefault();
            Rect lastRect = menuBarButtonRects.LastOrDefault();
            Rect menuBarRect = new Rect(firstRect.X, firstRect.Top, lastRect.Right, lastRect.Bottom);
            BitVector64 templateVector = MenuBarButtonStatus, operateVector = templateVector & ~((1UL << length) - 1);
            BitVector64 changeVector = ReferenceHelper.Exchange(ref MenuBarButtonChangedStatus, default);
            if (changeVector > 0)
                requireUpdate = true;
            if (menuBarRect.Contains(point))
            {
                result = true;
                for (int i = 0; i < length; i++)
                {
                    if (!menuBarButtonRects[i].Contains(point))
                        continue;
                    operateVector[i] = true;
                    break;
                }
            }
            templateVector ^= operateVector;
            if (templateVector > 0UL)
            {
                MenuBarButtonStatus = operateVector;
                MenuBarButtonChangedStatus = changeVector | templateVector;
                requireUpdate = true;
            }
            if (requireUpdate)
                Update();
            return result;
        }
        #endregion

        #region Utility Methods
        protected D2D1Brush GetBrush(Brush brush)
        {
            if (brush >= Brush._Last)
                throw new ArgumentOutOfRangeException(nameof(brush));
            return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)brush);
        }

        protected void GenerateMenu(string[] menuButtonTexts, int baseX, int baseY, int menuExtraWidth,
            out Rect[] menuButtonRects, out DWriteTextLayout[] menuButtonLayouts)
        {
            int menuX = baseX;
            Vector2 pixelsPerPoint = PixelsPerPoint;
            int count = menuButtonTexts.Length;
            DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(CurrentTheme).FontName, UIConstants.MenuFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            format.TextAlignment = DWriteTextAlignment.Center;
            menuButtonLayouts = new DWriteTextLayout[count];
            menuButtonRects = new Rect[count];
            int menuHeight = 0;
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = GraphicsUtils.CreateCustomTextLayout(menuButtonTexts[i], format, menuExtraWidth, float.PositiveInfinity);
                int height = MathI.Ceiling(layout.MaxHeight);
                if (menuHeight < height)
                    menuHeight = height;
                menuButtonLayouts[i] = layout;
            }
            menuHeight += UIConstants.ElementMargin;
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = menuButtonLayouts[i];
                layout.MaxHeight = menuHeight;

                int width = MathI.Ceiling(layout.MaxWidth);
                layout.MaxWidth = width;
                menuButtonRects[i] = Rect.FromXYWH(menuX, baseY, width, menuHeight);
                menuX += width;
            }
            format.Dispose();
        }
        #endregion

        #region WndProc
        protected override bool TryProcessSystemWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            switch (message)
            {
                case WindowMessage.NCMouseMove:
                case WindowMessage.NCMouseLeave:
                case WindowMessage.MouseLeave:
                    ulong val = MenuBarButtonStatus.Exchange(0UL);
                    if (val > 0UL)
                    {
                        MenuBarButtonChangedStatus |= val;
                        Update();
                    }
                    goto default;
                default:
                    return base.TryProcessSystemWindowMessage(hwnd, message, wParam, lParam, out result);
            }
        }
        #endregion

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref menuBarButtonLayouts);
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
