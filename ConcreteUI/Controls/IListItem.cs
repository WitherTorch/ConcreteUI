using System;
using System.Drawing;

using ConcreteUI.Graphics;

namespace ConcreteUI.Controls
{
    public interface IListItem : IDisposable
    {
        bool NeedRefresh();

        void Render(in RegionalRenderingContext context, SizeF size);
    }
}
