using System;
using System.Drawing;

using ShioUI.Graphics;

namespace ShioUI.Controls;

public interface IListItem : IDisposable
{
    bool NeedRefresh();

    void Render(in RegionalRenderingContext context, SizeF size);
}
