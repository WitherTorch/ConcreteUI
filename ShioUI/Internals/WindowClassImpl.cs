using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core.Helpers;

using ShioUI.Internals.Native;
using ShioUI.Windows;

namespace ShioUI.Internals;

internal sealed unsafe class WindowClassImpl
{
    public static readonly WindowClassImpl Instance;

#if NET472_OR_GREATER
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate nint WndProcDelegate(IntPtr hwnd, uint message, nint lParam, nint wParam);
    private static readonly WndProcDelegate? _wndProcDelegate;
#endif

    private readonly Dictionary<IntPtr, GCHandle> _hwndOwnerDict = new();
    private readonly Lock _hwndOwnerDictLock = new();
    private readonly IntPtr _hInstance;
    private readonly ushort _atom;

    static WindowClassImpl()
    {
        void* wndProcFunc;

#if NET8_0_OR_GREATER
        goto Direct;
#else
#if B64_ARCH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Type.GetType("Mono.Runtime") is null)
            goto Direct;
        else
            goto Indirect;
#elif B32_ARCH
        goto Indirect;
#elif ANYCPU
        if (PlatformHelper.IsX64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            goto Direct;
        goto Indirect;
#endif
#endif

    Direct:
#if NET8_0_OR_GREATER
        wndProcFunc = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)&ProcessWindowMessage;
#else
        wndProcFunc = (delegate* managed<IntPtr, uint, nint, nint, nint>)&ProcessWindowMessage;
#endif
        goto Tail;

#if !NET8_0_OR_GREATER
    Indirect:
        WndProcDelegate wndProcDelegate = ProcessWindowMessage;
        _wndProcDelegate = wndProcDelegate;
        wndProcFunc = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
        goto Tail;
#endif

    Tail:
        Instance = new WindowClassImpl(wndProcFunc);
    }

    private WindowClassImpl(void* wndProcFunc)
    {
        ushort atom;
        IntPtr hInstance = Kernel32.GetModuleHandleW(null);
        fixed (char* className = "ShioWindow")
        {
            WindowClassEx clazz = new WindowClassEx()
            {
                cbSize = UnsafeHelper.SizeOf<WindowClassEx>(),
                style = ClassStyles.OwnDC,
                hInstance = hInstance,
                lpfnWndProc = wndProcFunc,
                lpszClassName = className,
                hbrBackground = Gdi32.CreateSolidBrush(0x00000000)
            };

            atom = User32.RegisterClassExW(&clazz);
            if (atom == 0)
                throw new Win32Exception(Kernel32.GetLastError());
        }

        _hInstance = hInstance;
        _atom = atom;
    }

    public ushort Atom => _atom;
    public IntPtr HInstance => _hInstance;

#if NET8_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam)
    {
        WindowClassImpl instance = Instance;
        try
        {
            if (instance.TryProcessWindowMessage(hwnd, message, wParam, lParam, out nint result))
                return result;
        }
        catch (Exception ex)
        {
            MessageLoopExceptionEventHandler? eventHandler = WindowMessageLoop.GetExceptionEventHandler();
            if (eventHandler is null)
                throw;
            eventHandler.Invoke(null, new MessageLoopExceptionEventArgs(ex));
        }
        return User32.DefWindowProcW(hwnd, message, wParam, lParam);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRegisterWindow<T>(T owner) where T : IHwndOwner
        => TryRegisterWindowUnsafe(owner.Handle, owner);

    public bool TryRegisterWindowUnsafe<T>(IntPtr handle, T owner) where T : IHwndOwner
    {
        if (handle == IntPtr.Zero)
            return false;

        Dictionary<IntPtr, GCHandle> dict = _hwndOwnerDict;
        lock (_hwndOwnerDictLock)
        {
            if (!dict.TryGetValue(handle, out GCHandle weakRef))
            {
                dict.Add(handle, GCHandle.Alloc(owner, GCHandleType.Weak));
                return true;
            }
            object? target = weakRef.Target;
            if (ReferenceEquals(target, owner))
                return true;
            if (target is null || (target is IHwndOwner otherOwner && otherOwner.Handle == owner.Handle))
            {
                weakRef.Target = owner;
                return true;
            }
            return false;
        }
    }

    public bool TryUnregisterWindow<T>(T owner) where T : IHwndOwner
        => TryUnregisterWindowUnsafe(owner.Handle, owner);

    public bool TryUnregisterWindowUnsafe<T>(IntPtr handle, T owner) where T : IHwndOwner
    {
        if (handle == IntPtr.Zero)
            return false;

        Dictionary<IntPtr, GCHandle> dict = _hwndOwnerDict;
        lock (_hwndOwnerDictLock)
        {
            if (!dict.TryGetValue(handle, out GCHandle weakRef))
                return false;
            object? target = weakRef.Target;
            if (ReferenceEquals(target, owner))
            {
                dict.Remove(handle);
                weakRef.Free();
                return true;
            }
            if (target is null || (target is IHwndOwner otherOwner && otherOwner.Handle == owner.Handle))
            {
                dict.Remove(handle);
                weakRef.Free();
            }
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam, out nint result)
    {
        IHwndOwner? owner;
        lock (_hwndOwnerDictLock)
        {
            if (!_hwndOwnerDict.TryGetValue(hwnd, out GCHandle weakRef) || (owner = weakRef.Target as IHwndOwner) is null)
                goto Failed;
        }

        return owner.TryProcessWindowMessage(hwnd, (WindowMessage)message, wParam, lParam, out result);

    Failed:
        result = 0;
        return false;
    }
}
