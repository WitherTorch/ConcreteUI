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

    public enum ShowWindowCommands
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

    public enum SystemMetric
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

    public enum WindowCompositionAttribute
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

    public enum AccentState
    {
        Disabled = 0,
        EnableGradient = 1,
        EnableTransparentGradient = 2,
        EnableBlurBehind = 3,
        EnableAcrylicBlurBehind = 4, // RS4 1803
        EnableHostBackdrop = 5, // RS5 1809
    }

    [Flags]
    public enum AccentFlags
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
    public enum WindowStyles : uint
    {
        Border = 0x800000,
        Caption = 0xc00000,
        Child = 0x40000000,
        ClipChildren = 0x2000000,
        ClipSiblings = 0x4000000,
        Disabled = 0x8000000,
        DialogFrame = 0x400000,
        Group = 0x20000,
        HScroll = 0x100000,
        Maximize = 0x1000000,
        MaximizeBox = 0x10000,
        Minimize = 0x20000000,
        MinimizeBox = 0x20000,
        Overlapped = 0x0,
        OverlappedWindow = Overlapped | Caption | SystemMenu | SizeFrame | MinimizeBox | MaximizeBox,
        Popup = 0x80000000,
        PopupWindow = Popup | Border | SystemMenu,
        SizeFrame = 0x40000,
        SystemMenu = 0x80000,
        TabStop = 0x10000,
        Visible = 0x10000000,
        VScroll = 0x200000
    }

    [Flags]
    public enum ClassStyles : int
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

    public enum HitTestValue : int
    {
        Error = -2,
        Transparent = -1,
        NoWhere = 0,
        Client = 1,
        Caption = 2,
        SysMenu = 3,
        Growbox = 4,
        Menu = 5,
        HScroll = 6,
        VScroll = 7,
        MinimizeButton = 8,
        MaximizeButton = 9,
        LeftBorder = 10,
        RightBorder = 11,
        TopBorder = 12,
        TopLeftBorder = 13,
        TopRightBorder = 14,
        BottomBorder = 15,
        BottomLeftBorder = 16,
        BottomRightBorder = 17,
        NormalBorder = 18,
        Object = 19,
        CloseButton = 20,
        Help = 21
    }

    public enum WindowMessage : int
    {
        Null = 0x0000,
        Create = 0x0001,
        Destroy = 0x0002,
        Move = 0x0003,
        Size = 0x0005,
        Activate = 0x0006,
        SetFocus = 0x0007,
        KillFocus = 0x0008,
        Enable = 0x000A,
        SetRedraw = 0x000B,
        SetText = 0x000C,
        GetText = 0x000D,
        GetTextLength = 0x000E,
        Paint = 0x000F,
        Close = 0x0010,
        QueryEndSession = 0x0011,
        QueryOpen = 0x0013,
        EndSession = 0x0016,
        Quit = 0x0012,
        EraseBackground = 0x0014,
        SystemColorChange = 0x0015,
        ShowWindow = 0x0018,
        WinIniChange = 0x001A,
        SettingChange = WinIniChange,
        DeviceModeChange = 0x001B,
        ActivateApp = 0x001C,
        FontChange = 0x001D,
        TimeChange = 0x001E,
        CancelMode = 0x001F,
        SetCursor = 0x0020,
        MouseActivate = 0x0021,
        ChildActivate = 0x0022,
        QueueSync = 0x0023,
        GetMinMaxInfo = 0x0024,
        PaintIcon = 0x0026,
        IconEraseBackground = 0x0027,
        NextDialogControl = 0x0028,
        SpoolerStatus = 0x002A,
        DrawItem = 0x002B,
        MeasureItem = 0x002C,
        DeleteItem = 0x002D,
        VKeyToItem = 0x002E,
        CharToItem = 0x002F,
        SetFont = 0x0030,
        GetFont = 0x0031,
        SetHotKey = 0x0032,
        GetHotKey = 0x0033,
        QueryDragIcon = 0x0037,
        CompareItem = 0x0039,
        GetObject = 0x003D,
        Compacting = 0x0041,
        WindowPositionChanging = 0x0046,
        WindowPositionChanged = 0x0047,
        Power = 0x0048,
        CopyData = 0x004A,
        CancelJournal = 0x004B,
        Notify = 0x004E,
        InputLanguageChangeRequest = 0x0050,
        InputLanguageChange = 0x0051,
        TCard = 0x0052,
        Help = 0x0053,
        UserChanged = 0x0054,
        NotifyFormat = 0x0055,
        ContextMenu = 0x007B,
        StyleChanging = 0x007C,
        StyleChanged = 0x007D,
        DisplayChange = 0x007E,
        GetIcon = 0x007F,
        SetIcon = 0x0080,
        NCCreate = 0x0081,
        NCDestroy = 0x0082,
        NCCalcSize = 0x0083,
        NCHitTest = 0x0084,
        NCPaint = 0x0085,
        NCActivate = 0x0086,
        GetDialogCode = 0x0087,
        SyncPaint = 0x0088,

        NCMouseMove = 0x00A0,
        NCLeftButtonDown = 0x00A1,
        NCLeftButtonUp = 0x00A2,
        NCLeftButtonDoubleClick = 0x00A3,
        NCRightButtonDown = 0x00A4,
        NCRightButtonUp = 0x00A5,
        NCRightButtonDoubleClick = 0x00A6,
        NCMiddleButtonDown = 0x00A7,
        NCMiddleButtonUp = 0x00A8,
        NCMiddleButtonDoubleClick = 0x00A9,
        NCExtraButtonDown = 0x00AB,
        NCExtraButtonUp = 0x00AC,
        NCExtraButtonDoubleClick = 0x00AD,

        InputDeviceChange = 0x00FE,
        Input = 0x00FF,

        KeyDown = 0x0100,
        KeyUp = 0x0101,
        Char = 0x0102,
        DeadChar = 0x0103,
        SystemKeyDown = 0x0104,
        SystemKeyUp = 0x0105,
        SystemChar = 0x0106,
        SystemDeadChar = 0x0107,
        UniChar = 0x0109,

        ImeStartComposition = 0x010D,
        ImeEndComposition = 0x010E,
        ImeComposition = 0x010F,
        InitDialog = 0x0110,
        Command = 0x0111,
        SystemCommand = 0x0112,
        Timer = 0x0113,
        HScroll = 0x0114,
        VScroll = 0x0115,
        InitializeMenu = 0x0116,
        InitMenuPopup = 0x0117,
        MenuSelect = 0x011F,
        MenuChar = 0x0120,
        EnterIdle = 0x0121,
        MenuRightButtonUp = 0x0122,
        MenuDrag = 0x0123,
        MenuGetObject = 0x0124,
        UnInitMenuPopup = 0x0125,
        MenuCommand = 0x0126,

        ChangeUIState = 0x0127,
        UpdateUIState = 0x0128,
        QueryUIState = 0x0129,

        ControlColorForMessageBox = 0x0132,
        ControlColorForEdit = 0x0133,
        ControlColorForListBox = 0x0134,
        ControlColorFoRightButton = 0x0135,
        ControlColorForDialog = 0x0136,
        ControlColorForScrollBar = 0x0137,
        ControlColorForStatic = 0x0138,
        GetHandleOfMenu = 0x01E1,

        MouseMove = 0x0200,
        LeftButtonDown = 0x0201,
        LeftButtonUp = 0x0202,
        LeftButtonDoubleClick = 0x0203,
        RightButtonDown = 0x0204,
        RightButtonUp = 0x0205,
        RightButtonDoubleClick = 0x0206,
        MiddleButtonDown = 0x0207,
        MiddleButtonUp = 0x0208,
        MiddleButtonDoubleClick = 0x0209,
        MouseWheel = 0x020A,
        ExtraButtonDown = 0x020B,
        ExtraButtonUp = 0x020C,
        ExtraButtonDoubleClick = 0x020D,
        MouseHWheel = 0x020E,

        ParentNotify = 0x0210,
        EnterMenuLoop = 0x0211,
        ExitMenuLoop = 0x0212,

        NextMenu = 0x0213,
        Sizing = 0x0214,
        CaptureChanged = 0x0215,
        Moving = 0x0216,

        PowerBroadcast = 0x0218,

        DeviceChange = 0x0219,

        MdiCreate = 0x0220,
        MdiDestroy = 0x0221,
        MdiActivate = 0x0222,
        MdiRestore = 0x0223,
        MdiNext = 0x0224,
        MdiMaximize = 0x0225,
        MdiTile = 0x0226,
        MdiCascade = 0x0227,
        MdiIconArrange = 0x0228,
        MdiGetActive = 0x0229,
        MdiSetmenu = 0x0230,

        EnterSizeMove = 0x0231,
        ExitSizeMove = 0x0232,
        Dropfiles = 0x0233,
        MdiRefreshMenu = 0x0234,

        ImeSetContext = 0x0281,
        ImeNotify = 0x0282,
        ImeControl = 0x0283,
        ImeCompositionFull = 0x0284,
        ImeSelect = 0x0285,
        ImeChar = 0x0286,
        ImeRequest = 0x0288,
        ImeKeyDown = 0x0290,
        ImeKeyUp = 0x0291,

        MouseHover = 0x02A1,
        MouseLeave = 0x02A3,
        NCMouseHover = 0x02A0,
        NCMouseLeave = 0x02A2,

        WTSSessionChange = 0x02B1,

        TabletFirst = 0x02c0,
        TabletAdded = TabletFirst + 8,
        TabletDeleted = TabletFirst + 9,
        TabletFlick = TabletFirst + 11,
        TabletQuerySystemGestureStatus = TabletFirst + 12,
        TabletLast = TabletFirst + 20,

        DpiChanged = 0x02E0,

        Cut = 0x0300,
        Copy = 0x0301,
        Paste = 0x0302,
        Clear = 0x0303,
        Undo = 0x0304,
        RenderFormat = 0x0305,
        RenderAllFormats = 0x0306,
        DestroyClipboard = 0x0307,
        DrawClipboard = 0x0308,
        PaintClipboard = 0x0309,
        VScrollClipboard = 0x030A,
        SizeClipboard = 0x030B,
        AskClipBoardFormatName = 0x030C,
        ChangeClipBoardChain = 0x030D,
        HScrollClipboard = 0x030E,
        QueryNewPalette = 0x030F,
        PaletteIsChanging = 0x0310,
        PaletteChanged = 0x0311,
        Hotkey = 0x0312,

        Print = 0x0317,
        PrintClient = 0x0318,

        AppCommand = 0x0319,

        ThemeChanged = 0x031A,

        ClipboardUpdate = 0x031D,

        DwmCompositionChanged = 0x031E,
        DwmNCRenderingChanged = 0x031F,
        DwmColorizationColorChanged = 0x0320,
        DwmWindowMaximizedChange = 0x0321,

        GetTitleBarInfoEx = 0x033F,

        CustomClassMessageStart = 0x0400,
        MFCReflect = CustomClassMessageStart + 0x1C00,

        AppDefinedMessageStart = 0x8000,
        RegisterWindowMessageStart = 0xC000,
    }

    [Flags]
    public enum MessageBoxFlags : uint
    {
        Ok = 0x00000000U,
        OkCancel = 0x00000001U,
        AbortRetryIgnore = 0x00000002U,
        YesNoCancel = 0x00000003U,
        YesNo = 0x00000004U,
        RetryCancel = 0x00000005U,
        CancelRetryContinue = 0x00000006U,

        IconHand = 0x00000010U,
        IconQuestion = 0x00000020U,
        IconExclamation = 0x00000030U,
        IconAsterisk = 0x00000040U,

        UserIcon = 0x00000080U,
        IconWarning = IconExclamation,
        IconError = IconHand,

        IconInformation = IconAsterisk,
        IconStop = IconHand,

        DefaultButton1 = 0x00000000U,
        DefaultButton2 = 0x00000100U,
        DefaultButton3 = 0x00000200U,
        DefaultButton4 = 0x00000300U,
        ApplicationModel = 0x00000000U,
        SystemModel = 0x00001000U,
        TaskModel = 0x00002000U,
        HelpButton = 0x00004000U,

        NoFocus = 0x00008000U,
        SetForeground = 0x00010000U,
        DefaultDesktopOnly = 0x00020000U,

        TopMost = 0x00040000U,
        Right = 0x00080000U,
        RTLReading = 0x00100000U,

        ServiceNotification = 0x00200000U,
        ServiceNotificationNt3x = 0x00040000U
    }

    public enum DialogCommandId : uint
    {
        Invalid = 0,
        Ok = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
        Close = 8,
        Help = 9,
        TryAgain = 10,
        Continue = 11,
        Timeout = 32000
    }
}
