using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The root factory class for all DWrite objects.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteFactory : ComObject
    {
        public static readonly Guid IID_DWriteFactory = new Guid(0xb859ee5a, 0xd838, 0x4b5b, 0xa2, 0xe8, 0x1a, 0xdc, 0x7d, 0x93, 0xdb, 0x48);

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetSystemFontCollection = _Start,
            CreateCustomFontCollection,
            RegisterFontCollectionLoader,
            UnregisterFontCollectionLoader,
            CreateFontFileReference,
            CreateCustomFontFileReference,
            CreateFontFace,
            CreateRenderingParams,
            CreateMonitorRenderingParams,
            CreateCustomRenderingParams,
            RegisterFontFileLoader,
            UnregisterFontFileLoader,
            CreateTextFormat,
            CreateTypography,
            GetGdiInterop,
            CreateTextLayout,
            CreateGdiCompatibleTextLayout,
            CreateEllipsisTrimmingSign,
            CreateTextAnalyzer,
            CreateNumberSubstitution,
            CreateGlyphRunAnalysis,
            _End
        }

        public DWriteFactory() : base() { }

        public DWriteFactory(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [LocalsInit(false)]
        public static DWriteFactory Create(DWriteFactoryType factoryType = DWriteFactoryType.Shared)
        {
            void* nativePointer;
            Guid guid = IID_DWriteFactory;
            int hr = DWrite.DWriteCreateFactory(factoryType, &guid, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteFactory(nativePointer, ReferenceType.Owned);
        }

        /// <summary>
        /// Gets a font collection representing the set of installed fonts.
        /// </summary>
        /// <param name="checkForUpdates">
        /// If this parameter is <see langword="true"/>, the function performs an immediate check for changes to the set of installed fonts. <br/>
        /// If this parameter is <see langword="false"/>, the function will still detect changes if the font cache service is running, but there may be some latency. <br/>
        /// For example, an application might specify <see langword="true"/> if it has itself just installed a font and wants to be sure the font collection contains that font.
        /// </param>
        /// <returns>
        /// The system font collection object.
        /// </returns>
        public DWriteFontCollection GetSystemFontCollection(bool checkForUpdates = false)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSystemFontCollection);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, bool, int>)functionPointer)(nativePointer, &nativePointer, checkForUpdates);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteFontCollection(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateTextFormat(char*, DWriteFontCollection, DWriteFontWeight, DWriteFontStyle, DWriteFontStretch, float, char*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteTextFormat CreateTextFormat(string fontFamilyName, float fontSize,
            DWriteFontWeight fontWeight = DWriteFontWeight.Normal, DWriteFontStyle fontStyle = DWriteFontStyle.Normal,
            DWriteFontStretch fontStretch = DWriteFontStretch.Normal)
        {
            fixed (char* ptr = fontFamilyName, ptr2 = string.Empty)
                return CreateTextFormat(ptr, null, fontWeight, fontStyle, fontStretch, fontSize, ptr2);
        }

        /// <inheritdoc cref="CreateTextFormat(char*, DWriteFontCollection, DWriteFontWeight, DWriteFontStyle, DWriteFontStretch, float, char*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteTextFormat CreateTextFormat(string fontFamilyName, DWriteFontCollection? fontCollection,
            DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, DWriteFontStretch fontStretch, float fontSize, string localeName)
        {
            fixed (char* ptr = fontFamilyName, ptr2 = localeName)
                return CreateTextFormat(ptr, fontCollection, fontWeight, fontStyle, fontStretch, fontSize, ptr2);
        }

        /// <summary>
        /// Create a text format object used for text layout.
        /// </summary>
        /// <param name="fontFamilyName">Name of the font family</param>
        /// <param name="fontCollection">Font collection. <see langword="null"/> indicates the system font collection.</param>
        /// <param name="fontWeight">Font weight</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="fontStretch">Font stretch</param>
        /// <param name="fontSize">Logical size of the font in DIP units. A DIP ("device-independent pixel") equals 1/96 inch.</param>
        /// <param name="localeName">Locale name</param>
        /// <returns>
        /// A newly created text format object.
        /// </returns>
        /// <remarks>
        /// If fontCollection is <see langword="null"/>, the system font collection is used, grouped by typographic family name
        /// (<see cref="DWriteFontFamilyModel.WeightStretchStyle"/>) without downloadable fonts.
        /// </remarks>

        public DWriteTextFormat CreateTextFormat(char* fontFamilyName, DWriteFontCollection? fontCollection,
            DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, DWriteFontStretch fontStretch, float fontSize, char* localeName)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateTextFormat);
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, void*, DWriteFontWeight, DWriteFontStyle, DWriteFontStretch, float, char*, void**, int>)functionPointer)(
                nativePointer, fontFamilyName, fontCollection == null ? null : fontCollection.NativePointer,
                fontWeight, fontStyle, fontStretch, fontSize, localeName, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteTextFormat(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateTextLayout(char*, uint, DWriteTextFormat, float, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteTextLayout CreateTextLayout(char text, DWriteTextFormat textFormat)
            => CreateTextLayout(&text, 1u, textFormat, float.PositiveInfinity, float.PositiveInfinity);

        /// <inheritdoc cref="CreateTextLayout(char*, uint, DWriteTextFormat, float, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteTextLayout CreateTextLayout(char text, DWriteTextFormat textFormat, float maxWidth, float maxHeight)
            => CreateTextLayout(&text, 1u, textFormat, maxWidth, maxHeight);

        /// <inheritdoc cref="CreateTextLayout(char*, uint, DWriteTextFormat, float, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteTextLayout CreateTextLayout(string text, DWriteTextFormat textFormat)
            => CreateTextLayout(text, textFormat, float.PositiveInfinity, float.PositiveInfinity);

        /// <inheritdoc cref="CreateTextLayout(char*, uint, DWriteTextFormat, float, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteTextLayout CreateTextLayout(string text, DWriteTextFormat textFormat, float maxWidth, float maxHeight)
        {
            fixed (char* ptr = text)
                return CreateTextLayout(ptr, unchecked((uint)text.Length), textFormat, maxWidth, maxHeight);
        }

        /// <inheritdoc cref="CreateTextLayout(char*, uint, DWriteTextFormat, float, float)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteTextLayout CreateTextLayout(char* text, uint textLength, DWriteTextFormat textFormat)
            => CreateTextLayout(text, textLength, textFormat, float.PositiveInfinity, float.PositiveInfinity);

        /// <summary>
        /// CreateTextLayout takes a string, format, and associated constraints
        /// and produces an object representing the fully analyzed
        /// and formatted result.
        /// </summary>
        /// <param name="text">The text to layout.</param>
        /// <param name="textLength">The length of the <paramref name="text"/>.</param>
        /// <param name="textFormat">The format to apply to the string.</param>
        /// <param name="maxWidth">Width of the layout box.</param>
        /// <param name="maxHeight">Height of the layout box.</param>
        /// <returns>
        /// A new text layout object.
        /// </returns>
        public DWriteTextLayout CreateTextLayout(char* text, uint textLength, DWriteTextFormat textFormat, float maxWidth, float maxHeight)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateTextLayout);
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, uint, void*, float, float, void**, int>)functionPointer)(nativePointer, text, textLength,
                textFormat.NativePointer, maxWidth, maxHeight, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteTextLayout(nativePointer, ReferenceType.Owned);
        }
    }
}