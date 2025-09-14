using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Utils;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals
{
    internal sealed class FontIconResources : IDisposable
    {
        private static readonly FontIconResources _instance = new FontIconResources();

        private readonly FontIcon? _maxIcon, _restoreIcon, _minIcon, _closeIcon, _scrollUpIcon, _scrollDownIcon;
        private readonly Dictionary<float, FontIcon> _comboBoxDropdownIconDict, _checkMarkIconDict;

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
            _comboBoxDropdownIconDict = new Dictionary<float, FontIcon>();
            _checkMarkIconDict = new Dictionary<float, FontIcon>();
        }

        private static FontIcon? GetMaxIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE922, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F5D6, UIConstantsPrivate.TitleBarIconSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF031, UIConstantsPrivate.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon? GetRestoreIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE923, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F5D7, UIConstantsPrivate.TitleBarIconSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF032, UIConstantsPrivate.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon? GetMinIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE921, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F5D5, UIConstantsPrivate.TitleBarIconSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF072, UIConstantsPrivate.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon? GetCloseIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xE8BB, UIConstantsPrivate.TitleBarIconSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F5D9, UIConstantsPrivate.TitleBarIconSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF030, UIConstantsPrivate.TitleBarIconSize, out result))
                return result;
            return null;
        }

        private static FontIcon? GetScrollUpIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xEDDB, UIConstantsPrivate.ScrollBarScrollButtonSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F53A, UIConstantsPrivate.ScrollBarScrollButtonSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF035, UIConstantsPrivate.ScrollBarScrollButtonSize, out result))
                return result;
            return null;
        }

        private static FontIcon? GetScrollDownIcon(FontIconFactory factory)
        {
            if (factory.TryCreateFluentUIFontIcon(0xEDDC, UIConstantsPrivate.ScrollBarScrollButtonSize, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0x1F53B, UIConstantsPrivate.ScrollBarScrollButtonSize, out result) ||
                factory.TryCreateWebdingsFontIcon(0xF036, UIConstantsPrivate.ScrollBarScrollButtonSize, out result))
                return result;
            return null;
        }

        private static FontIcon? CreateComboBoxDropDownIcon(float layoutHeight)
        {
            FontIconFactory factory = FontIconFactory.Instance;
            const uint ComboBoxDropdownCharater = 0xE011;
            SizeF size = new SizeF(layoutHeight, layoutHeight);
            if (factory.TryCreateFluentUIFontIcon(ComboBoxDropdownCharater, size, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(ComboBoxDropdownCharater, size, out result))
                return result;
            return null;
        }

        private static FontIcon? CreateCheckMarkIcon(float layoutHeight)
        {
            FontIconFactory factory = FontIconFactory.Instance;
            SizeF size = new SizeF(layoutHeight, layoutHeight);
            if (factory.TryCreateFluentUIFontIcon(0xE73E, size, out FontIcon? result) ||
                factory.TryCreateSegoeSymbolFontIcon(0xE001, size, out result) ||
                factory.TryCreateWebdingsFontIcon(0x0061, size, out result))
                return result;
            return null;
        }

        private static unsafe FontIcon? GetOrCreateIcon(Dictionary<float, FontIcon> dict, float layoutHeight, 
            delegate* managed<float, FontIcon?> createFunc)
        {
            if (layoutHeight < float.Epsilon)
                return null;
            lock (dict)
            {
                if (dict.TryGetValue(layoutHeight, out FontIcon? result))
                    return result;
                result = createFunc(layoutHeight);
                if (result is not null)
                    dict.Add(layoutHeight, result);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMaximizeButton(IRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _maxIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderRestoreButton(IRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _restoreIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMinimizeButton(IRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _minIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderCloseButton(IRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _closeIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DrawDropDownButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => GetOrCreateIcon(_comboBoxDropdownIconDict, rect.Height - UIConstants.ElementMargin,
                &CreateComboBoxDropDownIcon)?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScrollBarUpButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _scrollUpIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScrollBarDownButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => _scrollDownIcon?.Render(context, rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DrawCheckMark(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
            => GetOrCreateIcon(_checkMarkIconDict, rect.Height,
                &CreateCheckMarkIcon)?.Render(context, rect.Location, brush);

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
