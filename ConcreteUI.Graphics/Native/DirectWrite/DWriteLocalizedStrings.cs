using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using LocalsInit;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

/// <summary>
/// Represents a collection of strings indexed by locale name.
/// </summary>
namespace ConcreteUI.Graphics.Native.DirectWrite
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteLocalizedStrings : ComObject
    {
        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetCount = _Start,
            FindLocaleName,
            GetLocaleNameLength,
            GetLocaleName,
            GetStringLength,
            GetString,
            _End
        }

        public DWriteLocalizedStrings() : base() { }

        public DWriteLocalizedStrings(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the number of language/string pairs.
        /// </summary>
        public uint Count => GetCount();

        [Inline(InlineBehavior.Remove)]
        private uint GetCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetCount);
            return ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="FindLocaleName(char*, uint*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindLocaleName(string localeName, out uint index)
        {
            fixed (char* ptr = localeName)
                return FindLocaleName(ptr, out index);
        }

        /// <inheritdoc cref="FindLocaleName(char*, uint*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindLocaleName(char* localeName, out uint index)
            => FindLocaleName(localeName, UnsafeHelper.AsPointerOut(out index));

        /// <summary>
        /// Gets the index of the item with the specified locale name.
        /// </summary>
        /// <param name="localeName">Locale name to look for.</param>
        /// <param name="index">Receives the zero-based index of the locale name/string pair.</param>
        /// <returns>
        /// <see langword="true"/> if the locale name exists or <see langword="false"/> if not.
        /// </returns>
        [LocalsInit(false)]
        public bool FindLocaleName(char* localeName, uint* index)
        {
            bool exists;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FindLocaleName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, uint*, bool*, int>)functionPointer)(nativePointer, localeName, index, &exists);
            ThrowHelper.ThrowExceptionForHR(hr);
            return exists;
        }

        /// <summary>
        /// Gets the length in characters (not including the null terminator) of the locale name with the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the locale name.</param>
        /// <returns>
        /// The length in characters, not including the null terminator.
        /// </returns>
        [LocalsInit(false)]
        public uint GetLocaleNameLength(uint index)
        {
            uint length;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleNameLength);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint*, int>)functionPointer)(nativePointer, index, &length);
            ThrowHelper.ThrowExceptionForHR(hr);
            return length;
        }

        /// <summary>
        /// Returns the locale name with the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the locale name.</param>
        /// <returns>
        /// The locale name.
        /// </returns>
        public string GetLocaleName(uint index)
        {
            uint length = GetLocaleNameLength(index);
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = StringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetLocaleName(index, ptr, length + 1);
            return result;
        }

        /// <summary>
        /// Copies the locale name with the specified index to the specified array.
        /// </summary>
        /// <param name="index">Zero-based index of the locale name.</param>
        /// <param name="localeName">Character array that receives the locale name.</param>
        /// <param name="size">Size of the array in characters. The size must include space for the terminating
        /// null character.</param>
        public void GetLocaleName(uint index, char* localeName, uint size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, char*, uint, int>)functionPointer)(nativePointer, index, localeName, size);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Gets the length in characters (not including the null terminator) of the string with the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the string.</param>
        /// <returns>
        /// The length in characters, not including the null terminator.
        /// </returns>
        [LocalsInit(false)]
        public uint GetStringLength(uint index)
        {
            uint length;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStringLength);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint*, int>)functionPointer)(nativePointer, index, &length);
            ThrowHelper.ThrowExceptionForHR(hr);
            return length;
        }

        /// <summary>
        /// Returns the string with the specified index.
        /// </summary>
        public string GetString(uint index)
        {
            uint length = GetStringLength(index);
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = StringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetString(index, ptr, length);
            return result;
        }

        /// <summary>
        /// Copies the string with the specified index to the specified array.
        /// </summary>
        /// <param name="index">Zero-based index of the string.</param>
        /// <param name="stringBuffer">Character array that receives the string.</param>
        /// <param name="size">Size of the array in characters. The size must include space for the terminating
        /// null character.</param>
        public void GetString(uint index, char* stringBuffer, uint size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetString);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, char*, uint, int>)functionPointer)(nativePointer, index, stringBuffer, size);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}