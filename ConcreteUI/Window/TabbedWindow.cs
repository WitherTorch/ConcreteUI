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
        private DWriteTextLayout[]? _menuBarButtonLayouts;
        private Rectangle[]? _menuBarButtonRects;
        private int _menuBarButtonLastRight;
        #endregion

        #region Rendering Fields
        protected BitVector64 MenuBarButtonStatus, MenuBarButtonChangedStatus;
        #endregion

        #region Properties
        public override int PageCount => _menuTitles.Length;

        public Rectangle[] MenuBarButtonBounds => NullSafetyHelper.ThrowIfNull(InterlockedHelper.Read(ref _menuBarButtonRects));
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
            int pageCount = PageCount;
            if (pageCount > 0)
            {
                float clientY = clientPoint.Y;
                Rectangle[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
                if (menuBarButtonRects is null || menuBarButtonRects.Length != pageCount)
                    return HitTestValue.NoWhere;
                Rect templateMenuBarButtonRect = UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(menuBarButtonRects), pageCount - 1);
                if (clientY <= templateMenuBarButtonRect.Bottom && clientY > _titleBarRect.Bottom)
                    return HitTestValue.Caption;
            }
            return HitTestValue.NoWhere;
        }

        protected override void RecalculateLayout(Size windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            int pageCount = PageCount;
            if (pageCount <= 0)
                return;
            Rectangle[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null || menuBarButtonRects.Length != pageCount)
                return;
            ref Rectangle menuBarButtonRectRef = ref UnsafeHelper.GetArrayDataReference(menuBarButtonRects);
            Rect pageRect = _pageRect;
            int x, y;
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
            for (int i = 0; i < pageCount; i++)
            {
                ref Rectangle rectRef = ref UnsafeHelper.AddTypedOffset(ref menuBarButtonRectRef, i);
                rectRef = new Rectangle(x, y, rectRef.Width, rectRef.Height);
                x = rectRef.Right;
            }
            _menuBarButtonLastRight = x;
            pageRect.Top = y + menuBarButtonRectRef.Height;
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
            GenerateMenu(_menuTitles, provider.FontName, baseX: 0, baseY: 27, menuExtraWidth: UIConstants.ElementMarginDouble,
                out Rectangle[] menuBarButtonRects, out DWriteTextLayout[] menuBarButtonLayouts);
            InterlockedHelper.Write(ref _menuBarButtonRects, menuBarButtonRects);
            DisposeHelper.SwapDispose(ref _menuBarButtonLayouts, menuBarButtonLayouts);
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            BitVector64 MenuBarButtonStatus = this.MenuBarButtonStatus;
            BitVector64 MenuBarButtonChangedStatus = this.MenuBarButtonChangedStatus;
            base.RenderTitle(deviceContext, collector, force);
            #region 繪製主選單
            int pageCount = PageCount;
            if (pageCount <= 0)
                return;
            Rectangle[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null || menuBarButtonRects.Length != pageCount)
                return;
            DWriteTextLayout[]? menuBarButtonLayouts = Interlocked.Exchange(ref _menuBarButtonLayouts, null);
            if (menuBarButtonLayouts is null)
                return;
            if (menuBarButtonLayouts.Length != pageCount)
            {
                DisposeHelper.DisposeAll(menuBarButtonLayouts);
                return;
            }
            D2D1ColorF clearDCColor = _clearDCColor;
            ref Rectangle menuBarButtonRectRef = ref UnsafeHelper.GetArrayDataReference(menuBarButtonRects);
            ref DWriteTextLayout menuBarButtonLayoutRef = ref UnsafeHelper.GetArrayDataReference(menuBarButtonLayouts);
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            D2D1Brush titleBackBrush = GetBrush(CoreWindow.Brush.TitleBackBrush);
            D2D1Brush menuBackBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuBackBrush);
            D2D1Brush menuForeBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuForeBrush);
            D2D1Brush menuSelectBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuSelectBrush);
            D2D1Brush menuHoverBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuHoverBrush);
            D2D1Brush menuHoverForeBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MenuHoverForeBrush);
            Vector2 pixelsPerPoint = PixelsPerPoint;
            float actualBottom = RenderingHelper.RoundInPixel(_pageRect.Top, pixelsPerPoint.Y);
            if (force)
            {
                Rect firstRect = menuBarButtonRectRef;
                RectF menuBarRect = RenderingHelper.RoundInPixel(
                    new RectF(firstRect.X, firstRect.Top, _menuBarButtonLastRight, firstRect.Bottom),
                    pixelsPerPoint);
                menuBarRect.Bottom = actualBottom;
                deviceContext.PushAxisAlignedClip(menuBarRect, D2D1AntialiasMode.Aliased);
                if (WindowMaterial != WindowMaterial.Integrated)
                    GraphicsUtils.ClearAndFill(deviceContext, menuBackBrush, clearDCColor);
                else
                {
                    GraphicsUtils.ClearAndFill(deviceContext, titleBackBrush, clearDCColor);
                    deviceContext.FillRectangle(menuBarRect, menuBackBrush);
                }
                deviceContext.PopAxisAlignedClip();
            }
            bool menuRedraw = isPageChanged || force;
            for (int i = 0, currentPageIndex = CurrentPage; i < pageCount; i++)
            {
                RectF rect = RenderingHelper.RoundInPixel(
                    UnsafeHelper.AddTypedOffset(ref menuBarButtonRectRef, i),
                    pixelsPerPoint);
                rect.Bottom = actualBottom;
                DWriteTextLayout layout = UnsafeHelper.AddTypedOffset(ref menuBarButtonLayoutRef, i);
                bool isSelected = currentPageIndex == i;
                if (isSelected)
                {
                    deviceContext.PushAxisAlignedClip(rect, D2D1AntialiasMode.Aliased);
                    if (!force)
                        GraphicsUtils.ClearAndFill(deviceContext, menuBackBrush, clearDCColor);
                    deviceContext.FillRectangle(rect, menuSelectBrush);
                    deviceContext.DrawTextLayout(rect.Location, layout, menuHoverForeBrush, D2D1DrawTextOptions.None);
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
                        deviceContext.DrawTextLayout(rect.Location, layout, menuHoverForeBrush, D2D1DrawTextOptions.None);
                    }
                    else
                    {
                        deviceContext.DrawTextLayout(rect.Location, layout, menuForeBrush, D2D1DrawTextOptions.None);
                    }
                    deviceContext.PopAxisAlignedClip();
                    collector.MarkAsDirty(rect);
                }
            }
            DisposeHelper.NullSwapOrDispose(ref _menuBarButtonLayouts, menuBarButtonLayouts);
            #endregion
        }

        protected override void OnMouseDown(ref HandleableMouseEventArgs args)
        {
            MouseButtons buttons = args.Buttons;
            if (buttons.HasFlagFast(MouseButtons.LeftButton))
            {
                int pageCount = PageCount;
                if (pageCount > 0)
                {
                    Rectangle[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
                    if (menuBarButtonRects is not null && menuBarButtonRects.Length == pageCount)
                    {
                        PointF location = args.Location;
                        ref Rectangle menuBarButtonRectRef = ref UnsafeHelper.GetArrayDataReference(menuBarButtonRects);
                        for (int i = 0; i < pageCount; i++)
                        {
                            if (UnsafeHelper.AddTypedOffset(ref menuBarButtonRectRef, i).Contains(location))
                            {
                                CurrentPage = i;
                                return;
                            }
                        }
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
            int pageCount = PageCount;
            Rectangle[]? menuBarButtonRects = InterlockedHelper.Read(ref _menuBarButtonRects);
            if (menuBarButtonRects is null || menuBarButtonRects.Length != pageCount)
                return false;
            bool result = false;
            ref Rectangle menuBarButtonRectRef = ref UnsafeHelper.GetArrayDataReference(menuBarButtonRects);
            Rect firstRect = menuBarButtonRectRef;
            Rect menuBarRect = new Rect(firstRect.X, firstRect.Y, _menuBarButtonLastRight, _pageRect.Top);
            BitVector64 templateVector = MenuBarButtonStatus, operateVector = templateVector & ~((1UL << pageCount) - 1);
            BitVector64 changeVector = ReferenceHelper.Exchange(ref MenuBarButtonChangedStatus, default);
            if (changeVector > 0)
                requireUpdate = true;
            if (menuBarRect.Contains(point))
            {
                result = true;
                for (int i = 0; i < pageCount; i++)
                {
                    if (!UnsafeHelper.AddTypedOffset(ref menuBarButtonRectRef, i).Contains(point))
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

        protected void GenerateMenu(string[] menuButtonTexts, string fontName, int baseX, int baseY, int menuExtraWidth,
            out Rectangle[] menuButtonRects, out DWriteTextLayout[] menuButtonLayouts)
        {
            int count = menuButtonTexts.Length;
            using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, UIConstants.MenuFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            format.TextAlignment = DWriteTextAlignment.Center;
            menuButtonLayouts = new DWriteTextLayout[count];
            menuButtonRects = new Rectangle[count];

            ref string menuButtonTextRef = ref UnsafeHelper.GetArrayDataReference(menuButtonTexts);
            ref DWriteTextLayout menuButtonLayoutRef = ref UnsafeHelper.GetArrayDataReference(menuButtonLayouts);
            ref Rectangle menuButtonRectRef = ref UnsafeHelper.GetArrayDataReference(menuButtonRects);
            int menuHeight = 0;
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = GraphicsUtils.CreateCustomTextLayout(
                    UnsafeHelper.AddTypedOffset(ref menuButtonTextRef, i),
                    format, menuExtraWidth, float.PositiveInfinity);
                menuHeight = MathHelper.Max(menuHeight, MathI.Ceiling(layout.MaxHeight));
                UnsafeHelper.AddTypedOffset(ref menuButtonLayoutRef, i) = layout;
            }
            menuHeight += UIConstants.ElementMargin;
            for (int i = 0, menuX = baseX; i < count; i++)
            {
                DWriteTextLayout layout = UnsafeHelper.AddTypedOffset(ref menuButtonLayoutRef, i);
                layout.MaxHeight = menuHeight;

                int width = MathI.Ceiling(layout.MaxWidth);
                layout.MaxWidth = width;
                UnsafeHelper.AddTypedOffset(ref menuButtonRectRef, i) = new Rectangle(menuX, baseY, width, menuHeight);
                menuX += width;
            }
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
                DisposeHelper.SwapDisposeInterlocked(ref _menuBarButtonLayouts);
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
