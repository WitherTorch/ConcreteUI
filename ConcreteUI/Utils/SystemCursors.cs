using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;

using WitherTorch.Common.Threading;

namespace ConcreteUI.Utils
{
    public static class SystemCursors
    {
        private static readonly LazyTiny<Win32ImageHandle>[] _cursorLazies = new LazyTiny<Win32ImageHandle>[(int)SystemCursorType._Last]
        {
            new (new CursorClosure(32512, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32513, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32514, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32515, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32516, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32642, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32643, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32644, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32645, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32646, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32648, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32649, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32650, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32651, mayFailed: false).Create, LazyThreadSafetyMode.PublicationOnly),

            // Only for Windows 10 or higher version
            new (new CursorClosure(32671, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32672, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),

            // Undocumented in winuser.h, but recorded on MSDN (https://learn.microsoft.com/en-us/windows/win32/menurc/about-cursors)
            new (new CursorClosure(32631, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32652, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32653, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32654, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32655, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32656, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32657, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32658, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32659, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32660, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32661, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32662, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
            new (new CursorClosure(32663, mayFailed: true).Create, LazyThreadSafetyMode.PublicationOnly),
        };

        public static Win32ImageHandle Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Default].Value;
        }

        public static Win32ImageHandle Arrow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Arrow].Value;
        }

        public static Win32ImageHandle IBeam
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.IBeam].Value;
        }

        public static Win32ImageHandle Wait
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Wait].Value;
        }

        public static Win32ImageHandle Cross
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Cross].Value;
        }

        public static Win32ImageHandle UpArrow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.UpArrow].Value;
        }

        public static Win32ImageHandle SizeNWSE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.SizeNWSE].Value;
        }

        public static Win32ImageHandle SizeNESW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.SizeNESW].Value;
        }

        public static Win32ImageHandle SizeWE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.SizeWE].Value;
        }

        public static Win32ImageHandle SizeNS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.SizeNS].Value;
        }

        public static Win32ImageHandle SizeAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.SizeAll].Value;
        }

        public static Win32ImageHandle No
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.No].Value;
        }

        public static Win32ImageHandle Hand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Hand].Value;
        }

        public static Win32ImageHandle AppStarting
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.AppStarting].Value;
        }

        public static Win32ImageHandle Help
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Help].Value;
        }

        public static Win32ImageHandle Pin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Pin].Value;
        }

        public static Win32ImageHandle Person
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Person].Value;
        }

        public static Win32ImageHandle Pen
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Pen].Value;
        }

        public static Win32ImageHandle ScrollingNS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingNS].Value;
        }

        public static Win32ImageHandle ScrollingWE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingWE].Value;
        }

        public static Win32ImageHandle ScrollingAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingAll].Value;
        }

        public static Win32ImageHandle ScrollingNorth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingNorth].Value;
        }

        public static Win32ImageHandle ScrollingSouth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingSouth].Value;
        }

        public static Win32ImageHandle ScrollingWest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingWest].Value;
        }

        public static Win32ImageHandle ScrollingEast
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingEast].Value;
        }

        public static Win32ImageHandle ScrollingNW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingNW].Value;
        }

        public static Win32ImageHandle ScrollingNE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingNE].Value;
        }

        public static Win32ImageHandle ScrollingSW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingSW].Value;
        }

        public static Win32ImageHandle ScrollingSE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.ScrollingSE].Value;
        }

        public static Win32ImageHandle Disk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cursorLazies[(int)SystemCursorType.Disk].Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Win32ImageHandle GetSystemCursor(SystemCursorType type)
            => _cursorLazies[(int)type].Value;

        private sealed unsafe class CursorClosure
        {
            private readonly char* _ptr;
            private readonly bool _mayFailed;

            public CursorClosure(int id, bool mayFailed)
            {
                _ptr = (char*)id;
                _mayFailed = mayFailed;
            }

            public Win32ImageHandle Create()
            {
                IntPtr handle = User32.LoadImageW(IntPtr.Zero, _ptr, Win32ImageType.Cursor, 0, 0,
                    LoadOrCopyImageOptions.DefaultSize | LoadOrCopyImageOptions.DefaultColor | LoadOrCopyImageOptions.Shared);
                if (handle == IntPtr.Zero)
                {
                    if (_mayFailed)
                    {
                        handle = User32.LoadImageW(IntPtr.Zero, (char*)32512, Win32ImageType.Cursor, 0, 0,
                            LoadOrCopyImageOptions.DefaultSize | LoadOrCopyImageOptions.DefaultColor | LoadOrCopyImageOptions.Shared);
                        if (handle == IntPtr.Zero)
                            Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                    }
                    Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                }
                return new Win32ImageHandle(handle, Win32ImageType.Cursor, ownsHandle: false);
            }
        }
    }

    public enum SystemCursorType
    {
        Default = 0,
        Arrow = Default,
        IBeam,
        Wait,
        Cross,
        UpArrow,
        SizeNWSE,
        SizeNESW,
        SizeWE,
        SizeNS,
        SizeAll,
        No,
        Hand,
        AppStarting,
        Help,
        Pin,
        Person,
        Pen,
        ScrollingNS,
        ScrollingWE,
        ScrollingAll,
        ScrollingNorth,
        ScrollingSouth,
        ScrollingWest,
        ScrollingEast,
        ScrollingNW,
        ScrollingNE,
        ScrollingSW,
        ScrollingSE,
        Disk,
        _Last
    }
}
