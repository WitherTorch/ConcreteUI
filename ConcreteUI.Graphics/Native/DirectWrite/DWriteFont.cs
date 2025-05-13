using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The <see cref="DWriteFont"/> class represents a physical font in a font collection.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteFont : ComObject
    {
        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetFontFamily = _Start,
            GetWeight,
            GetStretch,
            GetStyle,
            IsSymbolFont,
            GetFaceNames,
            GetInformationalStrings,
            GetSimulations,
            GetMetrics,
            HasCharacter,
            CreateFontFace,
            _End
        }

        public DWriteFont() : base() { }

        public DWriteFont(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the weight of the specified font.
        /// </summary>
        public DWriteFontWeight FontWeight => GetWeight();

        /// <summary>
        /// Gets the stretch (aka. width) of the specified font.
        /// </summary>
        public DWriteFontStretch FontStretch => GetStretch();

        /// <summary>
        /// Gets the style (aka. slope) of the specified font.
        /// </summary>
        public DWriteFontStyle FontStyle => GetStyle();

        /// <summary>
        /// Returns <see langword="true"/> if the font is a symbol font or <see langword="false"/> if not.
        /// </summary>
        public bool IsSymbolFont => IsSymbolFontCore();

        /// <summary>
        /// Gets a value that indicates what simulation are applied to the specified font.
        /// </summary>
        public DWriteFontSimulations Simulations => GetSimulations();

        /// <summary>
        /// Gets the font family to which the specified font belongs.
        /// </summary>
        /// <returns>
        /// The font family object.
        /// </returns>
        public DWriteFontFamily GetFontFamily()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamily);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteFontFamily(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontWeight GetWeight()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetWeight);
            return ((delegate* unmanaged[Stdcall]<void*, DWriteFontWeight>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontStretch GetStretch()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStretch);
            return ((delegate* unmanaged[Stdcall]<void*, DWriteFontStretch>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontStyle GetStyle()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStyle);
            return ((delegate* unmanaged[Stdcall]<void*, DWriteFontStyle>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private bool IsSymbolFontCore()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.IsSymbolFont);
            return ((delegate* unmanaged[Stdcall]<void*, bool>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Gets a localized strings collection containing the face names for the font (e.g., Regular or Bold), indexed by locale name.
        /// </summary>
        /// <returns>
        /// The newly created localized strings object.
        /// </returns>
        public DWriteLocalizedStrings GetFaceNames()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFaceNames);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteLocalizedStrings(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="GetInformationalStrings(DWriteInformationalStringId, bool*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteLocalizedStrings GetInformationalStrings(DWriteInformationalStringId informationalStringId, out bool exists)
            => GetInformationalStrings(informationalStringId, UnsafeHelper.AsPointerOut(out exists));

        /// <summary>
        /// Gets a localized strings collection containing the specified informational strings, indexed by locale name.
        /// </summary>
        /// <param name="informationalStringId">Identifies the string to get.</param>
        /// <param name="exists">Receives the value <see langword="true"/> if the font contains the specified string ID or <see langword="false"/> if not.</param>
        /// <returns>
        /// The newly created localized strings object.
        /// </returns>
        public DWriteLocalizedStrings GetInformationalStrings(DWriteInformationalStringId informationalStringId, bool* exists)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInformationalStrings);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteInformationalStringId, void**, bool*, int>)functionPointer)(nativePointer, informationalStringId,
                &nativePointer, exists);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteLocalizedStrings(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontSimulations GetSimulations()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSimulations);
            return ((delegate* unmanaged[Stdcall]<void*, DWriteFontSimulations>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="HasCharacter(uint)"/>
        /// <param name="charactor">Unicode (UCS-2) character value.</param>
        [Inline(InlineBehavior.Keep, export: true)]
        public bool HasCharacter(char charactor) => HasCharacter(unchecked((uint)charactor));

        /// <summary>
        /// Determines whether the font supports the specified character.
        /// </summary>
        /// <param name="unicodeValue">Unicode (UCS-4) character value.</param>
        /// <returns>
        /// <see langword="true"/> if the font supports the specified character or <see langword="false"/> if not.
        /// </returns>
        [LocalsInit(false)]
        public bool HasCharacter(uint unicodeValue)
        {
            bool exists;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.HasCharacter);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, bool*, int>)functionPointer)(nativePointer, unicodeValue, &exists);
            ThrowHelper.ThrowExceptionForHR(hr);
            return exists;
        }
    }
}