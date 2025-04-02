using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.CrossNative;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The <see cref="DWriteFontFamily"> class represents a set of fonts that share the same design but are differentiated
    /// by weight, stretch, and style.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteFontFamily : DWriteFontList
    {
        private new enum MethodTable
        {
            _Start = DWriteFontList.MethodTable._End,
            GetFamilyNames = _Start,
            GetFirstMatchingFont,
            GetMatchingFonts,
            _End,
        }

        public DWriteFontFamily() : base() { }

        public DWriteFontFamily(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Creates a localized strings object that contains the family names for the font family, indexed by locale name.
        /// </summary>
        /// <returns>
        ///  The newly created localized strings object.
        /// </returns>
        public DWriteLocalizedStrings GetFamilyNames()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFamilyNames);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteLocalizedStrings(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Gets the font that best matches the specified properties.
        /// </summary>
        /// <param name="weight">Requested font weight.</param>
        /// <param name="stretch">Requested font stretch.</param>
        /// <param name="style">Requested font style.</param>
        /// <returns>
        /// The newly created font object.
        /// </returns>
        public DWriteFont GetFirstMatchingFont(DWriteFontWeight weight, DWriteFontStretch stretch, DWriteFontStyle style)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFirstMatchingFont);
            int hr = ((delegate*<void*, DWriteFontWeight, DWriteFontStretch, DWriteFontStyle, void**, int>)functionPointer)(nativePointer,
                weight, stretch, style, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteFont(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Gets a list of fonts in the font family ranked in order of how well they match the specified properties.
        /// </summary>
        /// <param name="weight">Requested font weight.</param>
        /// <param name="stretch">Requested font stretch.</param>
        /// <param name="style">Requested font style.</param>
        /// <returns>
        /// The newly created font list object.
        /// </returns>
        public DWriteFontList GetMatchingFonts(DWriteFontWeight weight, DWriteFontStretch stretch, DWriteFontStyle style)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMatchingFonts);
            int hr = ((delegate*<void*, DWriteFontWeight, DWriteFontStretch, DWriteFontStyle, void**, int>)functionPointer)(nativePointer,
                weight, stretch, style, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteFontList(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}