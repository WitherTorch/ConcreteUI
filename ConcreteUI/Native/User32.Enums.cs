using System;

namespace ConcreteUI.Native
{
    [Flags]
    public enum WindowPositionFlags
    {
        SwapWithAsyncWindowPos = 0x4000,
        SwapWithDefererase = 0x2000,
        SwapWithDrawFrame = 0x0020,
        SwapWithFrameChanged = 0x0020,
        SwapWithHideWindow = 0x0080,
        SwapWithNoActivate = 0x0010,
        SwapWithNoCopyBits = 0x0100,
        SwapWithNoMove = 0x0002,
        SwapWithNoOwnerZOrder = 0x0200,
        SwapWithNoRedraw = 0x0008,
        SwapWithNoReposition = 0x0200,
        SwapWithNoSendChanging = 0x0400,
        SwapWithNoSize = 0x0001,
        SwapWithNoZOrder = 0x0004,
        SwapWithShowWindow = 0x0040,
        SwapWithChangeWindowState = 0x8000
    }

    internal enum ShowWindowCommands
    {
        Hide = 0,
        ShowNormal = 1,
        Normal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowMaximized = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNA = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimize = 11
    }

    internal enum SystemMetric
    {
        SM_CXSCREEN = 0,  // 0x00
        SM_CYSCREEN = 1,  // 0x01
        SM_CXVSCROLL = 2,  // 0x02
        SM_CYHSCROLL = 3,  // 0x03
        SM_CYCAPTION = 4,  // 0x04
        SM_CXBORDER = 5,  // 0x05
        SM_CYBORDER = 6,  // 0x06
        SM_CXDLGFRAME = 7,  // 0x07
        SM_CXFIXEDFRAME = 7,  // 0x07
        SM_CYDLGFRAME = 8,  // 0x08
        SM_CYFIXEDFRAME = 8,  // 0x08
        SM_CYVTHUMB = 9,  // 0x09
        SM_CXHTHUMB = 10, // 0x0A
        SM_CXICON = 11, // 0x0B
        SM_CYICON = 12, // 0x0C
        SM_CXCURSOR = 13, // 0x0D
        SM_CYCURSOR = 14, // 0x0E
        SM_CYMENU = 15, // 0x0F
        SM_CXFULLSCREEN = 16, // 0x10
        SM_CYFULLSCREEN = 17, // 0x11
        SM_CYKANJIWINDOW = 18, // 0x12
        SM_MOUSEPRESENT = 19, // 0x13
        SM_CYVSCROLL = 20, // 0x14
        SM_CXHSCROLL = 21, // 0x15
        SM_DEBUG = 22, // 0x16
        SM_SWAPBUTTON = 23, // 0x17
        SM_CXMIN = 28, // 0x1C
        SM_CYMIN = 29, // 0x1D
        SM_CXSIZE = 30, // 0x1E
        SM_CYSIZE = 31, // 0x1F
        SM_CXSIZEFRAME = 32, // 0x20
        SM_CXFRAME = 32, // 0x20
        SM_CYSIZEFRAME = 33, // 0x21
        SM_CYFRAME = 33, // 0x21
        SM_CXMINTRACK = 34, // 0x22
        SM_CYMINTRACK = 35, // 0x23
        SM_CXDOUBLECLK = 36, // 0x24
        SM_CYDOUBLECLK = 37, // 0x25
        SM_CXICONSPACING = 38, // 0x26
        SM_CYICONSPACING = 39, // 0x27
        SM_MENUDROPALIGNMENT = 40, // 0x28
        SM_PENWINDOWS = 41, // 0x29
        SM_DBCSENABLED = 42, // 0x2A
        SM_CMOUSEBUTTONS = 43, // 0x2B
        SM_SECURE = 44, // 0x2C
        SM_CXEDGE = 45, // 0x2D
        SM_CYEDGE = 46, // 0x2E
        SM_CXMINSPACING = 47, // 0x2F
        SM_CYMINSPACING = 48, // 0x30
        SM_CXSMICON = 49, // 0x31
        SM_CYSMICON = 50, // 0x32
        SM_CYSMCAPTION = 51, // 0x33
        SM_CXSMSIZE = 52, // 0x34
        SM_CYSMSIZE = 53, // 0x35
        SM_CXMENUSIZE = 54, // 0x36
        SM_CYMENUSIZE = 55, // 0x37
        SM_ARRANGE = 56, // 0x38
        SM_CXMINIMIZED = 57, // 0x39
        SM_CYMINIMIZED = 58, // 0x3A
        SM_CXMAXTRACK = 59, // 0x3B
        SM_CYMAXTRACK = 60, // 0x3C
        SM_CXMAXIMIZED = 61, // 0x3D
        SM_CYMAXIMIZED = 62, // 0x3E
        SM_NETWORK = 63, // 0x3F
        SM_CLEANBOOT = 67, // 0x43
        SM_CXDRAG = 68, // 0x44
        SM_CYDRAG = 69, // 0x45
        SM_SHOWSOUNDS = 70, // 0x46
        SM_CXMENUCHECK = 71, // 0x47
        SM_CYMENUCHECK = 72, // 0x48
        SM_SLOWMACHINE = 73, // 0x49
        SM_MIDEASTENABLED = 74, // 0x4A
        SM_MOUSEWHEELPRESENT = 75, // 0x4B
        SM_XVIRTUALSCREEN = 76, // 0x4C
        SM_YVIRTUALSCREEN = 77, // 0x4D
        SM_CXVIRTUALSCREEN = 78, // 0x4E
        SM_CYVIRTUALSCREEN = 79, // 0x4F
        SM_CMONITORS = 80, // 0x50
        SM_SAMEDISPLAYFORMAT = 81, // 0x51
        SM_IMMENABLED = 82, // 0x52
        SM_CXFOCUSBORDER = 83, // 0x53
        SM_CYFOCUSBORDER = 84, // 0x54
        SM_TABLETPC = 86, // 0x56
        SM_MEDIACENTER = 87, // 0x57
        SM_STARTER = 88, // 0x58
        SM_SERVERR2 = 89, // 0x59
        SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
        SM_CXPADDEDBORDER = 92, // 0x5C
        SM_DIGITIZER = 94, // 0x5E
        SM_MAXIMUMTOUCHES = 95, // 0x5F
        SM_REMOTESESSION = 0x1000, // 0x1000
        SM_SHUTTINGDOWN = 0x2000, // 0x2000
        SM_REMOTECONTROL = 0x2001, // 0x2001
        SM_CONVERTIBLESLATEMODE = 0x2003,
        SM_SYSTEMDOCKED = 0x2004,
    }

