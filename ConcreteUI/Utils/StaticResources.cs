using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils
{
    internal static class StaticResources
    {
        private static string _capFontName;
        public static string CaptionFontFamilyName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capFontName;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _capFontName = value;
                ApplyFont(value);
            }
        }

        public static DWriteTextFormat titleFormat;
        public static DWriteTextFormat menuTextFormat;
        public static DWriteTextFormat defaultTextFormat;
        public static DWriteTextFormat defaultTextFormatTrailling;
        public static DWriteTextFormat descriptionTextFormat;

        static StaticResources()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeStaticResource()
        {
            titleFormat?.Dispose();
            menuTextFormat?.Dispose();
        }

        public static void ApplyFont(string fontName)
        {
            DWriteFactory writeFactory = SharedResources.DWriteFactory;
            _capFontName = fontName;
            DWriteTextFormat format = writeFactory.CreateTextFormat(fontName, UIConstants.TitleFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            DisposeHelper.SwapDisposeInterlocked(ref titleFormat, format);
            format = writeFactory.CreateTextFormat(fontName, UIConstants.MenuFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            format.TextAlignment = DWriteTextAlignment.Center;
            DisposeHelper.SwapDisposeInterlocked(ref menuTextFormat, format);
            format = writeFactory.CreateTextFormat(fontName, UIConstants.DefaultFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            DisposeHelper.SwapDisposeInterlocked(ref defaultTextFormat, format);
            format = writeFactory.CreateTextFormat(fontName, UIConstants.DefaultFontSize);
            format.TextAlignment = DWriteTextAlignment.Trailing;
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            DisposeHelper.SwapDisposeInterlocked(ref defaultTextFormatTrailling, format);
            format = writeFactory.CreateTextFormat(fontName, UIConstants.DescriptionFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Far;
            DisposeHelper.SwapDisposeInterlocked(ref descriptionTextFormat, format);
        }
    }
}
