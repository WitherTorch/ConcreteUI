using System;

namespace ConcreteUI.Layout;

public sealed class CyclicDependencyException : Exception
{
    private readonly LayoutNode[] _walkedNodes;

    public LayoutNode[] WalkedNodes => _walkedNodes;

    public CyclicDependencyException(LayoutNode[] walkedNodes)
    {
        _walkedNodes = walkedNodes;
    }

    public CyclicDependencyException(string? message, LayoutNode[] walkedNodes) : base(message)
    {
        _walkedNodes = walkedNodes;
    }
}
