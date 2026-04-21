using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    partial class ScrollableElementBase
    {
        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;

                OnEnableChanged(value);
                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        protected bool DrawWhenDisabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _drawWhenDisabled;
            set
            {
                if (_drawWhenDisabled == value)
                    return;
                _drawWhenDisabled = value;

                Update(ScrollableElementUpdateFlags.All);
            }
        }

        protected ScrollBarType ScrollBarType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scrollBarType;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_scrollBarType == value)
                    return;
                _scrollBarType = value;

                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        protected Size SurfaceSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Size result;
                ref readonly nuint versionRef = ref _surfaceSizeVersion;
                nuint version = OptimisticLock.Enter(in versionRef);
                do
                {
                    result = BoundsHelper.ConvertUInt64ToSize(in _surfaceSizeRaw);
                } while (!OptimisticLock.TryLeave(in versionRef, ref version));
                return result;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ulong castedValue = BoundsHelper.ConvertSizeToUInt64(value);
                if (InterlockedHelper.Exchange(ref _surfaceSizeRaw, castedValue) == castedValue)
                    return;
                OptimisticLock.Increase(ref _surfaceSizeVersion);
                Update(ScrollableElementUpdateFlags.RecalcLayout);
            }
        }

        public Point ViewportPoint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Point result;
                ref readonly nuint versionRef = ref _viewportPointVersion;
                nuint version = OptimisticLock.Enter(in versionRef);
                do
                {
                    result = BoundsHelper.ConvertUInt64ToPoint(in _viewportPointRaw);
                } while (!OptimisticLock.TryLeave(in versionRef, ref version));
                return result;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set
            {
                ulong castedValue = BoundsHelper.ConvertPointToUInt64(value);
                if (InterlockedHelper.Exchange(ref _viewportPointRaw, castedValue) == castedValue)
                    return;
                OptimisticLock.Increase(ref _viewportPointVersion);
                Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
            }
        }

        protected Rectangle ContentBounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Point contentLocation;
                Size contentSize;
                ref readonly nuint versionRef = ref _contentBoundsVersion;
                nuint version = OptimisticLock.Enter(in versionRef);
                do
                {
                    contentLocation = BoundsHelper.ConvertUInt64ToPoint(in _contentLocationRaw);
                    contentSize = BoundsHelper.ConvertUInt64ToSize(in _contentSizeRaw);
                } while (!OptimisticLock.TryLeave(in versionRef, ref version));
                return new Rectangle(contentLocation, contentSize);
            }
        }

        protected Point ContentLocation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Point contentLocation;
                ref readonly nuint versionRef = ref _contentBoundsVersion;
                nuint version = OptimisticLock.Enter(in versionRef);
                do
                {
                    contentLocation = BoundsHelper.ConvertUInt64ToPoint(in _contentLocationRaw);
                } while (!OptimisticLock.TryLeave(in versionRef, ref version));
                return contentLocation;
            }
        }

        protected Size ContentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Size contentSize;
                ref readonly nuint versionRef = ref _contentBoundsVersion;
                nuint version = OptimisticLock.Enter(in versionRef);
                do
                {
                    contentSize = BoundsHelper.ConvertUInt64ToSize(in _contentSizeRaw);
                } while (!OptimisticLock.TryLeave(in versionRef, ref version));
                return contentSize;
            }
        }

        protected bool StickBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _stickBottom;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _stickBottom = value;
        }

        public string ScrollBarThemePrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scrollBarThemePrefix;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _scrollBarThemePrefix = value;
        }
    }
}
