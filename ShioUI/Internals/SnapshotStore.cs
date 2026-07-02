using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

using ShioUI.Windows;

namespace ShioUI.Internals;

internal sealed unsafe partial class CacheStore<T> : IDisposable
{
    private static readonly Pool<CacheNode> _snapshotPool = new(initialLength: 32);

    private readonly Dictionary<ulong, CacheNode> _snapshotDict = new();
    private readonly Lock _syncLock = new();
    private readonly object _owner;
    private readonly delegate* managed<object, CacheNode, void> _createSnapshotFunc, _removeSnapshotFunc;

    private CacheNode? _lastSnapshot;
    private ulong _lastTimestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CacheStore(object owner,
        delegate* managed<object, CacheNode, void> createSnapshotFunc,
        delegate* managed<object, CacheNode, void> removeSnapshotFunc)
    {
        _owner = owner;
        _createSnapshotFunc = createSnapshotFunc;
        _removeSnapshotFunc = removeSnapshotFunc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateTimestamp(ulong timestamp)
    {
        lock (_syncLock)
        {
            if (ReferenceHelper.Exchange(ref _lastTimestamp, timestamp) == timestamp)
                return;
            CacheNode? lastSnapshot = ReferenceHelper.Exchange(ref _lastSnapshot, null);
            if (lastSnapshot is not null)
                Dereference(lastSnapshot);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CacheNode GetLastSnapshot()
    {
        CacheNode? snapshot = InterlockedHelper.Read(ref _lastSnapshot);
        if (snapshot is not null)
        {
            snapshot.EnterBarrier();
            try
            {
                if (!ReferenceEquals(this, snapshot.Owner))
                    goto Slow;
                snapshot.AddRef();
            }
            finally
            {
                snapshot.ExitBarrier();
            }
        }

    Slow:
        return GetSnapshotSlow();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private CacheNode GetSnapshotSlow()
    {
        lock (_syncLock)
        {
            CacheNode snapshot = Core();
            snapshot.AddRef();
            _lastSnapshot = snapshot;
            return snapshot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        CacheNode Core()
        {
            CacheNode? snapshot = _lastSnapshot;
            if (snapshot is not null)
            {
                snapshot.EnterBarrier();
                try
                {
                    if (!ReferenceEquals(this, snapshot.Owner))
                        goto Create;
                    snapshot.AddRef();
                }
                finally
                {
                    snapshot.ExitBarrier();
                }
                return snapshot;
            }

        Create:
            snapshot = _snapshotPool.Rent();

            ulong timestamp = InterlockedHelper.Read(ref _lastTimestamp);
            _createSnapshotFunc(_owner, snapshot);

            snapshot.AddRef();
            snapshot.Timestamp = timestamp;
            snapshot.Owner = this;
            _snapshotDict[timestamp] = snapshot;
            return snapshot;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dereference(CacheNode snapshot)
    {
        snapshot.EnterBarrier();
        try
        {
            if (!ReferenceEquals(this, snapshot.Owner) || snapshot.RemoveRef())
                return;
            lock (_syncLock)
            {
                DebugHelper.ThrowIf(ReferenceEquals(InterlockedHelper.Read(ref _lastSnapshot), snapshot));
                ((ICollection<KeyValuePair<ulong, CacheNode>>)_snapshotDict).Remove(KeyValuePair.Create(snapshot.Timestamp, snapshot));
            }
            _removeSnapshotFunc(_owner, snapshot);
            snapshot.CleanUp();
            _snapshotPool.Return(snapshot);
        }
        finally
        {
            snapshot.ExitBarrier();
        }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            CacheNode? lastSnapshot = ReferenceHelper.Exchange(ref _lastSnapshot, null);
            if (lastSnapshot is not null)
                Dereference(lastSnapshot);
        }
    }
}
