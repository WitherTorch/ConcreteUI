using System.Threading;

using RiceTea.Core.Helpers;

namespace ShioUI.Internals;

partial class CacheStore<T>
{
    public sealed class CacheNode
    {
        public CacheStore<T>? Owner;
        public T[]? Array;
        public int Count;
        public ulong Timestamp;

        private nuint _refCount;
        private nuint _barrier;

        public void AddRef() => _refCount = MathHelper.Min(_refCount + 1, UnsafeHelper.GetMaxValue<nuint>());

        public bool RemoveRef()
        {
            nuint refCount = _refCount;
            _refCount = MathHelper.Max(refCount - 1, 0);
            return refCount != 1;
        }

        public void EnterBarrier()
        {
            ref nuint barrier = ref _barrier;
            while (InterlockedHelper.Exchange(ref barrier, 1) != 0)
            {
                SpinWait waiter = new SpinWait();
                do
                {
                    waiter.SpinOnce();
                } while (InterlockedHelper.Read(ref barrier) != 0);
            }
        }

        public void ExitBarrier() => InterlockedHelper.Exchange(ref _barrier, 0);

        public void CleanUp()
        {
            Owner = null;
            Array = null;
            Count = 0;
            Timestamp = 0;
        }
    }
}
