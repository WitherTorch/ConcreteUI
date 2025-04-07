using System;
using System.Collections.Generic;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Theme
{
    partial class DefaultThemeProvider
    {
        private abstract class ThemeContextBase : IThemeContext
        {
            private readonly Dictionary<string, IThemedColorFactory> _colorDict;
            private readonly Dictionary<string, IThemedBrushFactory> _brushDict;

            public abstract bool IsDarkTheme { get; }

            protected ThemeContextBase()
            {
                Dictionary<string, IThemedColorFactory> colorDict = new Dictionary<string, IThemedColorFactory>();
                Dictionary<string, IThemedBrushFactory> brushDict = new Dictionary<string, IThemedBrushFactory>();

                foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(key => colorDict[key]))
                    colorDict[item.Key.ToLowerAscii()] = item.Value;
                foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(key => colorDict[key], key => brushDict[key]))
                    brushDict[item.Key.ToLowerAscii()] = item.Value;

                _colorDict = colorDict;
                _brushDict = brushDict;
            }

            protected ThemeContextBase(ThemeContextBase original)
            {
                _colorDict = new Dictionary<string, IThemedColorFactory>(original._colorDict);
                _brushDict = new Dictionary<string, IThemedBrushFactory>(original._brushDict);
            }

            public abstract IThemeContext Clone();

            public bool TryGetBrushFactory(string node, out IThemedBrushFactory brushFactory)
                => _brushDict.TryGetValue(node.ToLowerAscii(), out brushFactory);

            public bool TryGetColorFactory(string node, out IThemedColorFactory colorFactory)
                => _colorDict.TryGetValue(node.ToLowerAscii(), out colorFactory);

            public bool TrySetBrushFactory(string node, IThemedBrushFactory brushFactory, bool overrides)
            {
                Dictionary<string, IThemedBrushFactory> brushDict = _brushDict;
                if (!overrides && brushDict.ContainsKey(node))
                    return false;
                brushDict[node] = brushFactory;
                return true;
            }

            public bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides)
            {
                Dictionary<string, IThemedColorFactory> colorDict = _colorDict;
                if (!overrides && colorDict.ContainsKey(node))
                    return false;
                colorDict[node] = colorFactory;
                return true;
            }

            public void ApplyToOtherContext(IThemeContext other, bool overrides)
            {
                if (other is ThemeContextBase otherContextBase)
                {
                    ApplyToOtherContextMethodClosureFast closure = new ApplyToOtherContextMethodClosureFast(this, otherContextBase);
                    foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(closure.GetColorFactory))
                        other.TrySetColorFactory(item.Key.ToLowerAscii(), item.Value, overrides);
                    foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(closure.GetColorFactory, closure.GetBrushFactory))
                        other.TrySetBrushFactory(item.Key.ToLowerAscii(), item.Value, overrides);
                }
                else
                {
                    ApplyToOtherContextMethodClosureSlow closure = new ApplyToOtherContextMethodClosureSlow(this, other);
                    foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(closure.GetColorFactory))
                        other.TrySetColorFactory(item.Key.ToLowerAscii(), item.Value, overrides);
                    foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(closure.GetColorFactory, closure.GetBrushFactory))
                        other.TrySetBrushFactory(item.Key.ToLowerAscii(), item.Value, overrides);
                }
            }

            protected abstract IEnumerable<KeyValuePair<string, IThemedColorFactory>> CreateColorFactories(Func<string, IThemedColorFactory> queryFunction);

            protected abstract IEnumerable<KeyValuePair<string, IThemedBrushFactory>> CreateBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunction, Func<string, IThemedBrushFactory> queryBrushFunction);

            private readonly struct ApplyToOtherContextMethodClosureFast
            {
                private readonly ThemeContextBase _this;
                private readonly ThemeContextBase _otherContext;

                public ApplyToOtherContextMethodClosureFast(ThemeContextBase @this, ThemeContextBase otherContext)
                {
                    _this = @this;
                    _otherContext = otherContext;
                }

                public IThemedColorFactory GetColorFactory(string node)
                {
                    if (_otherContext._colorDict.TryGetValue(node, out IThemedColorFactory result))
                        return result;
                    return _this._colorDict[node];
                }

                public IThemedBrushFactory GetBrushFactory(string node)
                {
                    if (_otherContext._brushDict.TryGetValue(node, out IThemedBrushFactory result))
                        return result;
                    return _this._brushDict[node];
                }
            }

            private readonly struct ApplyToOtherContextMethodClosureSlow
            {
                private readonly ThemeContextBase _this;
                private readonly IThemeContext _otherContext;

                public ApplyToOtherContextMethodClosureSlow(ThemeContextBase @this, IThemeContext otherContext)
                {
                    _this = @this;
                    _otherContext = otherContext;
                }

                public IThemedColorFactory GetColorFactory(string node)
                {
                    if (_otherContext.TryGetColorFactory(node, out IThemedColorFactory result))
                        return result;
                    return _this._colorDict[node];
                }

                public IThemedBrushFactory GetBrushFactory(string node)
                {
                    if (_otherContext.TryGetBrushFactory(node, out IThemedBrushFactory result))
                        return result;
                    return _this._brushDict[node];
                }
            }
        }
    }
}