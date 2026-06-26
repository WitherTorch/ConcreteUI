namespace ShioUI.Layout.Internals;

internal sealed class PageWidthNode : LayoutNode
{
    public static readonly PageWidthNode Instance = new PageWidthNode();

    private PageWidthNode() { }

    public override int Compute(in LayoutNodeManager manager) => manager.GetPageSize().Width;

    public override bool Equals(object? obj) => obj is PageWidthNode;

    public override int GetHashCode() => base.GetHashCode();
}

internal sealed class PageHeightNode : LayoutNode
{
    public static readonly PageHeightNode Instance = new PageHeightNode();

    private PageHeightNode() { }

    public override int Compute(in LayoutNodeManager manager) => manager.GetPageSize().Height;

    public override bool Equals(object? obj) => obj is PageHeightNode;

    public override int GetHashCode() => base.GetHashCode();
}