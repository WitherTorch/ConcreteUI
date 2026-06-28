namespace ShioUI.Layout.Internals;

internal sealed class PageWidthNode : LayoutNode
{
    public static readonly PageWidthNode Instance = new PageWidthNode();

    private PageWidthNode() { }

    protected override int ComputeCore(in LayoutNodeManager manager) => manager.GetPageSize().Width;

    public override bool Equals(object? obj) => obj is PageWidthNode;

    public override int GetHashCode() => base.GetHashCode();
}

internal sealed class PageHeightNode : LayoutNode
{
    public static readonly PageHeightNode Instance = new PageHeightNode();

    private PageHeightNode() { }

    protected override int ComputeCore(in LayoutNodeManager manager) => manager.GetPageSize().Height;

    public override bool Equals(object? obj) => obj is PageHeightNode;

    public override int GetHashCode() => base.GetHashCode();
}