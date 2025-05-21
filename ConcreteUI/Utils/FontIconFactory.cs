using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.DirectWrite;

namespace ConcreteUI.Utils
{
    public sealed class FontIconFactory
    {
        private static readonly FontIconFactory _instance = new FontIconFactory();

        private readonly DWriteFont?[] _fluentSymbolFonts;
        private readonly DWriteFont? _segoeSymbolFont, _webDingsFont;
        private readonly string[] _fluentSymbolFontNames =
            ["Segoe Fluent Icons", "Segoe MDL2 Assets"];
        private readonly string _segoeSymbolFontName = "Segoe UI Symbol";
        private readonly string _webDingsFontName = "Webdings";

        public static FontIconFactory Instance => _instance;

        private FontIconFactory()
        {
            DWriteFactory factory = SharedResources.DWriteFactory;

            DWriteFontCollection collection = factory.GetSystemFontCollection();

            string[] fluentSymbolFontNames = _fluentSymbolFontNames;
            DWriteFont[] fluentSymbolFonts = new DWriteFont[fluentSymbolFontNames.Length];
            _fluentSymbolFonts = fluentSymbolFonts;

            uint index;
            for (int i = 0, length = fluentSymbolFontNames.Length; i < length; i++)
            {
                if (collection.FindFamilyName(fluentSymbolFontNames[i], out index))
                {
                    using DWriteFontFamily fontFamily = collection[index];
                    fluentSymbolFonts[i] = fontFamily.GetFirstMatchingFont(DWriteFontWeight.Normal, DWriteFontStretch.Normal, DWriteFontStyle.Normal);
                }
            }
            if (collection.FindFamilyName(_segoeSymbolFontName, out index))
            {
                using DWriteFontFamily fontFamily = collection[index];
                _segoeSymbolFont = fontFamily.GetFirstMatchingFont(DWriteFontWeight.Normal, DWriteFontStretch.Normal, DWriteFontStyle.Normal);
            }
            if (collection.FindFamilyName(_webDingsFontName, out index))
            {
                using DWriteFontFamily fontFamily = collection[index];
                _webDingsFont = fontFamily.GetFirstMatchingFont(DWriteFontWeight.Normal, DWriteFontStretch.Normal, DWriteFontStyle.Normal);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateFluentUIFontIcon(uint unicodeValue, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
            => TryCreateFluentUIFontIcon(unicodeValue, DWriteFontWeight.Normal, DWriteFontStyle.Normal, size, out icon);

        public bool TryCreateFluentUIFontIcon(uint unicodeValue, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
        {
            DWriteFont?[] fonts = _fluentSymbolFonts;
            string[] fontNames = _fluentSymbolFontNames;
            for (int i = 0, length = fonts.Length; i < length; i++)
            {
                if (TryCreateFontIconCore(fonts[i], fontNames[i], fontWeight, fontStyle, unicodeValue, size, out icon))
                    return true;
            }
            icon = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateSegoeSymbolFontIcon(uint unicodeValue, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
            => TryCreateSegoeSymbolFontIcon(unicodeValue, DWriteFontWeight.Normal, DWriteFontStyle.Normal, size, out icon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateSegoeSymbolFontIcon(uint unicodeValue, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
            => TryCreateFontIconCore(_segoeSymbolFont, _segoeSymbolFontName, fontWeight, fontStyle, unicodeValue, size, out icon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateWebdingsFontIcon(uint unicodeValue, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
            => TryCreateWebdingsFontIcon(unicodeValue, DWriteFontWeight.Normal, DWriteFontStyle.Normal, size, out icon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateWebdingsFontIcon(uint unicodeValue, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
            => TryCreateFontIconCore(_webDingsFont, _webDingsFontName, fontWeight, fontStyle, unicodeValue, size, out icon);

        private static bool TryCreateFontIconCore(DWriteFont? font, string fontName, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, uint unicodeValue, SizeF size, [NotNullWhen(true)] out FontIcon? icon)
        {
            if (font is null || !font.HasCharacter(unicodeValue))
            {
                icon = null;
                return false;
            }
            icon = new FontIcon(fontName, fontWeight, fontStyle, unicodeValue, size);
            return true;
        }
    }
}
