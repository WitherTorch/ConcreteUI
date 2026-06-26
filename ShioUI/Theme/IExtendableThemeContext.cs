using System;
using System.Collections.Generic;

namespace ShioUI.Theme;

public interface IExtendableThemeContext : IThemeContext
{
    void RegisterColorFactoryGenerator(Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedColorFactory>>> generator);

    void RegisterBrushFactoryGenerator(Func<IThemeContext, IEnumerable<KeyValuePair<string, IThemedBrushFactory>>> generator);
}
