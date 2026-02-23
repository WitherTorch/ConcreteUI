using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class D2D1Device : D2D1Resource
    {
        public static readonly Guid IID_ID2D1Device = new Guid(0x47dd575d, 0xac05, 0x4cdd, 0x80, 0x49, 0x9b, 0x02, 0xcd, 0x16, 0xf4, 0x4c);

        private new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            CreateDeviceContext = _Start,
            CreatePrintControl,
            SetMaximumTextureMemory,
            GetMaximumTextureMemory,
            ClearResources,
            _End
        }

        public D2D1Device() : base() { }

        public D2D1Device(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCreate(DXGIDevice device, [NotNullWhen(true)] out D2D1Device? d2dDevice)
            => TryCreate(device, null, out d2dDevice);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCreate(DXGIDevice device, in D2D1CreationProperties creationProperties, [NotNullWhen(true)] out D2D1Device? d2dDevice)
            => TryCreate(device, UnsafeHelper.AsPointerIn(in creationProperties), out d2dDevice);

        public static bool TryCreate(DXGIDevice device, D2D1CreationProperties* creationProperties, [NotNullWhen(true)] out D2D1Device? d2dDevice)
        {
            void* nativePointer;
            int hr = D2D1.D2D1CreateDevice(device.NativePointer, creationProperties, &nativePointer);
            if (hr < 0 || nativePointer is null)
            {
                d2dDevice = null;
                return false;
            }
            d2dDevice = new D2D1Device(nativePointer, ReferenceType.Owned);
            return true;
        }

        /// <summary>
        /// Gets or sets the maximum amount of texture memory to maintain before evicting caches.
        /// </summary>
        public ulong MaximumTextureMemory
        {
            get => GetMaximumTextureMemory();
            set => SetMaximumTextureMemory(value);
        }

        /// <summary>
        /// Creates a new device context with no initially assigned target.
        /// </summary>
        public D2D1DeviceContext CreateDeviceContext(D2D1DeviceContextOptions options)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateDeviceContext);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1DeviceContextOptions, void**, int>)functionPointer)(nativePointer, options, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1DeviceContext(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMaximumTextureMemory(ulong maximumInBytes)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMaximumTextureMemory);
            ((delegate* unmanaged[Stdcall]<void*, ulong, void>)functionPointer)(nativePointer, maximumInBytes);
        }

        [Inline(InlineBehavior.Remove)]
        private ulong GetMaximumTextureMemory()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaximumTextureMemory);
            return ((delegate* unmanaged[Stdcall]<void*, ulong>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Clears all resources that are cached but not held in use by the application
        /// through an interface reference.
        /// </summary>
        public void ClearResources(uint millisecondsSinceUse = 0)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ClearResources);
            ((delegate* unmanaged[Stdcall]<void*, uint, void>)functionPointer)(nativePointer, millisecondsSinceUse);
        }
    }
}
