using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;


using WitherTorch.CrossNative;
using WitherTorch.CrossNative.Windows;
using WitherTorch.CrossNative.Helpers;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The format of text used for text layout.
    /// </summary>
    /// <remarks>
    /// This object may not be thread-safe and it may carry the state of text format change.
    /// </remarks>

    [SuppressUnmanagedCodeSecurity]
    public unsafe class DWriteTextFormat : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            SetTextAlignment = _Start,
            SetParagraphAlignment,
            SetWordWrapping,
            SetReadingDirection,
            SetFlowDirection,
            SetIncrementalTabStop,
            SetTrimming,
            SetLineSpacing,
            GetTextAlignment,
            GetParagraphAlignment,
            GetWordWrapping,
            GetReadingDirection,
            GetFlowDirection,
            GetIncrementalTabStop,
            GetTrimming,
            GetLineSpacing,
            GetFontCollection,
            GetFontFamilyNameLength,
            GetFontFamilyName,
            GetFontWeight,
            GetFontStyle,
            GetFontStretch,
            GetFontSize,
            GetLocaleNameLength,
            GetLocaleName,
            _End
        }

        public DWriteTextFormat() : base() { }

        public DWriteTextFormat(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Get or set alignment option of text relative to layout box's leading and trailing edge.
        /// </summary>
        public DWriteTextAlignment TextAlignment
        {
            get => GetTextAlignment();
            set => SetTextAlignment(value);
        }

        /// <summary>
        /// Get or set alignment option of paragraph relative to layout box's top and bottom edge.
        /// </summary>
        public DWriteParagraphAlignment ParagraphAlignment
        {
            get => GetParagraphAlignment();
            set => SetParagraphAlignment(value);
        }

        /// <summary>
        /// Set word wrapping option.
        /// </summary>
        public DWriteWordWrapping WordWrapping
        {
            get => GetWordWrapping();
            set => SetWordWrapping(value);
        }

        /// <summary>
        /// Get or set paragraph reading direction.
        /// </summary>
        /// <remarks>
        /// The flow direction must be perpendicular to the reading direction.<br/>
        /// Setting both to a vertical direction or both to horizontal yields
        /// DWRITE_E_FLOWDIRECTIONCONFLICTS when calling GetMetrics or Draw.
        /// </remarks>
        public DWriteReadingDirection ReadingDirection
        {
            get => GetReadingDirection();
            set => SetReadingDirection(value);
        }

        /// <summary>
        /// Get or set paragraph flow direction.
        /// </summary>
        /// <remarks>
        /// The flow direction must be perpendicular to the reading direction.<br/>
        /// Setting both to a vertical direction or both to horizontal yields
        /// DWRITE_E_FLOWDIRECTIONCONFLICTS when calling GetMetrics or Draw.
        /// </remarks>
        public DWriteFlowDirection FlowDirection
        {
            get => GetFlowDirection();
            set => SetFlowDirection(value);
        }

        /// <summary>
        /// Get or set incremental tab stop position.
        /// </summary>
        public float IncrementalTabStop
        {
            get => GetIncrementalTabStop();
            set => SetIncrementalTabStop(value);
        }

        /// <summary>
        /// Get the font weight.
        /// </summary>
        public DWriteFontWeight FontWeight => GetFontWeight();

        /// <summary>
        /// Get the font style.
        /// </summary>
        public DWriteFontStyle FontStyle => GetFontStyle();

        /// <summary>
        /// Get the font stretch.
        /// </summary>
        public DWriteFontStretch FontStretch => GetFontStretch();

        /// <summary>
        /// Get the font em height.
        /// </summary>
        public float FontSize => GetFontSize();

        [Inline(InlineBehavior.Remove)]
        private void SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetTextAlignment);
            int hr = ((delegate*<void*, DWriteTextAlignment, int>)functionPointer)(nativePointer, textAlignment);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetParagraphAlignment(DWriteParagraphAlignment paragraphAlignment)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetParagraphAlignment);
            int hr = ((delegate*<void*, DWriteParagraphAlignment, int>)functionPointer)(nativePointer, paragraphAlignment);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetWordWrapping(DWriteWordWrapping wordWrapping)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetWordWrapping);
            int hr = ((delegate*<void*, DWriteWordWrapping, int>)functionPointer)(nativePointer, wordWrapping);
            if (hr >= 0)
                return;
            if (wordWrapping < DWriteWordWrapping.EmergencyBreak)
                throw Marshal.GetExceptionForHR(hr);
            WordWrapping = DWriteWordWrapping.Wrap; //For Windows version lower than 8.1
        }

        [Inline(InlineBehavior.Remove)]
        private void SetReadingDirection(DWriteReadingDirection readingDirection)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetReadingDirection);
            int hr = ((delegate*<void*, DWriteReadingDirection, int>)functionPointer)(nativePointer, readingDirection);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetFlowDirection(DWriteFlowDirection flowDirection)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFlowDirection);
            int hr = ((delegate*<void*, DWriteFlowDirection, int>)functionPointer)(nativePointer, flowDirection);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetIncrementalTabStop(float incrementalTabStop)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetIncrementalTabStop);
            int hr = ((delegate*<void*, float, int>)functionPointer)(nativePointer, incrementalTabStop);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Set line spacing.
        /// </summary>
        /// <param name="lineSpacingMethod">How to determine line height.</param>
        /// <param name="lineSpacing">The line height, or rather distance between one baseline to another.</param>
        /// <param name="baseline">Distance from top of line to baseline. A reasonable ratio to lineSpacing is 80%.</param>
        /// <remarks>
        /// For the default method, spacing depends solely on the content.
        /// For uniform spacing, the given line height will override the content.
        /// </remarks>
        public void SetLineSpacing(DWriteLineSpacingMethod lineSpacingMethod, float lineSpacing, float baseline)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetLineSpacing);
            int hr = ((delegate*<void*, DWriteLineSpacingMethod, float, float, int>)functionPointer)(nativePointer, lineSpacingMethod, lineSpacing, baseline);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteTextAlignment GetTextAlignment()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetTextAlignment);
            return ((delegate*<void*, DWriteTextAlignment>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteParagraphAlignment GetParagraphAlignment()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParagraphAlignment);
            return ((delegate*<void*, DWriteParagraphAlignment>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteWordWrapping GetWordWrapping()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetWordWrapping);
            return ((delegate*<void*, DWriteWordWrapping>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteReadingDirection GetReadingDirection()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetReadingDirection);
            return ((delegate*<void*, DWriteReadingDirection>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFlowDirection GetFlowDirection()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFlowDirection);
            return ((delegate*<void*, DWriteFlowDirection>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetIncrementalTabStop()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetIncrementalTabStop);
            return ((delegate*<void*, float>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Get line spacing.
        /// </summary>
        /// <param name="lineSpacingMethod">How line height is determined.</param>
        /// <param name="lineSpacing">The line height, or rather distance between one baseline to another.</param>
        /// <param name="baseline">Distance from top of line to baseline.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetLineSpacing(out DWriteLineSpacingMethod lineSpacingMethod, out float lineSpacing, out float baseline)
            => GetLineSpacing(UnsafeHelper.AsPointerOut(out lineSpacingMethod), UnsafeHelper.AsPointerOut(out lineSpacing), UnsafeHelper.AsPointerOut(out baseline));

        /// <summary>
        /// Get line spacing.
        /// </summary>
        /// <param name="lineSpacingMethod">How line height is determined.</param>
        /// <param name="lineSpacing">The line height, or rather distance between one baseline to another.</param>
        /// <param name="baseline">Distance from top of line to baseline.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        public void GetLineSpacing(DWriteLineSpacingMethod* lineSpacingMethod, float* lineSpacing, float* baseline)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLineSpacing);
            int hr = ((delegate*<void*, DWriteLineSpacingMethod*, float*, float*, int>)functionPointer)(nativePointer, lineSpacingMethod, lineSpacing, baseline);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Get the font collection.
        /// </summary>
        /// <returns>
        /// The current font collection.
        /// </returns>
        public DWriteFontCollection GetFontCollection()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontCollection);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteFontCollection(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Get the length of the font family name, in characters, not including the terminating NULL character.
        /// </summary>
        public uint GetFontFamilyNameLength()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamilyNameLength);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Get the current font family name.
        /// </summary>
        public string GetFontFamilyName()
        {
            uint length = GetFontFamilyNameLength();
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = UnsafeStringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetFontFamilyName(ptr, length + 1);
            return result;
        }

        /// <summary>
        /// Get a copy of the font family name.
        /// </summary>
        /// <param name="fontFamilyName">Character array that receives the current font family name</param>
        /// <param name="nameSize">Size of the character array in character count including the terminated NULL character.</param>
        public void GetFontFamilyName(char* fontFamilyName, uint nameSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamilyName);
            int hr = ((delegate*<void*, char*, uint, int>)functionPointer)(nativePointer, fontFamilyName, nameSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontWeight GetFontWeight()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontWeight);
            return ((delegate*<void*, DWriteFontWeight>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontStyle GetFontStyle()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontStyle);
            return ((delegate*<void*, DWriteFontStyle>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontStretch GetFontStretch()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontStretch);
            return ((delegate*<void*, DWriteFontStretch>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetFontSize()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontSize);
            return ((delegate*<void*, float>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Get the length of the locale name, in characters, not including the terminating NULL character.
        /// </summary>
        public uint GetLocaleNameLength()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleNameLength);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Get the current locale name.
        /// </summary>
        public string GetLocaleName()
        {
            uint length = GetLocaleNameLength();
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = UnsafeStringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetLocaleName(ptr, length + 1);
            return result;
        }

        /// <summary>
        /// Get a copy of the locale name.
        /// </summary>
        /// <param name="localeName">Character array that receives the current locale name</param>
        /// <param name="nameSize">Size of the character array in character count including the terminated NULL character.</param>
        public void GetLocaleName(char* localeName, uint nameSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleName);
            int hr = ((delegate*<void*, char*, uint, int>)functionPointer)(nativePointer, localeName, nameSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}