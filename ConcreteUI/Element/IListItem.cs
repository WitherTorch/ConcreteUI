using System;
using System.Drawing;

using ConcreteUI.Graphics;

namespace ConcreteUI.Element;

public interface IListItem : IDisposable
{
    bool NeedRefresh();

    void Render(in RegionalRenderingContext context, SizeF size);
}
