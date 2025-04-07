using ConcreteUI.Controls;
using ConcreteUI.Graphics.Native.DirectWrite;

namespace ConcreteUI.Utils
{
    internal static class TextFormatUtils
    {
        public static DWriteTextFormat CreateTextFormat(TextAlignment alignment, float fontSize)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat result = factory.CreateTextFormat(StaticResources.CaptionFontFamilyName, fontSize);
            SetAlignment(result, alignment);
            return result;
        }

        public static DWriteTextFormat CreateTextFormat(TextAlignment alignment, float fontSize, DWriteFontStyle style)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat result = factory.CreateTextFormat(StaticResources.CaptionFontFamilyName, fontSize, fontStyle: style);
            SetAlignment(result, alignment);
            return result;
        }

        public static DWriteTextLayout CreateTextLayout(string text, TextAlignment alignment, float fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat format = factory.CreateTextFormat(StaticResources.CaptionFontFamilyName, fontSize);
            SetAlignment(format, alignment);
            DWriteTextLayout result = factory.CreateTextLayout(text, format);
            format.Dispose();
            return result;
        }

        public static DWriteTextLayout CreateTextLayout(string text, TextAlignment alignment, float fontSize, DWriteFontStyle style)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat format = factory.CreateTextFormat(StaticResources.CaptionFontFamilyName, fontSize, fontStyle: style);
            SetAlignment(format, alignment);
            DWriteTextLayout result = factory.CreateTextLayout(text, format);
            format.Dispose();
            return result;
        }

        public static void SetAlignment(DWriteTextFormat format, TextAlignment textAlign)
        {
            DWriteParagraphAlignment pAlign;
            DWriteTextAlignment tAlign;
            switch (textAlign)
            {
                case TextAlignment.TopLeft:
                    pAlign = DWriteParagraphAlignment.Near;
                    tAlign = DWriteTextAlignment.Leading;
                    break;
                case TextAlignment.TopCenter:
                    pAlign = DWriteParagraphAlignment.Near;
                    tAlign = DWriteTextAlignment.Center;
                    break;
                case TextAlignment.TopRight:
                    pAlign = DWriteParagraphAlignment.Near;
                    tAlign = DWriteTextAlignment.Trailing;
                    break;
                case TextAlignment.MiddleLeft:
                    pAlign = DWriteParagraphAlignment.Center;
                    tAlign = DWriteTextAlignment.Leading;
                    break;
                case TextAlignment.MiddleCenter:
                    pAlign = DWriteParagraphAlignment.Center;
                    tAlign = DWriteTextAlignment.Center;
                    break;
                case TextAlignment.MiddleRight:
                    pAlign = DWriteParagraphAlignment.Center;
                    tAlign = DWriteTextAlignment.Trailing;
                    break;
                case TextAlignment.BottomLeft:
                    pAlign = DWriteParagraphAlignment.Far;
                    tAlign = DWriteTextAlignment.Leading;
                    break;
                case TextAlignment.BottomCenter:
                    pAlign = DWriteParagraphAlignment.Far;
                    tAlign = DWriteTextAlignment.Center;
                    break;
                case TextAlignment.BottomRight:
                    pAlign = DWriteParagraphAlignment.Far;
                    tAlign = DWriteTextAlignment.Trailing;
                    break;
                default:
                    return;
            }
            format.ParagraphAlignment = pAlign;
            format.TextAlignment = tAlign;
        }
    }
}
