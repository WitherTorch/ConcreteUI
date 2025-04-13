using System.Collections.Concurrent;

using ConcreteUI.Graphics.Native.DirectWrite;

namespace ConcreteUI.Utils
{
    public static class FontHeightHelper
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<float, float>> _fontHeightDict
            = new ConcurrentDictionary<string, ConcurrentDictionary<float, float>>();

        public static float GetFontHeight(string fontFamily, float fontSize) => 
            _fontHeightDict.GetOrAdd(fontFamily, _ => new ConcurrentDictionary<float, float>())
                .GetOrAdd(fontSize, GetFontHeightCore, fontFamily);

        private static float GetFontHeightCore(float fontSize, string fontFamily)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            using DWriteTextFormat format = factory.CreateTextFormat(fontFamily, fontSize);
            using DWriteTextLayout layout = factory.CreateTextLayout(string.Empty, format);
            return layout.GetMetrics().Height;
        }
    }
}
