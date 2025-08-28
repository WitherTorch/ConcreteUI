using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Native;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Window2
{
    internal sealed class WindowClassImpl
    {
        private static readonly LazyTiny<WindowClassImpl> _instanceLazy =
            new LazyTiny<WindowClassImpl>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);

        public static WindowClassImpl Instance => _instanceLazy.Value;

        private readonly ConcurrentDictionary<nint, IHwndOwner> _hwndOwnerDict;
        private readonly nint _hInstance;
        private readonly ushort _atom;

        private WindowClassImpl(ushort atom, nint hInstance)
        {
            _atom = atom;
            _hInstance = hInstance;
            _hwndOwnerDict = new ConcurrentDictionary<nint, IHwndOwner>();
        }

        public ushort Atom => _atom;
        public nint HInstance => _hInstance;

        private static unsafe WindowClassImpl CreateInstance()
        {
            nint hInstance = Kernel32.GetModuleHandleW(null);
            fixed (char* className = "ConcreteWindow")
            {
                WindowClassEx clazz = new WindowClassEx()
                {
                    cbSize = UnsafeHelper.SizeOf<WindowClassEx>(),
                    style = ClassStyles.ClassDC,
                    lpfnWndProc = (delegate*<nint, WindowMessage, nint, nint, nint>)&ProcessWindowMessage,
                    hInstance = hInstance,
                    lpszClassName = className,
                    hbrBackground = Gdi32.CreateSolidBrush(0x00000000)
                };

                ushort atom = User32.RegisterClassExW(&clazz);
                if (atom == 0)
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                return new WindowClassImpl(atom, hInstance);
            }
        }

        private static unsafe nint ProcessWindowMessage(nint hwnd, WindowMessage message, nint wParam, nint lParam)
        {
            WindowClassImpl? instance = _instanceLazy.GetValueDirectly();
            if (instance is not null && instance.TryProcessWindowMessage(hwnd, message, wParam, lParam, out nint result))
                return result;
            return User32.DefWindowProcW(hwnd, message, wParam, lParam);
        }

        public bool TryRegisterWindow(IHwndOwner owner) 
            => _hwndOwnerDict.TryAdd(owner.Handle, owner);

        public bool UnregisterWindow(nint handle, IHwndOwner owner)
        {
#if NET8_0_OR_GREATER
            return _hwndOwnerDict.TryRemove(System.Collections.Generic.KeyValuePair.Create(handle, owner));
#else
            return _hwndOwnerDict.TryRemove(handle, out owner);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryProcessWindowMessage(nint hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            if (_hwndOwnerDict.TryGetValue(hwnd, out IHwndOwner? owner) && owner.TryProcessWindowMessage(message, wParam, lParam, out result))
                return true;

            result = 0;
            return false;
        }
    }
}
