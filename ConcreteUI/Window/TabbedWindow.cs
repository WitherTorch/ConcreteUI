﻿using System;
using System.Drawing;
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

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window
{
    public abstract class TabbedWindow : PagedWindow
    {
        #region Enums
        protected new enum Brush
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
            "app.menu.itemSelected.back",
            "app.menu.itemHovered.back",
            "app.menu.fore.active",
            "app.menu.fore"
        }.ToLowerAscii();
        #endregion

        #region Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly string[] menuTitles;
        private DWriteTextLayout[] menuBarButtonLayouts;
        private float menuBarButtonLastRight;
        private RectF[] menuBarButtonRects;
        #endregion

        #region Rendering Fields
        protected BitVector64 MenuBarButtonStatus, MenuBarButtonChangedStatus;
        #endregion

        #region Properties

        public RectF[] MenuBarButtonBounds => menuBarButtonRects;
        #endregion

        #region Constructor
        protected TabbedWindow(CoreWindow parent, string[] menuTitles) : base(parent)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            this.menuTitles = menuTitles;
        }
        #endregion

        #region Override Methods
        protected override HitTestValue CustomHitTest(in PointF clientPoint)
        {
            HitTestValue result = base.CustomHitTest(in clientPoint);
            if (result != HitTestValue.NoWhere && result != HitTestValue.Client)
                return result;
            if (MousePositionChangedForMenuBar(in clientPoint, false))
                return HitTestValue.Client;
            float clientY = clientPoint.Y;
            RectF templateMenuBarButtonRect = menuBarButtonRects.LastOrDefault();
            if (clientY <= templateMenuBarButtonRect.Bottom && clientY > _titleBarRect.Bottom)
                return HitTestValue.Caption;
            return HitTestValue.NoWhere;
        }

        protected override void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            base.RecalculateLayout(windowSize, false);
            RectF pageRect = _pageRect;
            float x, y;
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
            RectF[] menuBarButtonRects = this.menuBarButtonRects;
            for (int i = 0, count = menuBarButtonRects.Length; i < count; i++)
            {
                RectF rect = menuBarButtonRects[i];
                float width = rect.Width;
                float height = rect.Height;
                rect.Left = x;
                rect.Top = y;
                x = rect.Right = x + width;
                rect.Bottom = y + height;
                menuBarButtonRects[i] = rect;
            }
            menuBarButtonLastRight = x;
            pageRect.Top = y + menuBarButtonRects.FirstOrDefault().Height;
            _pageRect = pageRect = GraphicsUtils.AdjustRectangleF(pageRect);
            if (callRecalculatePageLayout && pageRect.IsValid)
            {
                RecalculatePageLayout((Rect)pageRect);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            GenerateMenu(menuTitles, baseX: 0, baseY: 27, menuExtraWidth: 15, menuHeight: 26, out menuBarButtonRects, out menuBarButtonLayouts);
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            BitVector64 MenuBarButtonStatus = this.MenuBarButtonStatus;
            BitVector64 MenuBarButtonChangedStatus = this.MenuBarButtonChangedStatus;
            base.RenderTitle(deviceContext, collector, force);
            #region 繪製主選單
            RectF[] menuBarButtonRects = this.menuBarButtonRects;
            D2D1ColorF clearDCColor = _clearDCColor;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush titleBackBrush = GetBrush(CoreWindow.Brush.TitleBackBrush);
            D2D1Brush menuBackBrush = brushes[(int)Brush.MenuBackBrush];
            D2D1Brush menuForeBrush = brushes[(int)Brush.MenuForeBrush];
            D2D1Brush menuSelectBrush = brushes[(int)Brush.MenuSelectBrush];
            D2D1Brush menuHoverBrush = brushes[(int)Brush.MenuHoverBrush];
            D2D1Brush menuHoverForeBrush = brushes[(int)Brush.MenuHoverForeBrush];
            if (force)
            {
                RectF firstRect = menuBarButtonRects.FirstOrDefault();
                RectF lastRect = menuBarButtonRects.LastOrDefault();
                RectF menuBarRect = new RectF(firstRect.X, firstRect.Top, lastRect.Right, lastRect.Bottom);
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
            DWriteTextLayout[] menuBarButtonLayouts = this.menuBarButtonLayouts;
            for (int i = 0, currentPageIndex = CurrentPage, count = menuBarButtonRects.Length; i < count; i++)
            {
                RectF rect = menuBarButtonRects[i];
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
                    collector.MarkAsDirty(GraphicsUtils.AdjustRectangle(rect));
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
                    collector.MarkAsDirty(GraphicsUtils.AdjustRectangle(rect));
                }
            }
            #endregion
        }

        protected override void OnMouseDownForElements(in MouseInteractEventArgs args)
        {
            RectF[] menuBarButtonRects = this.menuBarButtonRects;
            for (int i = 0, count = menuBarButtonRects.Length; i < count; i++)
            {
                if (menuBarButtonRects[i].Contains(args.Location))
                {
                    CurrentPage = i;
                    return;
                }
            }
            base.OnMouseDownForElements(args);
        }

        protected virtual bool MousePositionChangedForMenuBar(in PointF point, bool requireUpdate)
        {
            bool result = false;
            RectF[] menuBarButtonRects = this.menuBarButtonRects;
            int length = menuBarButtonRects.Length;
            RectF firstRect = menuBarButtonRects.FirstOrDefault();
            RectF lastRect = menuBarButtonRects.LastOrDefault();
            RectF menuBarRect = new RectF(firstRect.X, firstRect.Top, lastRect.Right, lastRect.Bottom);
            BitVector64 templateVector = MenuBarButtonStatus, operateVector = templateVector & ~((1UL << length) - 1);
            BitVector64 changeVector = MenuBarButtonChangedStatus;
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
        protected D2D1Brush GetBrush(Brush brush) => _brushes[(int)brush];

        protected void GenerateMenu(string[] menuButtonTexts, float baseX, float baseY, float menuExtraWidth, float menuHeight,
            out RectF[] menuButtonRects, out DWriteTextLayout[] menuButtonLayouts)
        {
            float menuX = baseX;
            int count = menuButtonTexts.Length;
            DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(Theme.FontName, UIConstants.MenuFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            format.TextAlignment = DWriteTextAlignment.Center;
            menuButtonLayouts = new DWriteTextLayout[count];
            menuButtonRects = new RectF[count];
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = GraphicsUtils.CreateCustomTextLayout(menuButtonTexts[i], format, menuExtraWidth, menuHeight);
                float width = layout.MaxWidth;
                menuButtonLayouts[i] = layout;
                menuButtonRects[i] = RectF.FromXYWH(menuX, baseY, width, menuHeight);
                menuX += width;
            }
            format.Dispose();
        }
        #endregion

        #region WndProc
        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessage)m.Msg)
            {
                case WindowMessage.MouseLeave:
                    ulong val = MenuBarButtonStatus.Exchange(0UL);
                    if (val > 0UL)
                    {
                        MenuBarButtonChangedStatus |= val;
                        Update();
                    }
                    goto default;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            var menuBarButtonLayouts = this.menuBarButtonLayouts;
            for (int i = 0, count = menuBarButtonLayouts.Length; i < count; i++)
                menuBarButtonLayouts[i]?.Dispose();
            base.Dispose(disposing);
        }
    }
}
