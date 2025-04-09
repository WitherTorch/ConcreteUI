using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    public sealed class ThemeResourceProvider : IDisposable
    {
        private readonly Dictionary<string, D2D1Brush> _brushDict;
        private readonly D2D1DeviceContext _deviceContext;
        private readonly IThemeContext _themeContext;
        private readonly WindowMaterial _material;

        private bool _disposed;

        public IThemeContext ThemeContext => _themeContext;

        public string FontName => _themeContext.FontName;

        public ThemeResourceProvider(D2D1DeviceContext deviceContext, IThemeContext themeContext, WindowMaterial material)
        {
            _deviceContext = deviceContext;
            _themeContext = themeContext;
            _material = material;

            _brushDict = new Dictionary<string, D2D1Brush>();
            _disposed = false;
        }

        public bool TryGetColor(string node, out D2D1ColorF color)
        {
            if (!_themeContext.TryGetColorFactory(node, out IThemedColorFactory factory))
            {
                color = default;
                return false;
            }
            color = factory.CreateColorByMaterial(_material);
            return true;
        }

        public bool TryGetBrush(string node, out D2D1Brush brush)
        {
            Dictionary<string, D2D1Brush> brushDict = _brushDict;
            if (brushDict.TryGetValue(node, out brush)) 
                return true;
            if (_themeContext.TryGetBrushFactory(node, out IThemedBrushFactory factory))
            {
                brush = factory.CreateBrushByMaterial(_deviceContext, _material);
                brushDict.Add(node, brush);
                return true;
            }
            brush = null;
            return false;
        }

        private void DisposeCore()
        {
            if (_disposed)
                return;
            _disposed = true;

            Dictionary<string, D2D1Brush> brushDict = _brushDict;
            foreach (D2D1Brush brush in brushDict.Values)
                brush.Dispose();
            brushDict.Clear();
        }

        ~ThemeResourceProvider()
        {
            DisposeCore();
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
