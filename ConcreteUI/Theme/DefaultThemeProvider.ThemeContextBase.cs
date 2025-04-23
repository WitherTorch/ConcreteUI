using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection.Emit;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Theme
{
    partial class DefaultThemeProvider
    {
        private abstract class ThemeContextBase : IExtendableThemeContext
        {
            private readonly Dictionary<string, IThemedColorFactory> _colorDict;
            private readonly Dictionary<string, IThemedBrushFactory> _brushDict;
            private readonly HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedColorFactory>>>> _colorFactoryGenerators;
            private readonly HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedBrushFactory>>>> _brushFactoryGenerators;

            private string _fontName;

            public abstract bool IsDarkTheme { get; }

            public string FontName
            {
                get => _fontName;
                set => _fontName = value;
            }

            protected ThemeContextBase()
            {
                _fontName = NullSafetyHelper.ThrowIfNull(SystemFonts.CaptionFont).Name;
                _colorFactoryGenerators = new HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedColorFactory>>>>();
                _brushFactoryGenerators = new HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedBrushFactory>>>>();

                Dictionary<string, IThemedColorFactory> colorDict = new Dictionary<string, IThemedColorFactory>();
                Dictionary<string, IThemedBrushFactory> brushDict = new Dictionary<string, IThemedBrushFactory>();

                foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(key => colorDict[key.ToLowerAscii()]))
                    colorDict[item.Key.ToLowerAscii()] = item.Value;
                foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(key => colorDict[key.ToLowerAscii()], key => brushDict[key.ToLowerAscii()]))
                    brushDict[item.Key.ToLowerAscii()] = item.Value;

                _colorDict = colorDict;
                _brushDict = brushDict;
            }

            protected ThemeContextBase(ThemeContextBase original)
            {
                _fontName = original._fontName;
                _colorDict = new Dictionary<string, IThemedColorFactory>(original._colorDict);
                _brushDict = new Dictionary<string, IThemedBrushFactory>(original._brushDict);
                _colorFactoryGenerators = new HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedColorFactory>>>>(original._colorFactoryGenerators);
                _brushFactoryGenerators = new HashSet<Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedBrushFactory>>>>(original._brushFactoryGenerators);
            }

            public abstract IThemeContext Clone();

            public bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory)
                => _brushDict.TryGetValue(node.ToLowerAscii(), out brushFactory);

            public bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory)
                => _colorDict.TryGetValue(node.ToLowerAscii(), out colorFactory);

            public bool TrySetBrushFactory(string node, IThemedBrushFactory brushFactory, bool overrides)
            {
                Dictionary<string, IThemedBrushFactory> brushDict = _brushDict;
                node = node.ToLowerAscii();
                if (!overrides && brushDict.ContainsKey(node))
                    return false;
                brushDict[node] = brushFactory;
                return true;
            }

            public bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides)
            {
                Dictionary<string, IThemedColorFactory> colorDict = _colorDict;
                node = node.ToLowerAscii();
                if (!overrides && colorDict.ContainsKey(node))
                    return false;
                colorDict[node] = colorFactory;
                return true;
            }

            public void BuildContextForAnother(IThemeContext other, bool overrides)
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
                if (other is IExtendableThemeContext extendableOther)
                {
                    foreach (var generator in _colorFactoryGenerators)
                        extendableOther.RegisterColorFactoryGenerator(generator);
                    foreach (var generator in _brushFactoryGenerators)
                        extendableOther.RegisterBrushFactoryGenerator(generator);
                }
                else
                {
                    foreach (var generator in _colorFactoryGenerators)
                    {
                        foreach (KeyValuePair<string, IThemedColorFactory> item in generator.Invoke(other))
                            other.TrySetColorFactory(item.Key, item.Value, overrides);
                    }
                    foreach (var generator in _brushFactoryGenerators)
                    {
                        foreach (KeyValuePair<string, IThemedBrushFactory> item in generator.Invoke(other))
                            other.TrySetBrushFactory(item.Key, item.Value, overrides);
                    }
                }
            }

            protected abstract IEnumerable<KeyValuePair<string, IThemedColorFactory>> CreateColorFactories(Func<string, IThemedColorFactory> queryFunction);

            protected abstract IEnumerable<KeyValuePair<string, IThemedBrushFactory>> CreateBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunction, Func<string, IThemedBrushFactory> queryBrushFunction);

            public void RegisterColorFactoryGenerator(Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedColorFactory>>> generator)
            {
                if (!_colorFactoryGenerators.Add(generator))
                    return;
                foreach (KeyValuePair<string, IThemedColorFactory> item in generator.Invoke(this))
                    _colorDict[item.Key] = item.Value;
            }

            public void RegisterBrushFactoryGenerator(Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedBrushFactory>>> generator)
            {
                if (!_brushFactoryGenerators.Add(generator))
                    return;
                foreach (KeyValuePair<string, IThemedBrushFactory> item in generator.Invoke(this))
                    _brushDict[item.Key] = item.Value;
            }

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
                    node = node.ToLowerAscii();
                    if (_otherContext._colorDict.TryGetValue(node, out IThemedColorFactory? result))
                        return result;
                    return _this._colorDict[node];
                }

                public IThemedBrushFactory GetBrushFactory(string node)
                {
                    node = node.ToLowerAscii();
                    if (_otherContext._brushDict.TryGetValue(node, out IThemedBrushFactory? result))
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
                    node = node.ToLowerAscii();
                    if (_otherContext.TryGetColorFactory(node, out IThemedColorFactory? result))
                        return result;
                    return _this._colorDict[node];
                }

                public IThemedBrushFactory GetBrushFactory(string node)
                {
                    node = node.ToLowerAscii();
                    if (_otherContext.TryGetBrushFactory(node, out IThemedBrushFactory? result))
                        return result;
                    return _this._brushDict[node];
                }
            }
        }
    }
}