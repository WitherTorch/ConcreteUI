namespace ShioUI.Layout.Internals;

internal sealed class PageWidthLayoutNode : LayoutNode
{
    public static readonly PageWidthLayoutNode Instance = new PageWidthLayoutNode();

    private PageWidthLayoutNode() { }

    protected override int ComputeCore(in LayoutContext context) => context.PageSize.Width;
}