    internal enum WindowCompositionAttribute
    {
        Undefined = 0,
        NCRenderingEnabled = 1,
        NCRenderingPolicy = 2,
        TransitionsForceDisabled = 3,
        AllowNCPaint = 4,
        CaptionButtonBounds = 5,
        NonClientRTLLayout = 6,
        ForceIconicRepresentation = 7,
        ExtendedFrameBounds = 8,
        HasIconicBitmap = 9,
        ThemeAttributes = 10,
        NcrenderingExiled = 11,
        NCAdornmentInfo = 12,
        ExcludedFromLivePreview = 13,
        VideoOverlayActive = 14,
        ForceActiveWindowAppearance = 15,
        DisallowPeek = 16,
        Cloak = 17,
        Cloaked = 18,
        AccentPolicy = 19,
        FreezeRepresentation = 20,
        EverUncloaked = 21,
        VisualOwner = 22,
        Holographic = 23,
        ExcludedFromDDA = 24,
        PassiveUpdateMode = 25,
        UseDarkModeColors = 26
    }

    internal enum AccentState
    {
        Disabled = 0,
        EnableGradient = 1,
        EnableTransparentGradient = 2,
        EnableBlurBehind = 3,
        EnableAcrylicBlurBehind = 4, // RS4 1803
        EnableHostBackdrop = 5, // RS5 1809
    }

