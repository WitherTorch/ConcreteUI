using System;
using System.Drawing;

using ConcreteUI.Graphics;

namespace ConcreteUI.Controls
{
    public interface IAppendOnlyListItem<TSelf> : IDisposable where TSelf : IAppendOnlyListItem<TSelf>
    {
        int CalculateHeight(TSelf? reference, bool force);

        void Render(in RegionalRenderingContext context, SizeF size);
    }
}
