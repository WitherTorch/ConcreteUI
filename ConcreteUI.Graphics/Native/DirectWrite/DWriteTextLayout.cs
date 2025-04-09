using System;
using System.Drawing;
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
    /// The <see cref="DWriteTextLayout"/> class represents a block of text after it has
    /// been fully analyzed and formatted.<br/>
    ///<br/>
    /// All coordinates are in device independent pixels (DIPs).
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteTextLayout : DWriteTextFormat
    {
        private new enum MethodTable
        {
            _Start = DWriteTextFormat.MethodTable._End,
            SetMaxWidth = _Start,
            SetMaxHeight,
            SetFontCollection,
            SetFontFamilyName,
            SetFontWeight,
            SetFontStyle,
            SetFontStretch,
            SetFontSize,
            SetUnderline,
            SetStrikethrough,
            SetDrawingEffect,
            SetInlineObject,
            SetTypography,
            SetLocaleName,
            GetMaxWidth,
            GetMaxHeight,
            GetFontCollection,
            GetFontFamilyNameLength,
            GetFontFamilyName,
            GetFontWeight,
            GetFontStyle,
            GetFontStretch,
            GetFontSize,
            GetUnderline,
            GetStrikethrough,
            GetDrawingEffect,
            GetInlineObject,
            GetTypography,
            GetLocaleNameLength,
            GetLocaleName,
            Draw,
            GetLineMetrics,
            GetMetrics,
            GetOverhangMetrics,
            GetClusterMetrics,
            DetermineMinWidth,
            HitTestPoint,
            HitTestTextPosition,
            HitTestTextRange,
            _End
        }

        public DWriteTextLayout() : base() { }

        public DWriteTextLayout(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Get or set layout maximum width
        /// </summary>
        public float MaxWidth
        {
            get => GetMaxWidth();
            set => SetMaxWidth(value);
        }

        /// <summary>
        /// Get or set layout maximum height
        /// </summary>
        public float MaxHeight
        {
            get => GetMaxHeight();
            set => SetMaxHeight(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMaxWidth(float maxWidth)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMaxWidth);
            int hr = ((delegate* unmanaged[Stdcall]<void*, float, int>)functionPointer)(nativePointer, maxWidth);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetMaxHeight(float maxHeight)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetMaxHeight);
            int hr = ((delegate* unmanaged[Stdcall]<void*, float, int>)functionPointer)(nativePointer, maxHeight);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set the font collection.
        /// </summary>
        /// <param name="fontCollection">The font collection to set</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontCollection(DWriteFontCollection fontCollection, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontCollection);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, DWriteTextRange, int>)functionPointer)(nativePointer, fontCollection.NativePointer, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set font family name.
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFontFamilyName(string fontFamilyName, DWriteTextRange textRange)
        {
            fixed (char* ptr = fontFamilyName)
                SetFontFamilyName(ptr, textRange);
        }

        /// <summary>
        /// Set null-terminated font family name.
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontFamilyName(char* fontFamilyName, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontFamilyName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, DWriteTextRange, int>)functionPointer)(nativePointer, fontFamilyName, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set font weight.
        /// </summary>
        /// <param name="fontWeight">Font weight</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontWeight(DWriteFontWeight fontWeight, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontWeight);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteFontWeight, DWriteTextRange, int>)functionPointer)(nativePointer, fontWeight, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set font style.
        /// </summary>
        /// <param name="fontStyle">Font style</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontStyle(DWriteFontStyle fontStyle, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontStyle);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteFontStyle, DWriteTextRange, int>)functionPointer)(nativePointer, fontStyle, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set font stretch.
        /// </summary>
        /// <param name="fontStretch">font stretch</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontStretch(DWriteFontStretch fontStretch, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontStretch);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteFontStretch, DWriteTextRange, int>)functionPointer)(nativePointer, fontStretch, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set font em height.
        /// </summary>
        /// <param name="fontSize">Font em height</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetFontSize(float fontSize, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetFontSize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, float, DWriteTextRange, int>)functionPointer)(nativePointer, fontSize, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set underline.
        /// </summary>
        /// <param name="hasUnderline">The Boolean flag indicates whether underline takes place</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetUnderline(bool hasUnderline, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetUnderline);
            int hr = ((delegate* unmanaged[Stdcall]<void*, bool, DWriteTextRange, int>)functionPointer)(nativePointer, hasUnderline, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set strikethrough.
        /// </summary>
        /// <param name="hasStrikethrough">The Boolean flag indicates whether strikethrough takes place</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetStrikethrough(bool hasStrikethrough, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetStrikethrough);
            int hr = ((delegate* unmanaged[Stdcall]<void*, bool, DWriteTextRange, int>)functionPointer)(nativePointer, hasStrikethrough, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set application-defined drawing effect.
        /// </summary>
        /// <param name="drawingEffect">Pointer to an application-defined drawing effect.</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        /// <remarks>
        /// This drawing effect is associated with the specified range and will be passed back
        /// to the application via the callback when the range is drawn at drawing time.
        /// </remarks>
        public void SetDrawingEffect(ComObject drawingEffect, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetDrawingEffect);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, DWriteTextRange, int>)functionPointer)(nativePointer,
                drawingEffect == null ? null : drawingEffect.NativePointer, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set locale name.
        /// </summary>
        /// <param name="localeName">Locale name</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLocaleName(string localeName, DWriteTextRange textRange)
        {
            fixed (char* ptr = localeName)
                SetLocaleName(ptr, textRange);
        }

        /// <summary>
        /// Set locale name.
        /// </summary>
        /// <param name="localeName">Locale name</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        public void SetLocaleName(char* localeName, DWriteTextRange textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetLocaleName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, DWriteTextRange, int>)functionPointer)(nativePointer, localeName, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetMaxWidth()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaxWidth);
            return ((delegate* unmanaged[Stdcall]<void*, float>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetMaxHeight()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaxHeight);
            return ((delegate* unmanaged[Stdcall]<void*, float>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="GetFontCollection(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteFontCollection? GetFontCollection(uint currentPosition)
            => GetFontCollection(currentPosition, null);

        /// <inheritdoc cref="GetFontCollection(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteFontCollection? GetFontCollection(uint currentPosition, out DWriteTextRange textRange)
            => GetFontCollection(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the font collection where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">Text range to which this change applies.</param>
        /// <returns>
        /// The current font collection.
        /// </returns>
        public DWriteFontCollection? GetFontCollection(uint currentPosition, DWriteTextRange* textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontCollection);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &nativePointer, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new DWriteFontCollection(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="GetFontFamilyNameLength(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public uint GetFontFamilyNameLength(uint currentPosition)
            => GetFontFamilyNameLength(currentPosition, null);

        /// <inheritdoc cref="GetFontFamilyNameLength(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetFontFamilyNameLength(uint currentPosition, out DWriteTextRange textRange)
            => GetFontFamilyNameLength(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the length of the font family name where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// Size of the character array in character count not including the terminated NULL character.
        /// </returns>
        [LocalsInit(false)]
        public uint GetFontFamilyNameLength(uint currentPosition, DWriteTextRange* textRange)
        {
            uint nameLength;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamilyNameLength);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &nameLength, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nameLength;
        }

        /// <inheritdoc cref="GetFontFamilyName(uint, char*, uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public string GetFontFamilyName(uint currentPosition)
            => GetFontFamilyName(currentPosition, null);

        /// <inheritdoc cref="GetFontFamilyName(uint, char*, uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetFontFamilyName(uint currentPosition, out DWriteTextRange textRange)
            => GetFontFamilyName(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <inheritdoc cref="GetFontFamilyName(uint, char*, uint, DWriteTextRange*)"/>
        public string GetFontFamilyName(uint currentPosition, DWriteTextRange* textRange)
        {
            uint length = GetFontFamilyNameLength(currentPosition, textRange);
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = StringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetFontFamilyName(currentPosition, ptr, length + 1);
            return result;
        }

        /// <inheritdoc cref="GetFontFamilyName(uint, char*, uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void GetFontFamilyName(uint currentPosition, char* fontFamilyName, uint nameSize)
            => GetFontFamilyName(currentPosition, fontFamilyName, nameSize, null);

        /// <inheritdoc cref="GetFontFamilyName(uint, char*, uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetFontFamilyName(uint currentPosition, char* fontFamilyName, uint nameSize, out DWriteTextRange textRange)
            => GetFontFamilyName(currentPosition, fontFamilyName, nameSize, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Copy the font family name where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="fontFamilyName">Character array that receives the current font family name</param>
        /// <param name="nameSize">Size of the character array in character count including the terminated NULL character.</param>
        /// <param name="textRange">The position range of the current format.</param>
        public void GetFontFamilyName(uint currentPosition, char* fontFamilyName, uint nameSize, DWriteTextRange* textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamilyName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, char*, uint, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, fontFamilyName, nameSize, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <inheritdoc cref="GetFontWeight(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteFontWeight GetFontWeight(uint currentPosition)
            => GetFontWeight(currentPosition, null);

        /// <inheritdoc cref="GetFontWeight(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteFontWeight GetFontWeight(uint currentPosition, out DWriteTextRange textRange)
            => GetFontWeight(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the font weight where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The current font weight.
        /// </returns>
        [LocalsInit(false)]
        public DWriteFontWeight GetFontWeight(uint currentPosition, DWriteTextRange* textRange)
        {
            DWriteFontWeight fontWeight;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontWeight);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, DWriteFontWeight*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &fontWeight, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return fontWeight;
        }

        /// <inheritdoc cref="GetFontStyle(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteFontStyle GetFontStyle(uint currentPosition)
            => GetFontStyle(currentPosition, null);

        /// <inheritdoc cref="GetFontStyle(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteFontStyle GetFontStyle(uint currentPosition, out DWriteTextRange textRange)
            => GetFontStyle(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the font style where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The current font style.
        /// </returns>
        [LocalsInit(false)]
        public DWriteFontStyle GetFontStyle(uint currentPosition, DWriteTextRange* textRange)
        {
            DWriteFontStyle fontStyle;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontStyle);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, DWriteFontStyle*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &fontStyle, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return fontStyle;
        }

        /// <inheritdoc cref="GetFontStretch(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteFontStretch GetFontStretch(uint currentPosition)
            => GetFontStretch(currentPosition, null);

        /// <inheritdoc cref="GetFontStretch(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteFontStretch GetFontStretch(uint currentPosition, out DWriteTextRange textRange)
            => GetFontStretch(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the font stretch where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The current font stretch.
        /// </returns>
        [LocalsInit(false)]
        public DWriteFontStretch GetFontStretch(uint currentPosition, DWriteTextRange* textRange)
        {
            DWriteFontStretch fontStretch;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontStretch);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, DWriteFontStretch*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &fontStretch, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return fontStretch;
        }

        /// <inheritdoc cref="GetFontSize(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public float GetFontSize(uint currentPosition)
            => GetFontSize(currentPosition, null);

        /// <inheritdoc cref="GetFontSize(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFontSize(uint currentPosition, out DWriteTextRange textRange)
            => GetFontSize(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the font em height where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The current font em height.
        /// </returns>
        [LocalsInit(false)]
        public float GetFontSize(uint currentPosition, DWriteTextRange* textRange)
        {
            float fontSize;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontSize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, float*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &fontSize, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return fontSize;
        }

        /// <inheritdoc cref="GetUnderline(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public bool GetUnderline(uint currentPosition)
            => GetUnderline(currentPosition, null);

        /// <inheritdoc cref="GetUnderline(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetUnderline(uint currentPosition, out DWriteTextRange textRange)
            => GetUnderline(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the underline presence where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The <see cref="bool"/> flag indicates whether text is underlined.
        /// </returns>
        [LocalsInit(false)]
        public bool GetUnderline(uint currentPosition, DWriteTextRange* textRange)
        {
            bool hasUnderline;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetUnderline);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, bool*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &hasUnderline, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return hasUnderline;
        }

        /// <inheritdoc cref="GetStrikethrough(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public bool GetStrikethrough(uint currentPosition)
            => GetStrikethrough(currentPosition, null);

        /// <inheritdoc cref="GetStrikethrough(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetStrikethrough(uint currentPosition, out DWriteTextRange textRange)
            => GetStrikethrough(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the strikethrough presence where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The <see cref="bool"/> flag indicates whether text has strikethrough.
        /// </returns>
        [LocalsInit(false)]
        public bool GetStrikethrough(uint currentPosition, DWriteTextRange* textRange)
        {
            bool hasStrikethrough;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStrikethrough);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, bool*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &hasStrikethrough, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return hasStrikethrough;
        }

        /// <inheritdoc cref="GetDrawingEffect(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public ComObject? GetDrawingEffect(uint currentPosition)
            => GetDrawingEffect(currentPosition, null);

        /// <inheritdoc cref="GetDrawingEffect(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComObject? GetDrawingEffect(uint currentPosition, out DWriteTextRange textRange)
            => GetDrawingEffect(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the application-defined drawing effect where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// The current application-defined drawing effect.
        /// </returns>
        public ComObject? GetDrawingEffect(uint currentPosition, DWriteTextRange* textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDrawingEffect);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &nativePointer, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="GetLocaleNameLength(uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public uint GetLocaleNameLength(uint currentPosition)
            => GetLocaleNameLength(currentPosition, null);

        /// <inheritdoc cref="GetLocaleNameLength(uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetLocaleNameLength(uint currentPosition, out DWriteTextRange textRange)
            => GetLocaleNameLength(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the length of the locale name where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="textRange">The position range of the current format.</param>
        /// <returns>
        /// Size of the character array in character count not including the terminated NULL character.
        /// </returns>
        [LocalsInit(false)]
        public uint GetLocaleNameLength(uint currentPosition, DWriteTextRange* textRange)
        {
            uint nameLength;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleNameLength);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint*, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, &nameLength, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nameLength;
        }

        /// <inheritdoc cref="GetLocaleName(uint, char*, uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public string GetLocaleName(uint currentPosition)
            => GetLocaleName(currentPosition, null);

        /// <inheritdoc cref="GetLocaleName(uint, char*, uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetLocaleName(uint currentPosition, out DWriteTextRange textRange)
            => GetLocaleName(currentPosition, UnsafeHelper.AsPointerOut(out textRange));

        /// <inheritdoc cref="GetLocaleName(uint, char*, uint, DWriteTextRange*)"/>
        public string GetLocaleName(uint currentPosition, DWriteTextRange* textRange)
        {
            uint length = GetLocaleNameLength(currentPosition, textRange);
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = StringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetLocaleName(currentPosition, ptr, length + 1);
            return result;
        }

        /// <inheritdoc cref="GetLocaleName(uint, char*, uint, DWriteTextRange*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void GetLocaleName(uint currentPosition, char* localeName, uint nameSize)
            => GetLocaleName(currentPosition, localeName, nameSize, null);

        /// <inheritdoc cref="GetLocaleName(uint, char*, uint, DWriteTextRange*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetLocaleName(uint currentPosition, char* localeName, uint nameSize, out DWriteTextRange textRange)
            => GetLocaleName(currentPosition, localeName, nameSize, UnsafeHelper.AsPointerOut(out textRange));

        /// <summary>
        /// Get the locale name where the current position is at.
        /// </summary>
        /// <param name="currentPosition">The current text position.</param>
        /// <param name="localeName">Character array that receives the current locale name</param>
        /// <param name="nameSize">Size of the character array in character count including the terminated NULL character.</param>
        /// <param name="textRange">The position range of the current format.</param>
        public void GetLocaleName(uint currentPosition, char* localeName, uint nameSize, DWriteTextRange* textRange)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLocaleName);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, char*, uint, DWriteTextRange*, int>)functionPointer)(nativePointer,
                currentPosition, localeName, nameSize, textRange);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Returns properties of each line.
        /// </summary>
        /// <returns>
        /// The array to fill with line information.
        /// </returns>
        public DWriteLineMetrics[] GetLineMetrics()
        {
            int hr = TryGetLineMetrics(null, 0u, out uint lineCount);
            if (hr >= 0)
                return Array.Empty<DWriteLineMetrics>();
            DWriteLineMetrics[]? result = null;
            while (hr == Constants.E_INSUFFICIENT_BUFFER)
            {
                result = new DWriteLineMetrics[lineCount];
                fixed (DWriteLineMetrics* ptr = result)
                    hr = TryGetLineMetrics(ptr, lineCount, &lineCount);
            }
            ;
            ThrowHelper.ThrowExceptionForHR(hr);
            return result ?? Array.Empty<DWriteLineMetrics>();
        }

        /// <summary>
        /// Try returns properties of each line.
        /// </summary>
        /// <param name="lineMetrics">The array to fill with line information.</param>
        /// <param name="maxLineCount">The maximum size of the lineMetrics array.</param>
        /// <param name="actualLineCount">The actual size of the lineMetrics
        /// array that is needed.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        /// <remarks>
        /// If <paramref name="maxLineCount"/> is not large enough <see cref="Constants.E_INSUFFICIENT_BUFFER"/>
        /// is returned and <paramref name="actualLineCount"/> is set to the number of lines
        /// needed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TryGetLineMetrics(DWriteLineMetrics* lineMetrics, uint maxLineCount, out uint actualLineCount)
            => TryGetLineMetrics(lineMetrics, maxLineCount, UnsafeHelper.AsPointerOut(out actualLineCount));

        /// <summary>
        /// Try returns properties of each line.
        /// </summary>
        /// <param name="lineMetrics">The array to fill with line information.</param>
        /// <param name="maxLineCount">The maximum size of the lineMetrics array.</param>
        /// <param name="actualLineCount">The actual size of the lineMetrics
        /// array that is needed.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        /// <remarks>
        /// If <paramref name="maxLineCount"/> is not large enough <see cref="Constants.E_INSUFFICIENT_BUFFER"/>
        /// is returned and *<paramref name="actualLineCount"/> is set to the number of lines
        /// needed.
        /// </remarks>
        public int TryGetLineMetrics(DWriteLineMetrics* lineMetrics, uint maxLineCount, uint* actualLineCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLineMetrics);
            return ((delegate* unmanaged[Stdcall]<void*, DWriteLineMetrics*, uint, uint*, int>)functionPointer)(nativePointer, lineMetrics, maxLineCount, actualLineCount);
        }

        /// <summary>
        /// Retrieves overall metrics for the formatted string.
        /// </summary>
        /// <returns>
        /// The returned metrics.
        /// </returns>
        /// <remarks>
        /// Drawing effects like underline and strikethrough do not contribute
        /// to the text size, which is essentially the sum of advance widths and
        /// line heights. <br/>
        /// Additionally, visible swashes and other graphic
        /// adornments may extend outside the returned width and height.
        /// </remarks>
        [LocalsInit(false)]
        public DWriteTextMetrics GetMetrics()
        {
            DWriteTextMetrics textMetrics;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMetrics);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteTextMetrics*, int>)functionPointer)(nativePointer, &textMetrics);
            ThrowHelper.ThrowExceptionForHR(hr);
            return textMetrics;
        }

        /// <summary>
        /// Determines the minimum possible width the layout can be set to without
        /// emergency breaking between the characters of whole words.
        /// </summary>
        /// <returns>
        /// The minimum width.
        /// </returns>
        public float DetermineMinWidth()
        {
            float minWidth;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DetermineMinWidth);
            int hr = ((delegate* unmanaged[Stdcall]<void*, float*, int>)functionPointer)(nativePointer, &minWidth);
            ThrowHelper.ThrowExceptionForHR(hr);
            return minWidth;
        }

        /// <inheritdoc cref="HitTestPoint(float, float, out bool, out bool)"/>
        /// <param name="point">The point to hit-test, relative to the top-left location of the layout box.</param>

        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteHitTestMetrics HitTestPoint(PointF point, out bool isTrailingHit, out bool isInside)
            => HitTestPoint(point.X, point.Y, out isTrailingHit, out isInside);

        /// <inheritdoc cref="HitTestPoint(float, float, bool*, bool*)"/>
        /// <param name="point">The point to hit-test, relative to the top-left location of the layout box.</param>

        [Inline(InlineBehavior.Keep, export: true)]
        public DWriteHitTestMetrics HitTestPoint(PointF point, bool* isTrailingHit, bool* isInside)
            => HitTestPoint(point.X, point.Y, isTrailingHit, isInside);

        /// <summary>
        /// Given a coordinate (in DIPs) relative to the top-left of the layout box,
        /// this returns the corresponding hit-test metrics of the text string where the hit-test has occurred.<br/>
        /// This is useful for mapping mouse clicks to caret positions. <br/>
        /// When the given coordinate is outside the text string, the function sets the output value <paramref name="isInside"/> to <see langword="false"/> but returns the nearest character
        /// position.
        /// </summary>
        /// <param name="pointX">X coordinate to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="pointY">Y coordinate to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="isTrailingHit">Output flag indicating whether the hit-test location is at the leading or the trailing side of the character. <br/> 
        ///     When the output <paramref name="isInside"/> value is set to <see langword="false"/>, this value is set according to the output
        ///     *position value to represent the edge closest to the hit-test location. </param>
        /// <param name="isInside">Output flag indicating whether the hit-test location is inside the text string.
        ///     When false, the position nearest the text's edge is returned.</param>
        /// <returns>Output geometry fully enclosing the hit-test location. When the output <paramref name="isInside"/> value
        ///  is set to false, this structure represents the geometry enclosing the edge closest to the hit-test location.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteHitTestMetrics HitTestPoint(float pointX, float pointY, out bool isTrailingHit, out bool isInside)
            => HitTestPoint(pointX, pointY, UnsafeHelper.AsPointerOut(out isTrailingHit), UnsafeHelper.AsPointerOut(out isInside));

        /// <summary>
        /// Given a coordinate (in DIPs) relative to the top-left of the layout box,
        /// this returns the corresponding hit-test metrics of the text string where the hit-test has occurred.<br/>
        /// This is useful for mapping mouse clicks to caret positions. <br/>
        /// When the given coordinate is outside the text string, the function sets the output value *<paramref name="isInside"/> to <see langword="false"/> but returns the nearest character
        /// position.
        /// </summary>
        /// <param name="pointX">X coordinate to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="pointY">Y coordinate to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="isTrailingHit">Output flag indicating whether the hit-test location is at the leading or the trailing side of the character. <br/> 
        ///     When the output *<paramref name="isInside"/> value is set to <see langword="false"/>, this value is set according to the output
        ///     *position value to represent the edge closest to the hit-test location. </param>
        /// <param name="isInside">Output flag indicating whether the hit-test location is inside the text string.
        ///     When false, the position nearest the text's edge is returned.</param>
        /// <returns>
        /// Geometry fully enclosing the hit-test location. When the output *<paramref name="isInside"/> value
        ///  is set to false, this structure represents the geometry enclosing the edge closest to the hit-test location.
        /// </returns>
        [LocalsInit(false)]
        public DWriteHitTestMetrics HitTestPoint(float pointX, float pointY, bool* isTrailingHit, bool* isInside)
        {
            DWriteHitTestMetrics hitTestMetrics;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.HitTestPoint);
            int hr = ((delegate* unmanaged[Stdcall]<void*, float, float, bool*, bool*, DWriteHitTestMetrics*, int>)functionPointer)(nativePointer,
                pointX, pointY, isTrailingHit, isInside, &hitTestMetrics);
            ThrowHelper.ThrowExceptionForHR(hr);
            return hitTestMetrics;
        }

        /// <inheritdoc cref="HitTestTextPosition(uint, bool, float*, float*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DWriteHitTestMetrics HitTestTextPosition(uint textPosition, bool isTrailingHit, out float pointX, out float pointY)
            => HitTestTextPosition(textPosition, isTrailingHit, UnsafeHelper.AsPointerOut(out pointX), UnsafeHelper.AsPointerOut(out pointY));

        /// <summary>
        /// Given a text position and whether the caret is on the leading or trailing
        /// edge of that position, this returns the corresponding coordinate (in DIPs)
        /// relative to the top-left of the layout box. <br/>
        /// This is most useful for drawing the caret's current position, but it could also be used to anchor an IME 
        /// to the typed text or attach a floating menu near the point of interest. <br/>
        /// It may also be used to programmatically obtain the geometry of a particular text position for UI automation.
        /// </summary>
        /// <param name="textPosition">Text position to get the coordinate of.</param>
        /// <param name="isTrailingHit">Flag indicating whether the location is of the leading or the trailing side of the specified text position. </param>
        /// <param name="pointX">Output caret X, relative to the top-left of the layout box.</param>
        /// <param name="pointY">Output caret Y, relative to the top-left of the layout box.</param>
        /// <returns>
        /// Geometry fully enclosing the specified text position.
        /// </returns>
        /// <remarks>
        /// When drawing a caret at the returned X,Y, it should be centered on X and drawn from the Y coordinate down.<br/>
        /// The height will be the size of the hit-tested text (which can vary in size within a line).<br/>
        /// Reading direction also affects which side of the character the caret is drawn.<br/>
        /// However, the returned X coordinate will be correct for either case.<br/>
        /// You can get a text length back that is larger than a single character.<br/>
        /// This happens for complex scripts when multiple characters form a single cluster,
        /// when diacritics join their base character, or when you test a surrogate pair.
        /// </remarks>
        [LocalsInit(false)]
        public DWriteHitTestMetrics HitTestTextPosition(uint textPosition, bool isTrailingHit, float* pointX, float* pointY)
        {
            DWriteHitTestMetrics hitTestMetrics;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.HitTestTextPosition);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, bool, float*, float*, DWriteHitTestMetrics*, int>)functionPointer)(nativePointer,
                textPosition, isTrailingHit, pointX, pointY, &hitTestMetrics);
            ThrowHelper.ThrowExceptionForHR(hr);
            return hitTestMetrics;
        }

        /// <summary>
        /// The application calls this function to get a set of hit-test metrics corresponding to a range of text positions. <br/>
        /// The main usage for this is to draw highlighted selection of the text string.
        /// </summary>
        /// <param name="textPosition">First text position of the specified range.</param>
        /// <param name="textLength">Number of positions of the specified range.</param>
        /// <param name="originX">Offset of the X origin (left of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <param name="originY">Offset of the Y origin (top of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <returns>
        /// Geometry fully enclosing the specified position range.
        /// </returns>
        /// <remarks>
        /// There are no gaps in the returned metrics. While there could be visual gaps,
        /// depending on bidi ordering, each range is contiguous and reports all the text,
        /// including any hidden characters and trimmed text.<br/>
        /// The height of each returned range will be the same within each line, regardless of how the font sizes vary.
        /// </remarks>
        public DWriteHitTestMetrics[] HitTestTextRange(uint textPosition, uint textLength, float originX, float originY)
        {
            int hr = TryHitTestTextRange(textPosition, textLength, originX, originY, null, 0u, out uint metricsCount);
            if (hr >= 0)
                return Array.Empty<DWriteHitTestMetrics>();
            DWriteHitTestMetrics[]? result = null;
            while (hr == Constants.E_INSUFFICIENT_BUFFER)
            {
                result = new DWriteHitTestMetrics[metricsCount];
                fixed (DWriteHitTestMetrics* ptr = result)
                    hr = TryHitTestTextRange(textPosition, textLength, originX, originY, ptr, metricsCount, &metricsCount);
            }
            ;
            ThrowHelper.ThrowExceptionForHR(hr);
            return result ?? Array.Empty<DWriteHitTestMetrics>();
        }

        /// <summary>
        /// The application calls this function to get a set of hit-test metrics corresponding to a range of text positions. <br/>
        /// The main usage for this is to draw highlighted selection of the text string.<br/>
        /// <br/>
        /// The function returns <see cref="Constants.E_INSUFFICIENT_BUFFER"/>, 
        /// when the buffer size of <paramref name="hitTestMetrics"/> is too small to hold all the regions calculated by the
        /// function.<br/>
        /// In such situation, the function sets the output value <paramref name="actualHitTestMetricsCount"/> to the number of geometries calculated.<br/>
        /// The application is responsible to allocate a new buffer of greater
        /// size and call the function again.<br/>
        ///<br/>
        /// A good value to use as an initial value for maxHitTestMetricsCount may
        /// be calculated from the following equation:<br/>
        /// maxHitTestMetricsCount = lineCount * maxBidiReorderingDepth<br/>
        /// where lineCount is obtained from the value of the output argument actualLineCount from the function <see cref="TryGetLineMetrics(DWriteLineMetrics*, uint, out uint)"/>,<br/>
        /// and the maxBidiReorderingDepth value from the <see cref="DWriteTextMetrics"/>
        /// structure from the function <see cref="GetMetrics()"/>
        /// </summary>
        /// <param name="textPosition">First text position of the specified range.</param>
        /// <param name="textLength">Number of positions of the specified range.</param>
        /// <param name="originX">Offset of the X origin (left of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <param name="originY">Offset of the Y origin (top of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <param name="hitTestMetrics">Pointer to a buffer of the output geometry fully enclosing the specified position range.</param>
        /// <param name="maxHitTestMetricsCount">Maximum number of distinct metrics it could hold in its buffer memory.</param>
        /// <param name="actualHitTestMetricsCount">Actual number of metrics returned or needed.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        /// <remarks>
        /// There are no gaps in the returned metrics. While there could be visual gaps,
        /// depending on bidi ordering, each range is contiguous and reports all the text,
        /// including any hidden characters and trimmed text.<br/>
        /// The height of each returned range will be the same within each line, regardless of how the font sizes vary.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TryHitTestTextRange(uint textPosition, uint textLength, float originX, float originY, DWriteHitTestMetrics* hitTestMetrics,
            uint maxHitTestMetricsCount, out uint actualHitTestMetricsCount)
            => TryHitTestTextRange(textPosition, textLength, originX, originY, hitTestMetrics, maxHitTestMetricsCount, UnsafeHelper.AsPointerOut(out actualHitTestMetricsCount));

        /// <summary>
        /// The application calls this function to get a set of hit-test metrics corresponding to a range of text positions. <br/>
        /// The main usage for this is to draw highlighted selection of the text string.<br/>
        /// <br/>
        /// The function returns <see cref="Constants.E_INSUFFICIENT_BUFFER"/>, 
        /// when the buffer size of <paramref name="hitTestMetrics"/> is too small to hold all the regions calculated by the
        /// function.<br/>
        /// In such situation, the function sets the output value *<paramref name="actualHitTestMetricsCount"/> to the number of geometries calculated.<br/>
        /// The application is responsible to allocate a new buffer of greater
        /// size and call the function again.<br/>
        ///<br/>
        /// A good value to use as an initial value for maxHitTestMetricsCount may
        /// be calculated from the following equation:<br/>
        /// maxHitTestMetricsCount = lineCount * maxBidiReorderingDepth<br/>
        /// where lineCount is obtained from the value of the output argument *actualLineCount from the function <see cref="TryGetLineMetrics(DWriteLineMetrics*, uint, uint*)"/>,<br/>
        /// and the maxBidiReorderingDepth value from the <see cref="DWriteTextMetrics"/>
        /// structure from the function <see cref="GetMetrics()"/>
        /// </summary>
        /// <param name="textPosition">First text position of the specified range.</param>
        /// <param name="textLength">Number of positions of the specified range.</param>
        /// <param name="originX">Offset of the X origin (left of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <param name="originY">Offset of the Y origin (top of the layout box) which is added to each of the hit-test metrics returned.</param>
        /// <param name="hitTestMetrics">Pointer to a buffer of the output geometry fully enclosing the specified position range.</param>
        /// <param name="maxHitTestMetricsCount">Maximum number of distinct metrics it could hold in its buffer memory.</param>
        /// <param name="actualHitTestMetricsCount">Actual number of metrics returned or needed.</param>
        /// <returns>
        /// Standard HRESULT error code.
        /// </returns>
        /// <remarks>
        /// There are no gaps in the returned metrics. While there could be visual gaps,
        /// depending on bidi ordering, each range is contiguous and reports all the text,
        /// including any hidden characters and trimmed text.<br/>
        /// The height of each returned range will be the same within each line, regardless of how the font sizes vary.
        /// </remarks>
        public int TryHitTestTextRange(uint textPosition, uint textLength, float originX, float originY, DWriteHitTestMetrics* hitTestMetrics,
            uint maxHitTestMetricsCount, uint* actualHitTestMetricsCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.HitTestTextRange);
            return ((delegate* unmanaged[Stdcall]<void*, uint, uint, float, float, DWriteHitTestMetrics*, uint, uint*, int>)functionPointer)(nativePointer,
                textPosition, textLength, originX, originY, hitTestMetrics, maxHitTestMetricsCount, actualHitTestMetricsCount);
        }
    }
}