    [Flags]
    internal enum AccentFlags
    {
        // ... 
        None = 0x0,
        DrawLeftBorder = 0x20,
        DrawTopBorder = 0x40,
        DrawRightBorder = 0x80,
        DrawBottomBorder = 0x100,
        DrawAllBorders = DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder
        // ... 
    }

    [Flags]
    internal enum ClassStyles : int
    {
        None = 0,
        ByteAlignClient = 0x1000,
        ByteAlignWindow = 0x2000,
        ClassDC = 0x0040,
        DoubleClicks = 0x0008,
        DropShadow = 0x00020000,
        GlobalClass = 0x4000,
        HRedraw = 0x0002,
        NoClose = 0x0200,
        OwnDC = 0x0020,
        ParentDC = 0x0080,
        SaveBits = 0x0800,
        VRedraw = 0x0001
    }

    [Flags]
    internal enum LoadOrCopyImageOptions : uint
    {
        DefaultColor = 0x00000000,
        Monochrome = 0x00000001,
        Color = 0x00000002,
        CopyReturnOrginal = 0x00000004,
        CopyDeleteOriginal = 0x00000008,
        LoadFromFile = 0x00000010,
        LoadTransparent = 0x00000020,
        DefaultSize = 0x00000040,
        VgaColor = 0x00000080,
        LoadMap3dColors = 0x00001000,
        CreateDibSection = 0x00002000,
        CopyFromResource = 0x00004000,
        Shared = 0x00008000
    }

    [Flags]
    internal enum GetMonitorFlags : uint
    {
        DefaultToNull = 0x00000000,
        DefaultToPrimary = 0x00000001,
        DefaultToNearest = 0x00000002
    }

    internal enum GetWindowCommand : uint
    {
        HwndFirst = 0,
        HwndLast = 1,
        HwndNext = 2,
        HwndPrevious = 3,
        Owner = 4,
        Child = 5,
        EnabledPopup = 6,
    }

    /// <summary>
    /// Queue status flags for GetQueueStatus() and MsgWaitForMultipleObjects()
    /// </summary>
    [Flags]
    internal enum QueueStatusFlags : uint
    {
        Key = 0x0001,
        MouseMove = 0x0002,
        MouseButton = 0x0004,
        PostMessage = 0x0008,
        Timer = 0x0010,
        Paint = 0x0020,
        SendMessage = 0x0040,
        HotKey = 0x0080,
        AllPostMessage = 0x0100,
        RawInput = 0x0400,
        Touch = 0x0800,
        Pointer = 0x1000,
        Mouse = MouseMove | MouseButton,
        Input = Mouse | Key | RawInput | Touch | Pointer,
        InputOld = Mouse | Key | RawInput,
        AllEvents = Input | PostMessage | Timer | Paint | HotKey,
        AllEventsOld = InputOld | PostMessage | Timer | Paint | HotKey,
        AllInput = Input | PostMessage | Timer | Paint | HotKey | SendMessage,
        AllInputOld = InputOld | PostMessage | Timer | Paint | HotKey | SendMessage
    }

    /// <summary>
    /// PeekMessage() Options
    /// </summary>
    [Flags]
    internal enum PeekMessageOptions : uint
    {
        NoRemove = 0x0000,
        Remove = 0x0001,
        NoYield = 0x0002,
        QueuedInput = QueueStatusFlags.Input << 16,
        QueuedPostMessage = (QueueStatusFlags.PostMessage | QueueStatusFlags.HotKey | QueueStatusFlags.Timer) << 16,
        QueuedPaint = QueueStatusFlags.Paint << 16,
        QueuedSendMessage = QueueStatusFlags.SendMessage << 16
    }
}
