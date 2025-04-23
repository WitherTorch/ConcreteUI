using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    partial class ThemeResourceProvider
    {
        private sealed class ThemeResourceProviderImpl : IThemeResourceProvider, IDisposable
        {
            private readonly Dictionary<string, D2D1Brush> _brushDict;
            private readonly D2D1DeviceContext _deviceContext;
            private readonly IThemeContext _themeContext;
            private readonly WindowMaterial _material;

            private bool _disposed;

            public IThemeContext ThemeContext => _themeContext;

            public string FontName => _themeContext.FontName;

            public ThemeResourceProviderImpl(D2D1DeviceContext deviceContext, IThemeContext themeContext, WindowMaterial material)
            {
                _deviceContext = deviceContext;
                _themeContext = themeContext;
                _material = material;

                _brushDict = new Dictionary<string, D2D1Brush>();
                _disposed = false;
            }

            public IThemeResourceProvider Clone() => new ThemeResourceProviderReference(this);

            public bool TryGetColor(string node, out D2D1ColorF color)
            {
                if (!_themeContext.TryGetColorFactory(node, out IThemedColorFactory? factory))
                {
                    color = default;
                    return false;
                }
                color = factory.CreateColorByMaterial(_material);
                return true;
            }

            public bool TryGetBrush(string node, [NotNullWhen(true)] out D2D1Brush? brush)
            {
                Dictionary<string, D2D1Brush> brushDict = _brushDict;
                if (brushDict.TryGetValue(node, out brush))
                    return true;
                if (_themeContext.TryGetBrushFactory(node, out IThemedBrushFactory? factory))
                {
                    brush = factory.CreateBrushByMaterial(_deviceContext, _material);
                    brushDict.Add(node, brush);
                    return true;
                }
                brush = null;
                return false;
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                    return;
                _disposed = true;

                Dictionary<string, D2D1Brush> brushDict = _brushDict;
                if (disposing)
                {
                    foreach (D2D1Brush brush in brushDict.Values)
                        brush.Dispose();
                }
                brushDict.Clear();
            }

            ~ThemeResourceProviderImpl()
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
}
