using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Internals;

using InlineMethod;

namespace ConcreteUI.Utils
{
    internal sealed class FontIconResources : IDisposable
    {
        private static readonly FontIconResources _instance = new FontIconResources();

        private readonly FontIcon _maxIcon, _restoreIcon, _minIcon, _closeIcon, _scrollUpIcon, _scrollDownIcon;
        private readonly ConcurrentDictionary<float, FontIcon> _comboBoxDropdownIconDict;

        private bool _disposed;

        public static FontIconResources Instance => _instance;

        private FontIconResources()
        {
            FontIconFactory factory = FontIconFactory.Instance;
            _maxIcon = GetMaxIcon(factory);
            _restoreIcon = GetRestoreIcon(factory);
            _minIcon = GetMinIcon(factory);
            _closeIcon = GetCloseIcon(factory);
            _scrollUpIcon = GetScrollUpIcon(factory);
            _scrollDownIcon = GetScrollDownIcon(factory);
            _comboBoxDropdownIconDict = new ConcurrentDictionary<float, FontIcon>();
        }

        private static FontIcon GetMaxIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE922, UIConstants.TitleBarIconSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F5D6, UIConstants.TitleBarIconSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF031, UIConstants.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetRestoreIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE923, UIConstants.TitleBarIconSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F5D7, UIConstants.TitleBarIconSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF032, UIConstants.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetMinIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE921, UIConstants.TitleBarIconSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F5D5, UIConstants.TitleBarIconSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF072, UIConstants.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetCloseIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE8BB, UIConstants.TitleBarIconSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F5D9, UIConstants.TitleBarIconSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF030, UIConstants.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetScrollUpIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xEDDB, UIConstants.ScrollBarScrollButtonSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F53A, UIConstants.ScrollBarScrollButtonSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF035, UIConstants.ScrollBarScrollButtonSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetScrollDownIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xEDDC, UIConstants.ScrollBarScrollButtonSize, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(0x1F53B, UIConstants.ScrollBarScrollButtonSize, out result))
                return result;
            if (factory.TryCreateWebdingsFontIcon(0xF036, UIConstants.ScrollBarScrollButtonSize, out result))
                return result;
            return null;
        }

        private static FontIcon GetComboBoxDropDownIcon(float layoutHeight)
        {
            FontIconFactory factory = FontIconFactory.Instance;
            const uint ComboBoxDropdownCharater = 0xE011;
            SizeF size = new SizeF(layoutHeight, layoutHeight);
            if (factory.TryCreateFluentUIFontIcon(ComboBoxDropdownCharater, size, out FontIcon result))
                return result;
            if (factory.TryCreateSegoeSymbolFontIcon(ComboBoxDropdownCharater, size, out result))
                return result;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMaximizeButton(D2D1DeviceContext context, PointF point, D2D1Brush brush) 
            => _maxIcon?.Render(context, AdjustPointForTitleBar(point), brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderRestoreButton(D2D1DeviceContext context, PointF point, D2D1Brush brush) 
            => _restoreIcon?.Render(context, AdjustPointForTitleBar(point), brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMinimizeButton(D2D1DeviceContext context, PointF point, D2D1Brush brush) 
            => _minIcon?.Render(context, AdjustPointForTitleBar(point), brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderCloseButton(D2D1DeviceContext context, PointF point, D2D1Brush brush) 
            => _closeIcon?.Render(context, AdjustPointForTitleBar(point), brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawDropDownButton(D2D1DeviceContext context, PointF point, float buttonHeight, D2D1Brush brush)
            => _comboBoxDropdownIconDict.GetOrAdd(buttonHeight, GetComboBoxDropDownIcon)?.Render(context, point, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScrollBarUpButton(D2D1DeviceContext context, in RectangleF targetRect, D2D1Brush brush) 
            => _scrollUpIcon?.Render(context, GetTargetPointForScrollBar(targetRect), brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScrollBarDownButton(D2D1DeviceContext context, in RectangleF targetRect, D2D1Brush brush) 
            => _scrollDownIcon?.Render(context, GetTargetPointForScrollBar(targetRect), brush);

        [Inline(InlineBehavior.Remove)]
        private static PointF AdjustPointForTitleBar(PointF original)
        {
            original.X += (UIConstants.TitleBarButtonSizeWidth - UIConstants.TitleBarIconSizeWidth) * 0.5f;
            original.Y += (UIConstants.TitleBarButtonSizeHeight - UIConstants.TitleBarIconSizeHeight) * 0.5f;
            return original;
        }

        [Inline(InlineBehavior.Remove)]
        private static PointF GetTargetPointForScrollBar(in RectangleF targetRect)
        {
            PointF result = targetRect.Location;
            result.X += (targetRect.Width - UIConstants.ScrollBarScrollButtonWidth) * 0.5f;
            result.Y += (targetRect.Height - UIConstants.ScrollBarScrollButtonWidth) * 0.5f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                _maxIcon?.Dispose();
                _restoreIcon?.Dispose();
                _minIcon?.Dispose();
                _closeIcon?.Dispose();
                _scrollUpIcon?.Dispose();
                _scrollDownIcon?.Dispose();
                _comboBoxDropdownIconDict.Clear();
            }
        }

        ~FontIconResources()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
