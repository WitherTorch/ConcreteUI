using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;
using ConcreteUI.Window;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Internals
{
    internal sealed partial class WindowClassImpl
    {
        private static readonly LazyTiny<WindowClassImpl> _instanceLazy =
            new LazyTiny<WindowClassImpl>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly List<Exception> _exceptionList = new List<Exception>();

        public static WindowClassImpl Instance => _instanceLazy.Value;

        private readonly OptimisticLock<Dictionary<IntPtr, WeakReference<IHwndOwner>>> _hwndOwnerDictWithLock;
        private readonly IntPtr _hInstance;
        private readonly ushort _atom;

        private WindowClassImpl(ushort atom, IntPtr hInstance)
        {
            _atom = atom;
            _hInstance = hInstance;
            _hwndOwnerDictWithLock = OptimisticLock.Create(new Dictionary<IntPtr, WeakReference<IHwndOwner>>());
        }

        public ushort Atom => _atom;
        public IntPtr HInstance => _hInstance;

        private static unsafe WindowClassImpl CreateInstance()
        {
            IntPtr hInstance = Kernel32.GetModuleHandleW(null);
            fixed (char* className = "ConcreteWindow")
            {
                WindowClassEx clazz = new WindowClassEx()
                {
                    cbSize = UnsafeHelper.SizeOf<WindowClassEx>(),
                    style = ClassStyles.ClassDC,
                    hInstance = hInstance,
                    lpfnWndProc = GetWndProcPointer(),
                    lpszClassName = className,
                    hbrBackground = Gdi32.CreateSolidBrush(0x00000000)
                };

                ushort atom = User32.RegisterClassExW(&clazz);
                if (atom == 0)
                    Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                return new WindowClassImpl(atom, hInstance);
            }
        }

        private static unsafe partial delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint> GetWndProcPointer();

        private static unsafe partial nint ProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam)
        {
            try
            {
                WindowClassImpl? instance = _instanceLazy.GetValueDirectly();
                if (instance is not null && instance.TryProcessWindowMessage(hwnd, (WindowMessage)message, wParam, lParam, out nint result))
                    return result;
            }
            catch (Exception ex)
            {
                List<Exception> exceptionList = _exceptionList;
                lock (exceptionList)
                    exceptionList.Add(ex);
                return 0;
            }
            return User32.DefWindowProcW(hwnd, message, wParam, lParam);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PumpExceptions()
        {
            List<Exception> exceptionList = _exceptionList;
            lock (exceptionList)
            {
                int count = exceptionList.Count;
                if (count <= 0)
                    return;
                try
                {
                    if (count == 1)
                        throw exceptionList[0];

                    throw new AggregateException(exceptionList.ToArray());
                }
                finally
                {
                    exceptionList.Clear();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRegisterWindow(IHwndOwner owner)
            => TryRegisterWindowUnsafe(owner.Handle, owner);

        public bool TryRegisterWindowUnsafe(IntPtr handle, IHwndOwner owner)
        {
            if (handle == IntPtr.Zero)
                return false;

            OptimisticLock<Dictionary<nint, WeakReference<IHwndOwner>>> dictWithLock = _hwndOwnerDictWithLock;
            if (dictWithLock.Read(dict => CheckExists(dict, handle)))
                return false;
            WeakReference<IHwndOwner> ownerRef = new WeakReference<IHwndOwner>(owner);
            dictWithLock.Write(dict => dict[handle] = ownerRef);
            return true;
        }

        public bool TryUnregisterWindow(IHwndOwner owner)
            => TryUnregisterWindowUnsafe(owner.Handle, owner);

        public bool TryUnregisterWindowUnsafe(IntPtr handle, IHwndOwner owner)
        {
            if (handle == IntPtr.Zero)
                return false;

            OptimisticLock<Dictionary<nint, WeakReference<IHwndOwner>>> dictWithLock = _hwndOwnerDictWithLock;
            if (dictWithLock.Read(dict => !CheckExists(dict, handle, owner)))
                return false;
            dictWithLock.Write(dict => dict.Remove(handle));
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            WeakReference<IHwndOwner>? ownerRef = _hwndOwnerDictWithLock.Read(
                dict => dict.TryGetValue(hwnd, out WeakReference<IHwndOwner>? result) ? result : null);

            if (ownerRef is not null && ownerRef.TryGetTarget(out IHwndOwner? owner) &&
                owner is not null && owner.TryProcessWindowMessage(hwnd, message, wParam, lParam, out result))
                return true;

            result = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckExists(Dictionary<IntPtr, WeakReference<IHwndOwner>> dict, IntPtr handle)
        {
            return dict.TryGetValue(handle, out WeakReference<IHwndOwner>? ownerRef) &&
                    ownerRef.TryGetTarget(out IHwndOwner? oldOwner) &&
                    oldOwner is not null && oldOwner.Handle == handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckExists(Dictionary<IntPtr, WeakReference<IHwndOwner>> dict, IntPtr handle, IHwndOwner owner)
        {
            return dict.TryGetValue(handle, out WeakReference<IHwndOwner>? ownerRef) &&
                    ownerRef.TryGetTarget(out IHwndOwner? oldOwner) &&
                    oldOwner is not null && ReferenceEquals(oldOwner, owner);
        }
    }
}
