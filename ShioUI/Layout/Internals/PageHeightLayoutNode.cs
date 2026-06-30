namespace ShioUI.Layout.Internals;

internal sealed class PageHeightLayoutNode : LayoutNode
{
    public static readonly PageHeightLayoutNode Instance = new PageHeightLayoutNode();

    private PageHeightLayoutNode() { }

    protected override int ComputeCore(in LayoutContext context) => context.PageSize.Height;
}