using System;
using System.Runtime.CompilerServices;
using System.Security;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class DXGI
    {
        private const string DXGI_DLL = "dxgi.dll";

        private static readonly void*[] _pointers = MethodImportHelper.GetImportedMethodPointers(DXGI_DLL,
            nameof(CreateDXGIFactory), nameof(CreateDXGIFactory1), nameof(CreateDXGIFactory2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateDXGIFactory(Guid* riid, void** pFactory)
        {
            void* pointer = _pointers[0];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<Guid*, void**, int>)pointer)(riid, pFactory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateDXGIFactory1(Guid* riid, void** pFactory)
        {
            void* pointer = _pointers[1];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<Guid*, void**, int>)pointer)(riid, pFactory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateDXGIFactory2(DXGICreateFactoryFlags flags, Guid* riid, void** pFactory)
        {
            void* pointer = _pointers[2];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<DXGICreateFactoryFlags, Guid*, void**, int>)pointer)(flags, riid, pFactory);
        }
    }
}
