using System;

using ConcreteUI.Native;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils
{
    public static class Clipboard
    {
        public static unsafe string GetText()
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            IntPtr dataHandle = User32.GetClipboardData(ClipboardFormat.UnicodeText);
            if (dataHandle == IntPtr.Zero)
                return string.Empty;
            char* ptr = (char*)Kernel32.GlobalLock(dataHandle);
            try
            {
                return new string(ptr);
            }
            finally
            {
                Kernel32.GlobalUnlock(dataHandle);
            }
        }

        public static bool HasText()
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            return User32.IsClipboardFormatAvailable(ClipboardFormat.UnicodeText);
        }

        public static unsafe void SetText(string? text)
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            User32.EmptyClipboard();
            if (text is null)
                return;
            int length = text.Length;
            if (length <= 0)
                return;
            nuint byteCount = unchecked((nuint)length + 1) * sizeof(char);
            IntPtr dataHandle = Kernel32.GlobalAlloc(GlobalAllocFlags.Movable, byteCount);
            char* ptr = (char*)Kernel32.GlobalLock(dataHandle);
            try
            {
                fixed (char* source = text)
                    UnsafeHelper.CopyBlock(ptr, source, byteCount);
            }
            finally
            {
                Kernel32.GlobalUnlock(dataHandle);
            }
            User32.SetClipboardData(ClipboardFormat.UnicodeText, dataHandle);
        }
    }


    /// <summary>
    /// Predefined Clipboard Formats
    /// </summary>
    public enum ClipboardFormat : uint
    {
        /// <summary>
        /// Text format.
        /// </summary>
        /// <remarks>
        /// Each line ends with a carriage return/linefeed (<CR-LF>) combination.<br/>
        /// A null character signals the end of the data. Use this format for ANSI text.
        /// </remarks>
        Text = 1,
        /// <summary>
        /// A handle to a bitmap (<b>HBITMAP</b>/<see cref="IntPtr"/>).
        /// </summary>
        Bitmap = 2,
        /// <summary>
        /// Handle to a metafile picture format as defined by the METAFILEPICT structure.
        /// </summary>
        /// <remarks>
        /// When passing a CF_METAFILEPICT handle by means of DDE, the application responsible for deleting hMem should also free the metafile referred to by the CF_METAFILEPICT handle.
        /// </remarks>
        MetafilePicture = 3,
        /// <summary>
        /// Microsoft Symbolic Link (SYLK) format.
        /// </summary>
        SymbolicLink = 4,
        /// <summary>
        /// Software Arts' Data Interchange Format.
        /// </summary>
        Dif = 5,
        /// <summary>
        /// Tagged-image file format (TIFF).
        /// </summary>
        Tiff = 6,
        /// <summary>
        /// Text format containing characters in the OEM character set.
        /// </summary>
        /// <remarks>
        /// Each line ends with a carriage return/linefeed (CR-LF) combination. A null character signals the end of the data.
        /// </remarks>
        OemText = 7,
        /// <summary>
        /// A memory object containing a BITMAPINFO structure followed by the bitmap bits.
        /// </summary>
        Dib = 8,
        /// <summary>
        /// Handle to a color palette.
        /// </summary>
        /// <remarks>
        /// Whenever an application places data in the clipboard that depends on or assumes a color palette, it should place the palette on the clipboard as well.<br/>
        /// If the clipboard contains data in the CF_PALETTE (logical color palette) format, the application should use the SelectPalette and RealizePalette functions to realize (compare) any other data in the clipboard against that logical palette.<br/>
        /// When displaying clipboard data, the clipboard always uses as its current palette any object on the clipboard that is in the CF_PALETTE format.
        /// </remarks>
        Palette = 9,
        /// <summary>
        /// Data for the pen extensions to the Microsoft Windows for Pen Computing.
        /// </summary>
        PenData = 10,
        /// <summary>
        /// Represents audio data more complex than can be represented in a CF_WAVE standard wave format.
        /// </summary>
        Riff = 11,
        /// <summary>
        /// Represents audio data in one of the standard wave formats, such as 11 kHz or 22 kHz PCM.
        /// </summary>
        Wave = 12,
        /// <summary>
        /// Unicode text format.
        /// </summary>
        /// <remarks>
        /// Each line ends with a carriage return/linefeed (CR-LF) combination. A null character signals the end of the data.
        /// </remarks>
        UnicodeText = 13,
        /// <summary>
        /// A handle to an enhanced metafile (HENHMETAFILE).
        /// </summary>
        EnhancedMetafile = 14,
        /// <summary>
        /// A handle to type HDROP that identifies a list of files.
        /// </summary>
        /// <remarks>
        /// An application can retrieve information about the files by passing the handle to the DragQueryFile function.
        /// </remarks>
        HandleOfDrop = 15,
        /// <summary>
        /// The data is a handle (HGLOBAL) to the locale identifier (LCID) associated with text in the clipboard.
        /// </summary>
        /// <remarks>
        /// When you close the clipboard, if it contains CF_TEXT data but no CF_LOCALE data, the system automatically sets the CF_LOCALE format to the current input language. <br/>
        /// You can use the CF_LOCALE format to associate a different locale with the clipboard text.<br/>
        /// An application that pastes text from the clipboard can retrieve this format to determine which character set was used to generate the text.<br/>
        /// Note that the clipboard does not support plain text in multiple character sets. To achieve this, use a formatted text data type such as RTF instead.<br/>
        /// The system uses the code page associated with CF_LOCALE to implicitly convert from CF_TEXT to CF_UNICODETEXT. Therefore, the correct code page table is used for the conversion.
        /// </remarks>
        Locale = 16,
        /// <summary>
        /// A memory object containing a BITMAPV5HEADER structure followed by the bitmap color space information and the bitmap bits.
        /// </summary>
        DibV5 = 17,
        [Obsolete("Internal use only!")]
        _MAX = 18,
        /// <summary>
        /// Owner-display format.
        /// </summary>
        /// <remarks>
        /// The clipboard owner must display and update the clipboard viewer window, and receive the 
        /// <see cref="WindowMessage.AskClipBoardFormatName"/>, 
        /// <see cref="WindowMessage.HScrollClipboard"/>, 
        /// <see cref="WindowMessage.PaintClipboard"/>, 
        /// <see cref="WindowMessage.SizeClipboard"/>, and 
        /// <see cref="WindowMessage.VScrollClipboard"/> messages.<br/>
        /// The hMem parameter must be NULL.
        /// </remarks>
        OwnerDisplay = 0x0080,
        /// <summary>
        /// Text display format associated with a private format.
        /// </summary>
        /// <remarks>
        /// The hMem parameter must be a handle to data that can be displayed in text format in lieu of the privately formatted data.
        /// </remarks>
        PrivateFormatText = 0x0081,
        /// <summary>
        /// Bitmap display format associated with a private format.
        /// </summary>
        /// <remarks>
        /// The hMem parameter must be a handle to data that can be displayed in bitmap format in lieu of the privately formatted data.
        /// </remarks>
        PrivateFormatBitmap = 0x0082,
        /// <summary>
        /// Metafile-picture display format associated with a private format. 
        /// </summary>
        /// <remarks>
        /// The hMem parameter must be a handle to data that can be displayed in metafile-picture format in lieu of the privately formatted data.
        /// </remarks>
        PrivateFormatMetafilePicture = 0x0083,
        /// <summary>
        /// Enhanced metafile display format associated with a private format.
        /// </summary>
        /// <remarks>
        /// The hMem parameter must be a handle to data that can be displayed in enhanced metafile format in lieu of the privately formatted data.
        /// </remarks>
        PrivateEnhancedMetafile = 0x008E,

        /*
         * "Private" formats don't get GlobalFree()'d
         */
        PrivateFirst = 0x0200,
        PrivateLast = 0x02FF,

        /*
         * "GDIOBJ" formats do get DeleteObject()'d
         */
        GdiObjectFirst = 0x0300,
        GdiObjectLast = 0x03FF
    }
}
