using System;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    public enum DWriteMeasuringMode : int
    {
        Natural,
        GdiClassic,
        GdiNatural
    }

    public enum DWriteFactoryType : int
    {
        Shared,
        Isolated
    }

    public enum DWriteFontFamilyModel : uint
    {
        Typographic,
        WeightStretchStyle
    }

    /// <summary>
    /// The font weight enumeration describes common values for degree of blackness or thickness of strokes of characters in a font.<br/>
    /// Font weight values less than 1 or greater than 999 are considered to be invalid, and they are rejected by font API functions.
    /// </summary>
    public enum DWriteFontWeight : int
    {
        /// <summary>
        /// Predefined font weight : Thin (100).
        /// </summary>
        Thin = 100,

        /// <summary>
        /// Predefined font weight : Extra-light (200).
        /// </summary>
        Extra_Light = 200,

        /// <summary>
        /// Predefined font weight : Ultra-light (200).
        /// </summary>
        Ultra_Light = 200,

        /// <summary>
        /// Predefined font weight : Light (300).
        /// </summary>
        Light = 300,

        /// <summary>
        /// Predefined font weight : Semi-light (350).
        /// </summary>
        SemiLight = 350,

        /// <summary>
        /// Predefined font weight : Utf16 (400).
        /// </summary>
        Normal = 400,

        /// <summary>
        /// Predefined font weight : Regular (400).
        /// </summary>
        Regular = 400,

        /// <summary>
        /// Predefined font weight : Medium (500).
        /// </summary>
        Medium = 500,

        /// <summary>
        /// Predefined font weight : Demi-bold (600).
        /// </summary>
        DemiBold = 600,

        /// <summary>
        /// Predefined font weight : Semi-bold (600).
        /// </summary>
        SemiBold = 600,

        /// <summary>
        /// Predefined font weight : Bold (700).
        /// </summary>
        Bold = 700,

        /// <summary>
        /// Predefined font weight : Extra-bold (800).
        /// </summary>
        ExtraBold = 800,

        /// <summary>
        /// Predefined font weight : Ultra-bold (800).
        /// </summary>
        UltraBold = 800,

        /// <summary>
        /// Predefined font weight : Black (900).
        /// </summary>
        Black = 900,

        /// <summary>
        /// Predefined font weight : Heavy (900).
        /// </summary>
        Heavy = 900,

        /// <summary>
        /// Predefined font weight : Extra-black (950).
        /// </summary>
        ExtraBlack = 950,

        /// <summary>
        /// Predefined font weight : Ultra-black (950).
        /// </summary>
        UltraBlack = 950
    }

    /// <summary>
    /// The font style enumeration describes the slope style of a font face, such as Utf16, Italic or Oblique.<br/>
    /// Values other than the ones defined in the enumeration are considered to be invalid, and they are rejected by font API functions.
    /// </summary>
    public enum DWriteFontStyle : int
    {
        /// <summary>
        /// Font slope style : Utf16.
        /// </summary>
        Normal,

        /// <summary>
        /// Font slope style : Oblique.
        /// </summary>
        Oblique,

        /// <summary>
        /// Font slope style : Italic.
        /// </summary>
        Italic
    }

    /// <summary>
    /// The font stretch enumeration describes relative change from the normal aspect ratio
    /// as specified by a font designer for the glyphs in a font.<br/>
    /// Values less than 1 or greater than 9 are considered to be invalid, and they are rejected by font API functions.
    /// </summary>
    public enum DWriteFontStretch : int
    {
        /// <summary>
        /// Predefined font stretch : Not known (0).
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Predefined font stretch : Ultra-condensed (1).
        /// </summary>
        UltraCondensed = 1,

        /// <summary>
        /// Predefined font stretch : Extra-condensed (2).
        /// </summary>
        ExtraCondensed = 2,

        /// <summary>
        /// Predefined font stretch : Condensed (3).
        /// </summary>
        Condensed = 3,

        /// <summary>
        /// Predefined font stretch : Semi-condensed (4).
        /// </summary>
        SemiCondensed = 4,

        /// <summary>
        /// Predefined font stretch : Utf16 (5).
        /// </summary>
        Normal = 5,

        /// <summary>
        /// Predefined font stretch : Medium (5).
        /// </summary>
        Medium = 5,

        /// <summary>
        /// Predefined font stretch : Semi-expanded (6).
        /// </summary>
        SemiExpanded = 6,

        /// <summary>
        /// Predefined font stretch : Expanded (7).
        /// </summary>
        Expanded = 7,

        /// <summary>
        /// Predefined font stretch : Extra-expanded (8).
        /// </summary>
        ExtraExpanded = 8,

        /// <summary>
        /// Predefined font stretch : Ultra-expanded (9).
        /// </summary>
        UltraExpanded = 9
    }

    /// <summary>
    /// Alignment of paragraph text along the reading direction axis relative to 
    /// the leading and trailing edge of the layout box.
    /// </summary>
    public enum DWriteTextAlignment : int
    {
        /// <summary>
        /// The leading edge of the paragraph text is aligned to the layout box's leading edge.
        /// </summary>
        Leading,

        /// <summary>
        /// The trailing edge of the paragraph text is aligned to the layout box's trailing edge.
        /// </summary>
        Trailing,

        /// <summary>
        /// The center of the paragraph text is aligned to the center of the layout box.
        /// </summary>
        Center,

        /// <summary>
        /// Align text to the leading side, and also justify text to fill the lines.
        /// </summary>
        Justified
    }

    /// <summary>
    /// Alignment of paragraph text along the flow direction axis relative to the
    /// flow's beginning and ending edge of the layout box.
    /// </summary>
    public enum DWriteParagraphAlignment : int
    {
        /// <summary>
        /// The first line of paragraph is aligned to the flow's beginning edge of the layout box.
        /// </summary>
        Near,

        /// <summary>
        /// The last line of paragraph is aligned to the flow's ending edge of the layout box.
        /// </summary>
        Far,

        /// <summary>
        /// The center of the paragraph is aligned to the center of the flow of the layout box.
        /// </summary>
        Center
    }

    /// <summary>
    /// Word wrapping in multiline paragraph.
    /// </summary>
    public enum DWriteWordWrapping : int
    {
        /// <summary>
        /// Words are broken across lines to avoid text overflowing the layout box.
        /// </summary>
        Wrap = 0,

        /// <summary>
        /// Words are kept within the same line even when it overflows the layout box.
        /// This option is often used with scrolling to reveal overflow text. 
        /// </summary>
        NoWrap = 1,

        /// <summary>
        /// Words are broken across lines to avoid text overflowing the layout box.
        /// Emergency wrapping occurs if the word is larger than the maximum width.
        /// </summary>
        EmergencyBreak = 2,

        /// <summary>
        /// Only wrap whole words, never breaking words (emergency wrapping) when the
        /// layout width is too small for even a single word.
        /// </summary>
        WholeWord = 3,

        /// <summary>
        /// Wrap between any valid characters clusters.
        /// </summary>
        Character = 4,
    }

    /// <summary>
    /// Direction for how reading progresses.
    /// </summary>
    public enum DWriteReadingDirection
    {
        /// <summary>
        /// Reading progresses from left to right.
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// Reading progresses from right to left.
        /// </summary>
        RightToLeft = 1,
        /// <summary>
        /// Reading progresses from top to bottom.
        /// </summary>
        TopToBottom = 2,
        /// <summary>
        /// Reading progresses from bottom to top.
        /// </summary>
        BottomToTop = 3,
    };

    /// <summary>
    /// Direction for how lines of text are placed relative to one another.
    /// </summary>
    public enum DWriteFlowDirection
    {
        /// <summary>
        /// Text lines are placed from top to bottom.
        /// </summary>
        TopToBottom = 0,
        /// <summary>
        /// Text lines are placed from bottom to top.
        /// </summary>
        BottomToTop = 1,
        /// <summary>
        /// Text lines are placed from left to right.
        /// </summary>
        LeftToRight = 2,
        /// <summary>
        /// Text lines are placed from right to left.
        /// </summary>
        RightToLeft = 3,
    }

    public enum DWriteLineSpacingMethod : int
    {
        /// <summary>
        /// Line spacing depends solely on the content, growing to accommodate the size of fonts and inline objects.
        /// </summary>
        Default,

        /// <summary>
        /// Lines are explicitly set to uniform spacing, regardless of contained font sizes.<br/>
        /// This can be useful to avoid the uneven appearance that can occur from font fallback.
        /// </summary>
        Uniform,

        /// <summary>
        /// Line spacing and baseline distances are proportional to the computed values based on the content, the size of the fonts and inline objects.
        /// </summary>
        Proportional
    }

    /// <summary>
    /// Specifies algorithmic style simulations to be applied to the font face.
    /// Bold and oblique simulations can be combined via bitwise OR operation.
    /// </summary>
    [Flags]
    public enum DWriteFontSimulations : ushort
    {
        /// <summary>
        /// No simulations are performed.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Algorithmic emboldening is performed.
        /// </summary>
        Bold = 0x0001,
        /// <summary>
        /// Algorithmic italicization is performed.
        /// </summary>
        Oblique = 0x0002
    }

    /// <summary>
    /// The informational string enumeration identifies a string in a font.
    /// </summary>
    public enum DWriteInformationalStringId : uint
    {
        /// <summary>
        /// Unspecified name ID.
        /// </summary>
        None,
        /// <summary>
        /// Copyright notice provided by the font.
        /// </summary>
        CopyrightNotice,
        /// <summary>
        /// String containing a version number.
        /// </summary>
        VersionStrings,
        /// <summary>
        /// Trademark information provided by the font.
        /// </summary>
        Trademark,
        /// <summary>
        /// Name of the font manufacturer.
        /// </summary>
        Manufacturer,
        /// <summary>
        /// Name of the font designer.
        /// </summary>
        Designer,
        /// <summary>
        /// URL of font designer (with protocol, e.g., http://, ftp://).
        /// </summary>
        DesignerUrl,
        /// <summary>
        /// Description of the font. Can contain revision information, usage recommendations, history, features, etc.
        /// </summary>
        Description,
        /// <summary>
        /// URL of font vendor (with protocol, e.g., http://, ftp://). If a unique serial number is embedded in the URL, it can be used to register the font.
        /// </summary>
        FontVenderUrl,
        /// <summary>
        /// Description of how the font may be legally used, or different example scenarios for licensed use. This field should be written in plain language, not legalese.
        /// </summary>
        LicenseDescription,
        /// <summary>
        /// URL where additional licensing information can be found.
        /// </summary>
        LicenseInfoUrl,
        /// <summary>
        /// GDI-compatible family name. Because GDI allows a maximum of four fonts per family, fonts in the same family may have different GDI-compatible family names
        /// (e.g., "Arial", "Arial Narrow", "Arial Black").
        /// </summary>
        Win32FamilyNames,
        /// <summary>
        /// GDI-compatible subfamily name.
        /// </summary>
        Win32SubfamilyName,
        /// <summary>
        /// Typographic family name preferred by the designer. This enables font designers to group more than four fonts in a single family without losing compatibility with
        /// GDI. This name is typically only present if it differs from the GDI-compatible family name.
        /// </summary>
        TypographicFamilyNames,
        /// <summary>
        /// Typographic subfamily name preferred by the designer. This name is typically only present if it differs from the GDI-compatible subfamily name. 
        /// </summary>
        TypographicSubFamilyNames,
        /// <summary>
        /// Sample text. This can be the font name or any other text that the designer thinks is the best example to display the font in.
        /// </summary>
        SampleText,
        /// <summary>
        /// The full name of the font, e.g. "Arial Bold", from name id 4 in the name table.
        /// </summary>
        FullName,
        /// <summary>
        /// The postscript name of the font, e.g. "GillSans-Bold" from name id 6 in the name table.
        /// </summary>
        PostScriptName,
        /// <summary>
        /// The postscript CID findfont name, from name id 20 in the name table.
        /// </summary>
        PostScriptCIDName,
        /// <summary>
        /// Family name for the weight-stretch-style model.
        /// </summary>
        WeightStretchStyleFamilyName,
        /// <summary>
        /// Script/language tag to identify the scripts or languages that the font was primarily designed to support.<br/>
        /// </summary>
        /// <remarks>      
        /// See <see cref="DWriteFontPropertyId.DesignScriptLanguageTag"/> for a longer description.
        /// </remarks>
        DesignScriptLanguageTag,
        /// <summary>
        /// Script/language tag to identify the scripts or languages that the font declares
        /// it is able to support.
        /// </summary>
        SupportedScriptLanguageTag,

        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        PreferredFamilyNames = TypographicFamilyNames,
        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        PreferredSubFamilyNames = TypographicSubFamilyNames,
        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        WWSFamilyName = WeightStretchStyleFamilyName
    }


    /// <summary>
    /// The font property enumeration identifies a string in a font.
    /// </summary>
    public enum DWriteFontPropertyId
    {
        /// <summary>
        /// Unspecified font property identifier.
        /// </summary>
        None,
        /// <summary>
        /// Family name for the weight-stretch-style model.
        /// </summary>
        WeightStretchStyleFamilyName,
        /// <summary>
        /// Family name preferred by the designer. This enables font designers to group more than four fonts in a single family without losing compatibility with
        /// GDI. This name is typically only present if it differs from the GDI-compatible family name.
        /// </summary>
        TypographicFamilyName,
        /// <summary>
        /// Face name of the for the weight-stretch-style (e.g., Regular or Bold).
        /// </summary>
        WeightStretchStyleFaceName,
        /// <summary>
        /// The full name of the font, e.g. "Arial Bold", from name id 4 in the name table.
        /// </summary>
        FullName,
        /// <summary>
        /// GDI-compatible family name. Because GDI allows a maximum of four fonts per family, fonts in the same family may have different GDI-compatible family names
        /// (e.g., "Arial", "Arial Narrow", "Arial Black").
        /// </summary>
        Win32FamilyName,
        /// <summary>
        /// The postscript name of the font, e.g. "GillSans-Bold" from name id 6 in the name table.
        /// </summary>
        PostScriptName,
        /// <summary>
        /// Script/language tag to identify the scripts or languages that the font was
        /// primarily designed to support.
        /// </summary>
        /// <remarks>
        /// The design script/language tag is meant to be understood from the perspective of
        /// users. For example, a font is considered designed for English if it is considered
        /// useful for English users. <br/>
        /// Note that this is different from what a font might be capable of supporting. 
        /// For example, the Meiryo font was primarily designed for
        /// Japanese users. While it is capable of displaying English well, it was not
        /// meant to be offered for the benefit of non-Japanese-speaking English users. <br/>
        ///<br/>
        /// As another example, a font designed for Chinese may be capable of displaying
        /// Japanese text, but would likely look incorrect to Japanese users.<br/>
        /// <br/>
        /// The valid values for this property are "ScriptLangTag" values. These are adapted
        /// from the IETF BCP 47 specification, "Tags for Identifying Languages" 
        /// (see <see href="http://tools.ietf.org/html/bcp47"/>).<br/>
        /// In a BCP 47 language tag, a language subtag element is mandatory and other subtags are optional. 
        /// In a ScriptLangTag, a script subtag is mandatory and other subtags are option.<br/>
        /// The following augmented BNF syntax, adapted from BCP 47, is used:<br/>
        /// ScriptLangTag = [language "-"] script ["-" region] *("-" variant) *("-" extension) ["-" privateuse]<br/>
        /// <br/>
        /// The expansion of the elements and the intended semantics associated with each
        /// are as defined in BCP 47. Script subtags are taken from ISO 15924. <br/>
        /// At present, no extensions are defined, and any extension should be ignored. Private use
        /// subtags are defined by private agreement between the source and recipient and
        /// may be ignored.<br/>
        /// <br/>
        /// Subtags must be valid for use in BCP 47 and contained in the Language Subtag Registry maintained by IANA. <br/>
        /// (See <seealso href="http://www.iana.org/assignments/language-subtag-registry/language-subtag-registry"/>
        /// and section 3 of BCP 47 for details.)<br/>
        /// <br/>
        /// Any ScriptLangTag value not conforming to these specifications is ignored.<br/>
        /// <br/>
        /// Examples:<br/>
        /// <list type="bullet">
        /// <item>"Latn" denotes Latin script (and any language or writing system using Latin)</item>
        /// <item>"Cyrl" denotes Cyrillic script</item>
        /// <item>
        /// "sr-Cyrl" denotes Cyrillic script as used for writing the Serbian language;
        /// a font that has this property value may not be suitable for displaying
        /// text in Russian or other languages written using Cyrillic script
        /// </item>
        /// <item>"Jpan" denotes Japanese writing (Han + Hiragana + Katakana)</item>
        /// </list><br/>
        /// When passing this property to GetPropertyValues, use the overload which does
        /// not take a language parameter, since this property has no specific language.
        /// </remarks>
        DesignScriptLanguageTag,
        /// <summary>
        /// Script/language tag to identify the scripts or languages that the font declares
        /// it is able to support.
        /// </summary>
        SupportedScriptLanguageTag,
        /// <summary>
        /// Semantic tag to describe the font (e.g. Fancy, Decorative, Handmade, Sans-serif, Swiss, Pixel, Futuristic).
        /// </summary>
        SemanticTag,
        /// <summary>
        /// Weight of the font represented as a decimal string in the range 1-999.
        /// </summary>
        /// <remark>
        /// This enum is discouraged for use with IDWriteFontSetBuilder2 in favor of the more generic font axis
        /// DWRITE_FONT_AXIS_TAG_WEIGHT which supports higher precision and range.
        /// </remark>
        Weight,
        /// <summary>
        /// Stretch of the font represented as a decimal string in the range 1-9.
        /// </summary>
        /// <remark>
        /// This enum is discouraged for use with IDWriteFontSetBuilder2 in favor of the more generic font axis
        /// DWRITE_FONT_AXIS_TAG_WIDTH which supports higher precision and range.
        /// </remark>
        Stretch,
        /// <summary>
        /// Style of the font represented as a decimal string in the range 0-2.
        /// </summary>
        /// <remark>
        /// This enum is discouraged for use with IDWriteFontSetBuilder2 in favor of the more generic font axes
        /// DWRITE_FONT_AXIS_TAG_SLANT and DWRITE_FONT_AXIS_TAG_ITAL.
        /// </remark>
        Style,
        /// <summary>
        /// Face name preferred by the designer. This enables font designers to group more than four fonts in a single
        /// family without losing compatibility with GDI.
        /// </summary>
        TypographicFaceName,
        /// <summary>
        /// Total number of properties for NTDDI_WIN10 (IDWriteFontSet).
        /// </summary>
        /// <remarks>
        /// <see cref="Total"/> cannot be used as a property ID.
        /// </remarks>
        [Obsolete("Total cannot be used as a property ID.")]
        Total = Style + 1,
        /// <summary>
        /// Total number of properties for NTDDI_WIN10_RS3 (IDWriteFontSet1).
        /// </summary>
        TotalRS3 = TypographicFaceName + 1,

        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        PreferredFamilyName = TypographicFamilyName,
        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        FamilyName = WeightStretchStyleFamilyName,
        [Obsolete("Obsolete aliases kept to avoid breaking existing code.")]
        FaceName = WeightStretchStyleFaceName
    }
}
