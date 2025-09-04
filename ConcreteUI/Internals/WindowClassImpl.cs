using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;
using ConcreteUI.Window;

using InlineIL;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Internals
{
    internal sealed class WindowClassImpl
    {
        private static readonly LazyTiny<WindowClassImpl> _instanceLazy =
            new LazyTiny<WindowClassImpl>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);

        public static WindowClassImpl Instance => _instanceLazy.Value;

        private readonly Dictionary<IntPtr, WeakReference<IHwndOwner>> _hwndOwnerDict;
        private readonly IntPtr _hInstance;
        private readonly ushort _atom;

        private WindowClassImpl(ushort atom, IntPtr hInstance)
        {
            _atom = atom;
            _hInstance = hInstance;
            _hwndOwnerDict = new Dictionary<nint, WeakReference<IHwndOwner>>();
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
                    lpszClassName = className,
                    hbrBackground = Gdi32.CreateSolidBrush(0x00000000)
                };
                IL.PushInRef(in clazz);
                IL.Emit.Ldftn(new MethodRef(typeof(WindowClassImpl), nameof(ProcessWindowMessage)));
                IL.Emit.Stfld(new FieldRef(typeof(WindowClassEx), nameof(WindowClassEx.lpfnWndProc)));

                ushort atom = User32.RegisterClassExW(&clazz);
                if (atom == 0)
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                return new WindowClassImpl(atom, hInstance);
            }
        }

#if NET8_0_OR_GREATER
        [UnmanagedCallersOnly]
#endif
        private static unsafe nint ProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam)
        {
            WindowClassImpl? instance = _instanceLazy.GetValueDirectly();
            if (instance is not null && instance.TryProcessWindowMessage(hwnd, (WindowMessage)message, wParam, lParam, out nint result))
                return result;
            return User32.DefWindowProcW(hwnd, message, wParam, lParam);
        }

        public bool TryRegisterWindow(IHwndOwner owner)
        {
            Dictionary<IntPtr, WeakReference<IHwndOwner>> dict = _hwndOwnerDict;
            lock (dict)
            {
                IntPtr handle = owner.Handle;
                if (handle == IntPtr.Zero || CheckExists(dict, handle))
                    return false;
                dict[handle] = new WeakReference<IHwndOwner>(owner);
                return true;
            }
        }

        public bool TryRegisterWindowUnsafe(IntPtr handle, IHwndOwner owner)
        {
            if (handle == IntPtr.Zero)
                return false;
            Dictionary<IntPtr, WeakReference<IHwndOwner>> dict = _hwndOwnerDict;
            lock (dict)
            {
                if (CheckExists(dict, handle))
                    return false;
                dict[handle] = new WeakReference<IHwndOwner>(owner);
            }
            return true;
        }

        public bool UnregisterWindow(IHwndOwner owner)
        {
            Dictionary<IntPtr, WeakReference<IHwndOwner>> dict = _hwndOwnerDict;
            lock (dict)
            {
                IntPtr handle = owner.Handle;
                if (handle == IntPtr.Zero || !CheckExists(dict, handle, owner))
                    return false;
                dict[handle] = new WeakReference<IHwndOwner>(owner);
                return true;
            }
        }

        public bool UnregisterWindowUnsafe(IntPtr handle, IHwndOwner owner)
        {
            if (handle == IntPtr.Zero)
                return false;
            Dictionary<IntPtr, WeakReference<IHwndOwner>> dict = _hwndOwnerDict;
            lock (dict)
            {
                if (!CheckExists(dict, handle, owner))
                    return false;
                dict.Remove(handle);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            Dictionary<IntPtr, WeakReference<IHwndOwner>> dict = _hwndOwnerDict;
            WeakReference<IHwndOwner>? ownerRef;
            lock (dict)
            {
                if (!dict.TryGetValue(hwnd, out ownerRef))
                    goto Failed;
            }
            if (ownerRef.TryGetTarget(out IHwndOwner? owner) && owner is not null && owner.TryProcessWindowMessage(hwnd, message, wParam, lParam, out result))
                return true;

            Failed:
            result = 0;
            return false;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckExists(Dictionary<IntPtr, WeakReference<IHwndOwner>> dict, nint handle)
        {
            return dict.TryGetValue(handle, out WeakReference<IHwndOwner>? ownerRef) &&
                    ownerRef.TryGetTarget(out IHwndOwner? oldOwner) &&
                    oldOwner is not null && oldOwner.Handle == handle;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckExists(Dictionary<IntPtr, WeakReference<IHwndOwner>> dict, nint handle, IHwndOwner owner)
        {
            return dict.TryGetValue(handle, out WeakReference<IHwndOwner>? ownerRef) &&
                    ownerRef.TryGetTarget(out IHwndOwner? oldOwner) &&
                    oldOwner is not null && ReferenceEquals(oldOwner, owner);
        }
    }
}
