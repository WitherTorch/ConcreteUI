using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils
{
    public sealed class FontIconFactory
    {
        private static readonly FontIconFactory _instance = new FontIconFactory();

        private readonly DWriteFont[] _fluentSymbolFonts;
        private readonly DWriteFont _segoeSymbolFont, _webDingsFont;
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

        public bool TryCreateFluentUIFontIcon(uint unicodeValue, SizeF size, out FontIcon icon)
        {
            UnsafeHelper.SkipInit(out icon);

            DWriteFont[] fonts = _fluentSymbolFonts;
            string[] fontNames = _fluentSymbolFontNames;
            for (int i = 0, length = fonts.Length; i < length; i++)
            {
                if (TryCreateFontIconCore(fonts[i], fontNames[i], unicodeValue, size, out icon))
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateSegoeSymbolFontIcon(uint unicodeValue, SizeF size, out FontIcon icon)
            => TryCreateFontIconCore(_segoeSymbolFont, _segoeSymbolFontName, unicodeValue, size, out icon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateWebdingsFontIcon(uint unicodeValue, SizeF size, out FontIcon icon)
            => TryCreateFontIconCore(_webDingsFont, _webDingsFontName, unicodeValue, size, out icon);

        private static bool TryCreateFontIconCore(DWriteFont font, string fontName, uint unicodeValue, SizeF size, out FontIcon icon)
        {
            UnsafeHelper.SkipInit(out icon);

            if (font is null || !font.HasCharacter(unicodeValue))
                return false;
            icon = new FontIcon(fontName, unicodeValue, size);
            return true;
        }
    }
}
