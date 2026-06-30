using RiceTea.Core.Helpers;

using ShioUI.Layout;

namespace ShioUI.Utils;

public static class ContainerAutoSizeHelper
{
    public static (int Result, bool ReversedFlow) Compute(LayoutNode node, UIElement element, in LayoutContext context,
        LayoutProperty deltaStart, LayoutProperty deltaEnd, int initialValue)
    {
        using LayoutContext.ChildrenEnumerator enumerator = context.GetChildrenEnumerator(element);
        if (!enumerator.MoveNext())
            goto Zero;

        int recorded_delta, recorded_start;

        {
            VirtualLayoutContext.Builder builder = context.CreateVirtualContextBuilder();
            using VirtualLayoutContext virtualContext = builder.WithFakeNodeValue(node, initialValue).Build();
            int start = int.MaxValue, end = int.MinValue;
            do
            {
                UIElement child = enumerator.Current;
                start = MathHelper.Min(start, virtualContext.GetComputedValue(child, deltaStart));
                end = MathHelper.Max(end, virtualContext.GetComputedValue(child, deltaEnd));
            } while (enumerator.MoveNext());
            virtualContext.ClearTemporaryCacheForNodes();

            recorded_start = start;
            recorded_delta = end - start;
        }

        enumerator.Reset();
        if (!enumerator.MoveNext())
            goto Zero;

        {
            VirtualLayoutContext.Builder builder = context.CreateVirtualContextBuilder();
            using VirtualLayoutContext virtualContext = builder.WithFakeNodeValue(node, recorded_delta).Build();
            int start = int.MaxValue, end = int.MinValue;
            do
            {
                UIElement child = enumerator.Current;
                start = MathHelper.Min(start, virtualContext.GetComputedValue(child, deltaStart));
                end = MathHelper.Max(end, virtualContext.GetComputedValue(child, deltaEnd));
            } while (enumerator.MoveNext());
            int delta = end - start;

            if (recorded_delta != delta)
                goto Failed;
            if (recorded_start != start)
            {
                virtualContext.ClearTemporaryCacheForNodes();
                return (Result: delta - start, ReversedFlow: true);
            }

            return (Result: end, ReversedFlow: false);
        }

    Failed:
        context.ThrowCyclicDependencyException(node);
        goto Zero;

    Zero:
        return (Result: 0, ReversedFlow: false);
    }
}
