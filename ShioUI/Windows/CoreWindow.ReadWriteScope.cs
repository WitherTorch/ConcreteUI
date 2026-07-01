using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace ShioUI.Windows;

partial class CoreWindow
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ReadActiveElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadActiveElementsScope(CoreWindow window)
        {
            EnterReadLock(ref window._activeElementsCacheReaderCount, window._activeElementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly UpgradedWriteActiveElementsScope EnterWriterScope()
        {
            CoreWindow? window = _window;
            DebugHelper.ThrowIf(window is null);
            return new UpgradedWriteActiveElementsScope(window);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is not null)
                ExitReadLock(ref window._activeElementsCacheReaderCount);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private ref struct WriteActiveElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WriteActiveElementsScope(CoreWindow window)
        {
            EnterWriteLock(ref window._activeElementsCacheReaderCount, window._activeElementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is null)
                return;
            ExitWriteLock(window._activeElementsCacheWriterLock);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    internal ref struct UpgradedWriteActiveElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UpgradedWriteActiveElementsScope(CoreWindow window)
        {
            EnterUpgradedWriteLock(ref window._activeElementsCacheReaderCount, window._activeElementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is null)
                return;

            ExitUpgradedWriteLock(ref window._activeElementsCacheReaderCount, window._activeElementsCacheWriterLock);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct ReadElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadElementsScope(CoreWindow window)
        {
            EnterReadLock(ref window._elementsCacheReaderCount, window._elementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly UpgradedWriteElementsScope EnterWriterScope()
        {
            CoreWindow? window = _window;
            DebugHelper.ThrowIf(window is null);
            return new UpgradedWriteElementsScope(window);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is not null)
                ExitReadLock(ref window._elementsCacheReaderCount);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private ref struct WriteElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WriteElementsScope(CoreWindow window)
        {
            EnterWriteLock(ref window._elementsCacheReaderCount, window._elementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is null)
                return;
            ExitWriteLock(window._elementsCacheWriterLock);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    internal ref struct UpgradedWriteElementsScope : IDisposable
    {
        private CoreWindow? _window;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UpgradedWriteElementsScope(CoreWindow window)
        {
            EnterUpgradedWriteLock(ref window._elementsCacheReaderCount, window._elementsCacheWriterLock);
            _window = window;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CheckValidOrThrow(CoreWindow window)
        {
            CoreWindow? currentWindow = _window;
            if (!ReferenceEquals(currentWindow, window))
                InvalidOperationException.Throw("The scope is not valid for the specified window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CoreWindow? window = ReferenceHelper.Exchange(ref _window, null);
            if (window is null)
                return;

            ExitUpgradedWriteLock(ref window._elementsCacheReaderCount, window._elementsCacheWriterLock);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnterReadLock(ref nuint readerCountRef, Lock writerLock)
    {
        writerLock.Enter();
        InterlockedHelper.Increment(ref readerCountRef);
        writerLock.Exit();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExitReadLock(ref nuint readerCountRef) => InterlockedHelper.Decrement(ref readerCountRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnterWriteLock(ref readonly nuint readerCountRef, Lock writerLock)
    {
        SpinWait waiter = new SpinWait();
        while (true)
        {
            while (InterlockedHelper.Read(in readerCountRef) > 0)
                waiter.SpinOnce();
            writerLock.Enter();
            if (InterlockedHelper.Read(in readerCountRef) > 0)
            {
                writerLock.Exit();
                waiter.Reset();
                continue;
            }
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExitWriteLock(Lock writerLock) => writerLock.Exit();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnterUpgradedWriteLock(ref nuint readerCountRef, Lock writerLock)
    {
        ExitReadLock(ref readerCountRef);
        EnterWriteLock(ref readerCountRef, writerLock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExitUpgradedWriteLock(ref nuint readerCountRef, Lock writerLock)
    {
        InterlockedHelper.Increment(ref readerCountRef);
        ExitWriteLock(writerLock);
    }
}
